using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Persistence.Sql;

public class MySaga
    : SqlSaga<MySagaData>
        , IAmStartedByMessages<StartSaga>
        , IHandleMessages<Response>

{
    protected override string CorrelationPropertyName => nameof(MySagaData.TheCorrellationId);

    protected override void ConfigureMapping(IMessagePropertyMapper mapper)
    {
        mapper.ConfigureMapping<StartSaga>(m => m.TheCorrellationId);
        mapper.ConfigureMapping<Response>(m => m.TheCorrellationId);
    }

    public async Task Handle(StartSaga message, IMessageHandlerContext context)
    {
        await context.SendLocal(new DoTask { TheCorrellationId = Data.TheCorrellationId });
    }

    public async Task Handle(Response message, IMessageHandlerContext context)
    {
        Data.Ack = true;
        NServiceBus.Logging.LogManager.GetLogger<MySaga>().Info("Ack response");
        //MarkAsComplete();
    }
}
