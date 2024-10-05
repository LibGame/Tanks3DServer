using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tanks3DServer.GameSessionScripts.GameDTO
{
    internal class TankActiveBonusRequest
    {
        public string playerID;
        public string gameSession;
        public BonusData bonusData;
    }
}
