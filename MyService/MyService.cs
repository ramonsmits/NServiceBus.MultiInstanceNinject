using System;
using System.Threading.Tasks;

public class MyService : IMyService
{
    public async Task ExpandTheGalaxy()
    {
        await Console.Out.WriteLineAsync("Galaxy is been expanded!");
    }
}