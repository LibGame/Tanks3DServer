using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tanks3DServer.Database;

namespace Tanks3DServer.LobbyScripts.LobbyDTO
{
    internal class BuyTankResponse
    {
        public int buyedTankID;
        public int userID;
        public User user;
        public bool result;
    }
}
