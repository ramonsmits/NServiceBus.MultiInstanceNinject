using NServiceBus;

public class Response : IMessage
{
    public string TheCorrellationId { get; set; }
}
