using Autofac;
using EnglishWordsServer.AutofacConfiig.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tanks3DServer.LobbyScripts;

namespace Tanks3DServer.Installers
{
    internal class LobbyInstaller : IRegisterInstalls
    {
        public void RegisterInstall(ContainerBuilder builder)
        {
            builder.RegisterType<LobbyHandler>().SingleInstance();
            builder.RegisterType<LobbyModel>().SingleInstance();
        }
    }
}
