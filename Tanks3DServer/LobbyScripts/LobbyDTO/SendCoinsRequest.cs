using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tanks3DServer.LobbyScripts.LobbyDTO
{
    internal class SendCoinsRequest
    {
        public string walletTarget;
        public string participantID;
        public int userID;
        public decimal amount;
    }
}
