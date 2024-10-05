using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tanks3DServer.LobbyScripts.LobbyDTO
{
    internal class BuyTankRequest
    {
        public int userID;
        public int tankID;
        public string participantID;
        public decimal price;
    }
}
