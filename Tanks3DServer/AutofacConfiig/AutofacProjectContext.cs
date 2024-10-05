using Autofac;
using EnglishWordsServer.AutofacConfiig.Interface;
using Tanks3DServer.Installers;


namespace EnglishWordsServer.AutofacConfiig
{
    internal class AutofacProjectContext
    {

        public static IContainer Container { get; private set; }


        private List<IRegisterInstalls> _bindings = new List<IRegisterInstalls>()
        {
            new LobbyInstaller(),
            new ServerTCPInstaller(),
            new GameInstaller(),
            new DatabaseInstaller(),
            new MoiraiInstaller(),
        };


        public void RegisterInstalls()
        {
            ContainerBuilder builder = new ContainerBuilder(); 

            foreach(var binding in _bindings)
            {
                binding.RegisterInstall(builder);
            }
            Container = builder.Build();
        }

    }
}
