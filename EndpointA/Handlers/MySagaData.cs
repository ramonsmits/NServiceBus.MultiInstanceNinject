using NServiceBus;

public class MySagaData : ContainSagaData
{
    public string TheCorrellationId { get; set; }
    public bool Ack { get; set; }
}
