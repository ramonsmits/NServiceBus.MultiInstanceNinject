using System;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;

namespace NServiceBus.MultiHostNinject
{
    class Program
    {
        static async Task Main()
        {
            InitLogging();

            var e1 = await Create("EndpointA", new[] { "EndpointB" });
            var e2 = await Create("EndpointB", new[] { "EndpointA" });

            await Console.Out.WriteLineAsync("Press ESC to exit, any other key to send message");

            while (Console.ReadKey().Key != ConsoleKey.Escape)
            {
                await Console.Out.WriteLineAsync("Sending message!");
                await e1.SendLocal(new StartSaga { TheCorrellationId = DateTime.UtcNow.ToString("s") });
            }

            await e1.Stop();
            await e2.Stop();
        }

        static async Task<IEndpointInstance> Create(string name, string[] asmExclusions)
        {
            using (NDC.Push(name))
            {
                var cfg = new EndpointConfiguration(name);
                var transport = cfg.UseTransport<LearningTransport>();
                var routing = transport.Routing();
                routing.RouteToEndpoint(typeof(StartSaga), "EndpointA");
                routing.RouteToEndpoint(typeof(Request), "EndpointB");

                cfg.UsePersistence<LearningPersistence>();

                var scanner = cfg.AssemblyScanner();
                scanner.ExcludeAssemblies(asmExclusions);

                var pipeline = cfg.Pipeline;
                pipeline.Register(behavior: new AssignMessageIdtoLog4netNdcBehavior(name), description: "Assigns the incoming message id to the log4net NDC.");

                cfg.RegisterComponents(x => x.ConfigureComponent<IMyService>(() => new MyService(), DependencyLifecycle.SingleInstance));
                cfg.EnableInstallers();

                return await Endpoint.Start(cfg);
            }
        }

        static void InitLogging()
        {
            var layout = new PatternLayout
            {
                //ConversionPattern = "%d [%t] %-5p %c [%x] - %m%n"
                ConversionPattern = "%date [%-3thread] %-5level %logger [%ndc] - %message%newline"
            };
            layout.ActivateOptions();
            var consoleAppender = new ManagedColoredConsoleAppender
            {
                Threshold = Level.Debug,
                Layout = layout,

            };
            consoleAppender.AddMapping(new ManagedColoredConsoleAppender.LevelColors { ForeColor = ConsoleColor.Red, BackColor = ConsoleColor.White, Level = Level.Fatal });
            consoleAppender.AddMapping(new ManagedColoredConsoleAppender.LevelColors { ForeColor = ConsoleColor.Red, Level = Level.Error });
            consoleAppender.AddMapping(new ManagedColoredConsoleAppender.LevelColors { ForeColor = ConsoleColor.Yellow, Level = Level.Warn });
            consoleAppender.AddMapping(new ManagedColoredConsoleAppender.LevelColors { ForeColor = ConsoleColor.Green, Level = Level.Info });
            consoleAppender.AddMapping(new ManagedColoredConsoleAppender.LevelColors { ForeColor = ConsoleColor.White, Level = Level.Debug });
            consoleAppender.AddMapping(new ManagedColoredConsoleAppender.LevelColors { ForeColor = ConsoleColor.Gray, Level = Level.Verbose });

            consoleAppender.ActivateOptions();

            var executingAssembly = Assembly.GetExecutingAssembly();
            var repository = log4net.LogManager.GetRepository(executingAssembly);
            BasicConfigurator.Configure(repository, consoleAppender);

            NServiceBus.Logging.LogManager.Use<Log4NetFactory>();
        }
    }
}


