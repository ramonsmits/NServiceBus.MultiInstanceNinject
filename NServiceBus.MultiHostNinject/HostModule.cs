using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using log4net;
using Ninject;
using Ninject.Extensions.ChildKernel;
using Ninject.Modules;
using NServiceBus;
using NServiceBus.Persistence.Sql;

class HostModule : NinjectModule
{
    public override void Load()
    {
        Bind<IMyService>().To<MyService>().InSingletonScope();
        Bind<IStartableEndpoint>().ToMethod(x => Create("EndpointA", new[] { "EndpointB" }, new ChildKernel(x.Kernel)).GetAwaiter().GetResult());
        Bind<IStartableEndpoint>().ToMethod(x => Create("EndpointB", new[] { "EndpointA" }, new ChildKernel(x.Kernel)).GetAwaiter().GetResult());
    }

    static async Task<IStartableEndpoint> Create(string name, string[] asmExclusions, IKernel childKernel)
    {
        using (LogicalThreadContext.Stacks["NDC"].Push(name))
        {
            var cfg = new EndpointConfiguration(name);
            var transport = cfg.UseTransport<RabbitMQTransport>();
            transport.ConnectionString("host=localhost");

            var routing = transport.Routing();
            routing.RouteToEndpoint(typeof(StartSaga), "EndpointA");
            routing.RouteToEndpoint(typeof(Request), "EndpointB");

            //cfg.UsePersistence<LearningPersistence>();
            var persistence = cfg.UsePersistence<SqlPersistence>();
            persistence.SubscriptionSettings().DisableCache();
            persistence.SqlDialect<SqlDialect.MsSqlServer>();
            persistence.ConnectionBuilder(() => new SqlConnection("server=.;Integrated Security=True;database=MultiHostNinject;App=MultiHostNinject"));

            var scanner = cfg.AssemblyScanner();
            scanner.ExcludeAssemblies(asmExclusions);

            var pipeline = cfg.Pipeline;
            pipeline.Register(behavior: new AssignMessageIdtoLog4netNdcBehavior(), description: "Assigns the incoming message id to the log4net NDC.");

            // == Passed child container
            cfg.UseContainer<NinjectBuilder>(customizations: customizations => { customizations.ExistingKernel(childKernel); });

            cfg.EnableInstallers();
            if (string.Equals("EndpointA", name, StringComparison.InvariantCulture)) cfg.EnableOutbox();

            return await Endpoint.Create(cfg).ConfigureAwait(false);
        }
    }
}