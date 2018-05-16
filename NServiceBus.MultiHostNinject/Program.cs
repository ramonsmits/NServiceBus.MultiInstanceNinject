using log4net;
using Topshelf;
using Topshelf.Ninject;

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
                x.UseNinject(new HostModule());
                x.Service<EndpointsControl>(s =>
                {
                    s.ConstructUsingNinject();
                    s.WhenStarted((i, hostControl) => i.Start(hostControl));
                    s.WhenStopped((i, hostControl) => i.Stop(hostControl));
                });
                x.SetServiceName("MultiHostNinject");
                x.OnException(ex => LogManager.GetLogger("Host").Fatal("OnException", ex));
            });
        }
        finally
        {
            LogManager.Shutdown();
        }
    }
}