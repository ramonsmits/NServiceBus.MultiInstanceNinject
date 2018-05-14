using System.Threading.Tasks;
using NServiceBus;

public class MySaga
    : Saga<MySagaData>
        , IAmStartedByMessages<StartSaga>
        , IHandleMessages<Response>

{
    protected override void ConfigureHowToFindSaga(SagaPropertyMapper<MySagaData> mapper)
    {
        mapper.ConfigureMapping<StartSaga>(m => m.TheCorrellationId).ToSaga(s => s.TheCorrellationId);
        mapper.ConfigureMapping<Response>(m => m.TheCorrellationId).ToSaga(s => s.TheCorrellationId);
    }

    public async Task Handle(StartSaga message, IMessageHandlerContext context)
    {
        await context.SendLocal(new DoTask { TheCorrellationId = Data.TheCorrellationId });
    }

    public async Task Handle(Response message, IMessageHandlerContext context)
    {
        Data.Ack = true;
        //MarkAsComplete();
    }
}