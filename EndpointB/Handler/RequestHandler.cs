using System.Threading.Tasks;
using NServiceBus;

public class RequestHandler : IHandleMessages<Request>
{
    IMyService service;
    public RequestHandler(IMyService service)
    {
        this.service = service;
    }

    public async Task Handle(Request message, IMessageHandlerContext context)
    {
        await service.ExpandTheGalaxy();
        await context.Reply(new Response { TheCorrellationId = message.TheCorrellationId });
    }
}