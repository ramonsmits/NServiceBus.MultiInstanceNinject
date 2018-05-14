using System;
using System.Threading.Tasks;
using NServiceBus;

public class TaskHandler : IHandleMessages<DoTask>
{
    IMyService service;
    public TaskHandler(IMyService service)
    {
        this.service = service;
    }

    public async Task Handle(DoTask message, IMessageHandlerContext context)
    {
        await service.ExpandTheGalaxy();
        var o = new SendOptions();
        o.DelayDeliveryWith(TimeSpan.FromSeconds(10));
        await context.Send(new Request { TheCorrellationId = message.TheCorrellationId }, o);
    }
}