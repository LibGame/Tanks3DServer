using Newtonsoft.Json;
using Tanks3DServer.ServerTCPScripts;

namespace Tanks3DServer.LobbyScripts
{
    public class LobbySessionParticipant
    {
        public short ordinal;
        public short tankID;
        public string lobbyParticipantID;
        public bool isReady = false;
        public int bid;
        public string name;

        [JsonIgnore] public WebSocketBehavior WebSocket;

        public Command command;

        public LobbySessionParticipant(string lobbyParticipantID, WebSocketBehavior webSocket, string name)
        {
            WebSocket = webSocket;
            this.lobbyParticipantID = lobbyParticipantID;
            this.name = name;
        }
    }
}
