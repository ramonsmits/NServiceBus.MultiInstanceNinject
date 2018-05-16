using System;
using System.Threading.Tasks;
using log4net;
using NServiceBus.Pipeline;

/// <summary>
/// Pushes the current message ID to the log4net NDC stack. When using a log4net conversation pattern containing `%ndc` then this value used ("%date [%-3thread] %-5level [%ndc] %logger - %message%newline").
/// </summary>
/// <example>
/// var pipeline = endpointConfiguration.Pipeline;
/// pipeline.Register(behavior: new AssignMessageIdtoLog4netNdcBehavior(), description: "Assigns the incoming message id to the log4net NDC.");
/// </example>
class AssignMessageIdtoLog4netNdcBehavior : Behavior<ITransportReceiveContext>
{
    public override async Task Invoke(ITransportReceiveContext context, Func<Task> next)
    {
        // Do not use NDC.Push, not compatible with async/await
        using (LogicalThreadContext.Stacks["NDC"].Push(context.Message.MessageId))
        {
            await next().ConfigureAwait(false);
        }
    }
}