using Autofac;
using EnglishWordsServer.AutofacConfiig.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tanks3DServer.LobbyScripts;
using Tanks3DServer.ServerTCPScripts;

namespace Tanks3DServer.Installers
{
    internal class ServerTCPInstaller : IRegisterInstalls
    {
        public void RegisterInstall(ContainerBuilder builder)
        {
            builder.RegisterType<ServerTCP>().SingleInstance();
        }
    }
}
