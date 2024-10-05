using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tanks3DServer.Database;

namespace Tanks3DServer.LobbyScripts.LobbyDTO
{
    internal class RegisterResponse
    {
        public string id;
        public AuthResultType AuthResultType;
        public User User;
    }
}
