using NServiceBus;

public class DoTask : ICommand
{
    public string TheCorrellationId { get; internal set; }
}