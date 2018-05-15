﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Ninject;
using Ninject.Extensions.ChildKernel;
using NServiceBus;
using NServiceBus.Persistence.Sql;
using Topshelf;

class EndpointsControl : ServiceControl
{
    ICollection<IEndpointInstance> instances;
    IKernel parentKernel;

    public async Task Start()
    {
        parentKernel = new StandardKernel();
        parentKernel.Bind<IMyService>().To<MyService>().InSingletonScope();

        var tasks = new List<Task<IEndpointInstance>>
        {
            Create("EndpointA", new[] {"EndpointB"}, new ChildKernel(parentKernel)),
            Create("EndpointB", new[] {"EndpointA"}, new ChildKernel(parentKernel))
        };

        instances = await Task.WhenAll(tasks).ConfigureAwait(false);

        ConsoleLoop(instances.First());
    }

    async void ConsoleLoop(IEndpointInstance instance)
    {
        while (true)
        {
            if (Environment.UserInteractive)
                Console.ReadKey();
            else
                await Task.Delay(5000).ConfigureAwait(false);
            await instance.Send(new StartSaga {TheCorrellationId = Guid.NewGuid().ToString("N")}).ConfigureAwait(false);
        }
    }

    public async Task Stop()
    {
        var tasks = new List<Task>();
        foreach (var i in instances) tasks.Add(i.Stop());
        await Task.WhenAll(tasks).ConfigureAwait(false);

        parentKernel.Dispose();
    }

    static async Task<IEndpointInstance> Create(string name, string[] asmExclusions, IKernel childKernel)
    {
        using (NDC.Push(name))
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
            if (string.Equals("EndpointA", name, StringComparison.InvariantCulture)) cfg.EnableOutbox();

            return await Endpoint.Start(cfg);
        }
    }

    public bool Start(HostControl hostControl)
    {
        Start().GetAwaiter().GetResult();
        return true;
    }

    public bool Stop(HostControl hostControl)
    {
        Stop().GetAwaiter().GetResult();
        return true;
    }
}
class Program
{
    static int Main()
    {
        LoggingHelper.InitLogging();
        try
        {
            return (int)HostFactory.Run(x =>
            {
                x.UseLog4Net();
                x.Service<EndpointsControl>();
                x.SetServiceName("MultiHostNinject");
                x.OnException(ex => LogManager.GetLogger("Host").Fatal("OnException", ex));
            });
        }
        finally
        {
            log4net.LogManager.Shutdown();
        }
    }
}


