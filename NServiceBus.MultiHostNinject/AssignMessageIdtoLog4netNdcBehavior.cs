using System;
using System.Threading.Tasks;
using log4net;
using NServiceBus.Pipeline;

class AssignMessageIdtoLog4netNdcBehavior : Behavior<ITransportReceiveContext>
{
    string name;
    public AssignMessageIdtoLog4netNdcBehavior(string name)
    {
        this.name = name;
    }

    public override async Task Invoke(ITransportReceiveContext context, Func<Task> next)
    {
        using (NDC.Push(name + ":" + context.Message.MessageId))
        {
            await next().ConfigureAwait(false);
        }
    }
}