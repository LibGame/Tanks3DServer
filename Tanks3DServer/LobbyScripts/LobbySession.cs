using Autofac;
using EnglishWordsServer.AutofacConfiig;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tanks3DServer.LobbyScripts
{
    internal class LobbySession
    {
        public string lobbySessionID;
        public string lobbyName;
        public string ownerName;
        public string password;
        public short maxPlayerCount;
        public short playerCount;
        public LobbyMode lobbyMode;
        public int minBid;
        public int timeRemaining = -1; // Время в секундах
        public List<LobbySessionParticipant> participants = new List<LobbySessionParticipant>();
        public bool isStartedGame = false;
        public bool addToRed = true;
        [JsonIgnore] private LobbyModel _lobbyModel;
        [JsonIgnore] private CancellationTokenSource _lobbyTimerCancellationToken;

        public IEnumerable<LobbySessionParticipant> Participants => participants;

        public LobbySession(string lobbySession, string lobbyName, string ownerName, short maxPlayerCount, short playerCount, LobbyMode lobbyMode,
            string password, int minBid)
        {
            _lobbyModel = AutofacProjectContext.Container.Resolve<LobbyModel>();
            this.lobbySessionID = lobbySession;
            this.lobbyName = lobbyName;
            this.ownerName = ownerName;
            this.maxPlayerCount = maxPlayerCount;
            this.playerCount = playerCount;
            this.lobbyMode = lobbyMode;
            this.password = password;
            this.minBid = minBid;
        }

        public void StartLobbyTimer()
        {
            _lobbyTimerCancellationToken = new CancellationTokenSource();
            _ = LobbyTimer(60, _lobbyTimerCancellationToken.Token);
        }

        public void StopLobbyTimer()
        {
            if(_lobbyTimerCancellationToken != null)
                _lobbyTimerCancellationToken.Cancel();
            Console.WriteLine("StopLobbyTimer");
        }

        public async Task LobbyTimer(int time, CancellationToken cancellationToken)
        {
            timeRemaining = time;

            while (timeRemaining > 0)
            {
                await Task.Delay(1000, cancellationToken); // Прерываем задержку, если токен отменен

                if (cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine("Lobby timer canceled");
                    break;
                }

                if (isStartedGame)
                    break;

                timeRemaining--;
                Console.WriteLine($"Time remaining: {timeRemaining} seconds");
            }

            if (isStartedGame)
                return;

            if (timeRemaining <= 0)
            {
                Console.WriteLine("Lobby expired");
                StartLobby();
            }
        }

        public void StartLobby()
        {
            isStartedGame = true;
            _lobbyModel.StartGameSession(this);
        }

        public void SetParticipants(IEnumerable<LobbySessionParticipant> participants)
        {
            this.participants = participants.ToList();
        }

        public void RemoveBySessionID(string participantID)
        {
            LobbySessionParticipant participant = participants.FirstOrDefault(x => x.lobbyParticipantID == participantID);
            if(participant != null)
            {
                participants.Remove(participant);
            }
        }

        public void Remove(LobbySessionParticipant participant)
        {
            this.participants.Remove(participant);
        }

        public void AddParticipant(LobbySessionParticipant participant)
        {
            participants.Add(participant);
        }
    }
}
