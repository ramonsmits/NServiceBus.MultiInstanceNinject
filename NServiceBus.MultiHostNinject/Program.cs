using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using log4net;
using Ninject;
using Ninject.Extensions.ChildKernel;
using NServiceBus;
using NServiceBus.Persistence.Sql;

class Program
{
    static async Task Main()
    {
        LoggingHelper.InitLogging();

        var parentKernel = new StandardKernel();
        parentKernel.Bind<IMyService>().To<MyService>().InSingletonScope();

        var e1 = await Create("EndpointA", new[] { "EndpointB" }, new ChildKernel(parentKernel));
        var e2 = await Create("EndpointB", new[] { "EndpointA" }, new ChildKernel(parentKernel));

        await Console.Out.WriteLineAsync("Press ESC to exit, any other key to send message");

        while (Console.ReadKey().Key != ConsoleKey.Escape)
        {
            await Console.Out.WriteLineAsync("Sending message!");
            await e1.SendLocal(new StartSaga { TheCorrellationId = DateTime.UtcNow.ToString("s") });
        }

        await e1.Stop();
        await e2.Stop();
    }

    static async Task<IEndpointInstance> Create(string name, string[] asmExclusions, IKernel childKernel)
    {
        using (NDC.Push(name))
        {
            var cfg = new EndpointConfiguration(name);
            var transport = cfg.UseTransport<LearningTransport>();
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
            pipeline.Register(behavior: new AssignMessageIdtoLog4netNdcBehavior(name), description: "Assigns the incoming message id to the log4net NDC.");

            // == Ninject container per instance
            //var kernel = new StandardKernel();
            //kernel.Bind<IMyService>().ToConstant(new MyService());
            //cfg.UseContainer<NinjectBuilder>(customizations: customizations => { customizations.ExistingKernel(kernel); });

            // == Passed child container
            cfg.UseContainer<NinjectBuilder>(customizations: customizations => { customizations.ExistingKernel(childKernel); });

            // == Default DI container
            //cfg.RegisterComponents(x => x.ConfigureComponent<IMyService>(() => new MyService(), DependencyLifecycle.SingleInstance));
            cfg.EnableInstallers();

            return await Endpoint.Start(cfg);
        }
    }
}