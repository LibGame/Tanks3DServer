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
using Tanks3DServer.Utilities;

namespace Tanks3DServer.GameSessionScripts
{
    internal class GameSession
    {
        public string sessionID;
        public List<GameParticipant> participants = new List<GameParticipant>();
        public List<BonusData> bonusDatas = new List<BonusData>();
        public LobbyMode lobbyMode;
        [JsonIgnore] private List<int> points = new List<int>();
        [JsonIgnore] private List<int> bonuses = new List<int>();
        private Random rng = new Random();
        public int sumBid;
        public bool isEndSession;

        public void InitGameSession()
        {
            for(int i = 0; i < 25; i++)
            {
                points.Add(i);
            }
            for (int i = 0; i < 5; i++)
            {
                bonuses.Add(i);
            }
            GenerateBonuses();
        }

        public void RemoveParticipantByWebSocket(WebSocketUDP ip)
        {
            participants = participants.Where(x => x.WebSocketUDP != ip).ToList();
        }

        public void UpdateDeleteAndCreateBonus(BonusData bonusData)
        {
            bonusDatas = bonusDatas.Where(x => x.uniqBonusID != bonusData.uniqBonusID).ToList();
            var pointsForGenerate = new List<int>(points);
            foreach(var bonus in bonusDatas)
            {
                pointsForGenerate.Remove(bonus.bonusPoint);
            }
            BonusData bonusData1 = new BonusData();
            bonusData1.bonusPoint = pointsForGenerate[rng.Next(0, pointsForGenerate.Count - 1)];
            bonusData1.bonusType = bonuses[rng.Next(0, bonuses.Count - 1)];
            bonusData1.uniqBonusID = Guid.NewGuid().ToString();
            bonusDatas.Add(bonusData1);
        }

        public void GenerateBonuses()
        {
            var pointsForGenerate = new List<int>(points);
            pointsForGenerate.Shuffle();
            List<BonusData> bonusDatas = new List<BonusData>();

            for (int x = 0; x < 5; x++)
            {
                BonusData bonusData = new BonusData();
                bonusData.bonusPoint = pointsForGenerate[x];
                bonusData.bonusType = bonuses[rng.Next(0, bonuses.Count - 1)];
                bonusData.uniqBonusID = Guid.NewGuid().ToString();
                bonusDatas.Add(bonusData);
            }
            this.bonusDatas = bonusDatas;
        }

    }
}
