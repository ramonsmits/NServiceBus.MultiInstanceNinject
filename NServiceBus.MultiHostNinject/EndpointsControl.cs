using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ninject;
using NServiceBus;
using Topshelf;

class EndpointsControl : ServiceControl
{
    ICollection<IEndpointInstance> instances;
    readonly IStartableEndpoint[] endpoints;

    public EndpointsControl(Task<IStartableEndpoint>[] endpoints)
    {
        this.endpoints =  Task.WhenAll(endpoints).GetAwaiter().GetResult();
    }

    async Task Start()
    {
        instances = await Task.WhenAll(endpoints.Select(i => i.Start())).ConfigureAwait(false);
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

    async Task Stop()
    {
        await Task.WhenAll(instances.Select(i => i.Stop())).ConfigureAwait(false);
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
