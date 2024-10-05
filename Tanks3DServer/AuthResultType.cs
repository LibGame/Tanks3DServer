using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tanks3DServer
{
    public enum AuthResultType
    {
        None,
        Registred,
        Logined,
        BusyMail,
        BusyUsername,
        WrongPassword,
        AccoundNotFound
    }
}
