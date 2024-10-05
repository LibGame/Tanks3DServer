using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Tanks3DServer.ServerTCPScripts;

namespace Tanks3DServer.LobbyScripts
{
    internal class Participant
    {
        [JsonIgnore] public WebSocketBehavior WebSocket { get; private set; }
        public string Name { get; private set; }
        public string Id { get; private set; }
        public string LobbySessionParticipant { get; set; }
        public Participant(WebSocketBehavior tcpClient, string name)
        {
            WebSocket = tcpClient;
            Name = name;
            Id = Guid.NewGuid().ToString();
        }
    }
}
