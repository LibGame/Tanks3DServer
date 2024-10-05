using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tanks3DServer.GameSessionScripts.GameDTO
{
    internal class ConnectToGameSessionResponse
    {
        public bool result;
        public List<BonusData> bonusDatas = new List<BonusData>();
        public GameParticipant participant;
    }
}
