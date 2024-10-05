using System.Collections;

namespace Tanks3DServer.LobbyScripts.LobbyDTO
{
    public class LobbyChatMessageRequest 
    {
        public string lobbyID;
        public string participantID;
        public string participantName;
        public string message;
    }
}