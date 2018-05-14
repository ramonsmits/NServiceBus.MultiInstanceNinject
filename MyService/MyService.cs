using System;
using System.Threading;
using System.Threading.Tasks;
using log4net;

public class MyService : IMyService
{
    static long count;
    long instance = Interlocked.Increment(ref count);

    public MyService()
    {
        LogManager.GetLogger("MyService").WarnFormat("MyService instance {0} is created", instance);
    }

    public async Task ExpandTheGalaxy()
    {
        LogManager.GetLogger("MyService").WarnFormat("Galaxy is been expanded via instance {0}!", instance);
    }
}