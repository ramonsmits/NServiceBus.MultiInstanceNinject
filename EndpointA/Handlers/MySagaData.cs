using NServiceBus;

public class MySagaData : ContainSagaData
{
    public string TheCorrellationId { get; set; }
}
