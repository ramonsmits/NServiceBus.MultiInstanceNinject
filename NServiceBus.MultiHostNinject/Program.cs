using log4net;
using Topshelf;

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
            LogManager.Shutdown();
        }
    }
}