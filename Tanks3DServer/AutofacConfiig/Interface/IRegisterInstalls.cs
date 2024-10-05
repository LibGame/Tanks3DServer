using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishWordsServer.AutofacConfiig.Interface
{
    internal interface IRegisterInstalls
    {
        public void RegisterInstall(ContainerBuilder builder);
    }
}
