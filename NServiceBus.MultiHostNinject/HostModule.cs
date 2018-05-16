using Ninject.Modules;

class HostModule : NinjectModule
{
    public override void Load()
    {
        Bind<IMyService>().To<MyService>().InSingletonScope();
    }
}