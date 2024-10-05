using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tanks3DServer.LobbyScripts;

namespace Tanks3DServer.GameSessionScripts.GameDTO
{
    internal class ConnectToGameSessionRequest
    {
        public string gameSession;
        public string playerID;
        public Command command;
        public int bid;
        public int userID;
    }
}
