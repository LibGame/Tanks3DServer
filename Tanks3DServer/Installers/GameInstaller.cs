using Autofac;
using EnglishWordsServer.AutofacConfiig.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tanks3DServer.GameSessionScripts;
using Tanks3DServer.LobbyScripts;
using Tanks3DServer.ServerUDPScripts;

namespace Tanks3DServer.Installers
{
    internal class GameInstaller : IRegisterInstalls
    {
        public void RegisterInstall(ContainerBuilder builder)
        {
            builder.RegisterType<GameController>().SingleInstance();
            builder.RegisterType<GameHandler>().SingleInstance();
            builder.RegisterType<ServerUDP>().SingleInstance();
        }
    }
}
