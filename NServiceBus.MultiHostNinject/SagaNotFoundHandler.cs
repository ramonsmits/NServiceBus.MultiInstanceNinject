using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Sagas;

class SagaNotFoundHandler : IHandleSagaNotFound
{
    public Task Handle(object message, IMessageProcessingContext context)
    {
        throw new Exception("Saga not found");
    }
}
