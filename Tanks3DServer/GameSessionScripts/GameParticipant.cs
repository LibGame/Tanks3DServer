using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Tanks3DServer.LobbyScripts;
using Tanks3DServer.ServerTCPScripts;
using Tanks3DServer.ServerUDPScripts;

namespace Tanks3DServer.GameSessionScripts
{
    internal class GameParticipant
    {
        public string id;
        public int userID;
        public Command command;
        public string playerName;
        public int bid;
        public int score;
        [JsonIgnore] public WebSocketUDP WebSocketUDP;
        [JsonIgnore] public bool isDeath;
    }
}
