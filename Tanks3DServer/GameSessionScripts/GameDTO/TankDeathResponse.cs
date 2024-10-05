using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tanks3DServer.GameSessionScripts.GameDTO
{
    internal class TankDeathResponse
    {
        public string playerID;
        public string attakerTankID;
        public int score;
    }
}
