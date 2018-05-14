using NServiceBus;

public class Request : IMessage
{
    public string TheCorrellationId { get; set; }
}