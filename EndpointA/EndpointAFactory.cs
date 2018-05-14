using System.Threading.Tasks;
using NServiceBus;

public class EndpointAFactory
{
    public async Task<IEndpointInstance> Create()
    {
        var cfg = new EndpointConfiguration("A");
        cfg.EnableInstallers();
        //cfg.UseContainer<NinjectBuilder>(
        //    customizations: customizations =>
        //    {
        //        customizations.ExistingKernel(kernel);
        //    });
        cfg.UsePersistence<LearningPersistence>();
        var t = cfg.UseTransport<LearningTransport>();
        var r = t.Routing();
        r.RouteToEndpoint(typeof(Request), "B");
        return await Endpoint.Start(cfg);
    }
}
