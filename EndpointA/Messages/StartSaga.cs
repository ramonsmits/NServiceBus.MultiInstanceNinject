using NServiceBus;

public class StartSaga : ICommand
{
    public string TheCorrellationId { get; set; }
}