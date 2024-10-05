using Autofac;
using EnglishWordsServer.AutofacConfiig.Interface;
using Tanks3DServer.MoiraiScripts;


namespace Tanks3DServer.Installers
{
    internal class MoiraiInstaller : IRegisterInstalls
    {
        public void RegisterInstall(ContainerBuilder builder)
        {
            builder.RegisterType<MoiraiService>().SingleInstance();
        }
    }
}
