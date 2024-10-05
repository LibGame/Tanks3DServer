using Tanks3DServer.Database;
using Tanks3DServer.MoiraiScripts;

namespace Tanks3DServer.LobbyScripts.LobbyDTO
{
    internal class GetProfileResponse
    {
        public List<Transaction> transactions;
        public User user;
        public bool result;
    }
}
