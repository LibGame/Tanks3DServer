using Autofac;
using Azure;
using EnglishWordsServer.AutofacConfiig;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Xml.Linq;
using Tanks3DServer.Database;
using Tanks3DServer.DTO;
using Tanks3DServer.GameSessionScripts;
using Tanks3DServer.Handlers;
using Tanks3DServer.LobbyScripts.LobbyDTO;
using Tanks3DServer.MoiraiScripts;
using Tanks3DServer.ServerTCPScripts;
using WebSocketSharp;
using static System.Runtime.InteropServices.JavaScript.JSType;
using WebSocketBehavior = Tanks3DServer.ServerTCPScripts.WebSocketBehavior;

namespace Tanks3DServer.LobbyScripts
{
    internal class LobbyModel
    {
        private GameController _gameController;
        private DatabaseService _databaseService;
        private MoiraiService _moiraiService;
        private List<LobbySession> _sessions = new List<LobbySession>();
        private List<Participant> _participants = new List<Participant>();

        public LobbyModel()
        {
            _gameController = AutofacProjectContext.Container.Resolve<GameController>();
            _databaseService = AutofacProjectContext.Container.Resolve<DatabaseService>();
            _moiraiService = AutofacProjectContext.Container.Resolve<MoiraiService>();
        }

        public async Task GetUpdatedWallet(UpdateWalletRequest request, WebSocketBehavior webSocket)
        {
            bool result = true;
            User user = await _databaseService.GetUserAsync(request.userID);
            List<Transaction> processedTransactions = JsonConvert.DeserializeObject<List<Transaction>>(user.ProccesedTransactionsJSon);
            List<Transaction> transactions = new List<Transaction>();
            List<Transaction> fullTransactions = new List<Transaction>();

            if (user != null)
            {
                fullTransactions = await _moiraiService.GetTransactions(user.WalletAddres, true);
                transactions = fullTransactions.TakeLast(5).ToList();
                decimal startBalance = user.Balance;

                if (processedTransactions == null)
                    processedTransactions = new List<Transaction>();
                foreach (var transaction in fullTransactions)
                {
                    if (!processedTransactions.Contains(transaction))
                    {

                        if (transaction.Category == "receive")
                        {
                            user.Balance += Math.Abs(transaction.Amount);
                        }
                        //else if (transaction.Category == "send")
                        //{
                        //    user.Balance -= Math.Abs(transaction.Amount);
                        //}
                        Console.WriteLine("user.Balance " + user.Balance);

                        processedTransactions.Add(transaction);
                    }
                }
                if (!startBalance.Equals(user.Balance))
                {
                    user.ProccesedTransactionsJSon = JsonConvert.SerializeObject(processedTransactions);
                }
            }
            else
            {
                result = false;
            }
            UpdateWalletResponse response = new UpdateWalletResponse();

            response.user = user;
            response.transactions = transactions;
            response.result = result;
            string data = JsonConvert.SerializeObject(response);
            Message message = new Message(Handlers.HandlerTypes.Lobby, (int)LobbyHandlersType.UpdateWalletResponse, "", data);
            string dataSend = JsonConvert.SerializeObject(message);

            _ = webSocket.SendMessageAsync(dataSend);
        }

        public async Task BuyTank(BuyTankRequest request , WebSocketBehavior WebSocket)
        {
            BuyTankResponse response = new BuyTankResponse();

            User user = await _databaseService.BuyTank(request.userID , request.tankID, request.price);

            if(user != null)
            {
                response.user = user;
                response.result = true;
                response.userID = request.userID;
                response.buyedTankID = request.tankID;
            }
            else
            {
                response.result = false;
            }

            string data = JsonConvert.SerializeObject(response);
            Message message = new Message(Handlers.HandlerTypes.Lobby, (int)LobbyHandlersType.BuyTankResponse, "", data);
            string dataSend = JsonConvert.SerializeObject(message);

            _ = WebSocket.SendMessageAsync(dataSend);
        }

        public void RemoveParticipant(WebSocketBehavior WebSocket)
        {
            Participant participant = _participants.FirstOrDefault(x => x.WebSocket.Equals(WebSocket));
            if(participant != null)
            {
                _participants.Remove(participant);
                if (!string.IsNullOrEmpty(participant.LobbySessionParticipant))
                {
                    if (TryGetLobbySession(participant.LobbySessionParticipant, out LobbySession session))
                    {

                        session.RemoveBySessionID(participant.Id);

                        if (session.participants.Count() <= 0)
                        {
                            session.StopLobbyTimer();
                            _sessions.Remove(session);
                        }
                        UpdateLobby(session);
                    }
                }
            }
        }

        public async void UpdateLobby(LobbySession lobbySession)
        {
            foreach(var participantLobby in lobbySession.Participants)
            {
                if(TryGetParticipant(participantLobby.lobbyParticipantID, out Participant participant))
                {
                    UpdateLobbyResponse updateLobbyResponse = new UpdateLobbyResponse();
                    updateLobbyResponse.participants = lobbySession.Participants.ToList();
                    string data = JsonConvert.SerializeObject(updateLobbyResponse);
                    Message message = new Message(Handlers.HandlerTypes.Lobby, (int)LobbyHandlersType.UpdateLobbyResponse, "", data);
                    string dataSend = JsonConvert.SerializeObject(message);

                    participant.WebSocket.SendMessageAsync(dataSend);
                }
            }
        }

        public void AddParticipant(Participant participant)
        {
            Console.WriteLine("AddParticipant");
            _participants.Add(participant);
        }

        public async Task SendMessageToLobbyChat(LobbyChatMessageRequest request, WebSocketBehavior WebSocket)
        {
            var lobbySession = _sessions.FirstOrDefault(session => session.lobbySessionID == request.lobbyID);
            foreach (var item in lobbySession.Participants)
            {
                LobbyChatMessageResponse response = new LobbyChatMessageResponse();
                response.participantID = request.participantID;
                response.participantName = request.participantName;
                response.message = request.message;
                string data = JsonConvert.SerializeObject(response);
                Message message = new Message(Handlers.HandlerTypes.Lobby, (int)LobbyHandlersType.LobbyChatMessageResponse, "", data);
                string dataSend = JsonConvert.SerializeObject(message);
                await item.WebSocket.SendMessageAsync(dataSend);

            }
        }

        public async void LeaveLobbySession(LeaveLobbyRequest leaveLobbyRequest, ServerTCPScripts.WebSocketBehavior WebSocket)
        {
            try
            {
                var lobbySession = _sessions.FirstOrDefault(session => session.lobbySessionID == leaveLobbyRequest.lobbySession);
                bool result = false;
                if (lobbySession != null)
                {
                    LobbySessionParticipant lobbySessionParticipant = lobbySession.
                        Participants.FirstOrDefault(participant => participant.lobbyParticipantID == leaveLobbyRequest.participantID);

                    if (lobbySessionParticipant != null)
                    {
                        if (TryGetParticipant(lobbySessionParticipant.lobbyParticipantID, out Participant participant1))
                        {
                            participant1.LobbySessionParticipant = "";
                            _participants.Add(participant1);
                        }
                        lobbySession.Remove(lobbySessionParticipant);
                        lobbySession.playerCount--;

                        LeaveLobbyResponse leaveLobbyResponse = new LeaveLobbyResponse();
                        string dataSend = "";

                        if (lobbySession.Participants.Count() <= 0)
                        {
                            _sessions.Remove(lobbySession);
                            lobbySession.StopLobbyTimer();
                            result = true;
                            leaveLobbyResponse.pariticipant = leaveLobbyRequest.participantID;
                            leaveLobbyResponse.lobbySession = lobbySession;
                            leaveLobbyResponse.result = result;
                            string data = JsonConvert.SerializeObject(leaveLobbyResponse);
                            Message message = new Message(Handlers.HandlerTypes.Lobby, (int)LobbyHandlersType.LeaveLobbyResponse, "", data);
                            dataSend = JsonConvert.SerializeObject(message);
                        }
                        else
                        {
                            leaveLobbyResponse.pariticipant = leaveLobbyRequest.participantID;
                            leaveLobbyResponse.lobbySession = lobbySession;
                            leaveLobbyResponse.result = result;
                            string data = JsonConvert.SerializeObject(leaveLobbyResponse);
                            Message message = new Message(Handlers.HandlerTypes.Lobby, (int)LobbyHandlersType.LeaveLobbyResponse, "", data);
                            dataSend = JsonConvert.SerializeObject(message);
                            foreach (var participant in _participants)
                            {
                                _ = participant.WebSocket.SendMessageAsync(dataSend);
                            }
                            lobbySession.SetParticipants(DistrebuteParticipantsInLobby(lobbySession, lobbySession.Participants.ToList()));
                            result = true;
                        }

                        _ = WebSocket.SendMessageAsync(dataSend);
                    }
                }

                foreach (var item in _participants)
                {
                    GetLobbySessionResponse getLobbySessionResponse = new GetLobbySessionResponse();
                    getLobbySessionResponse.sessions = _sessions;
                    string data1 = JsonConvert.SerializeObject(getLobbySessionResponse);
                    Message message1 = new Message(Handlers.HandlerTypes.Lobby, (int)LobbyHandlersType.GetLobbySessionResponse, "", data1);
                    string dataSend1 = JsonConvert.SerializeObject(message1);

                    _ = item.WebSocket.SendMessageAsync(dataSend1);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private bool TryGetParticipant(string id , out Participant participant)
        {
            participant = _participants.FirstOrDefault(x => x.Id == id);
            if(participant == null)
            {
                return false;
            }
            return true;
        }

        private bool TryGetParticipantInLobbySession(string id, LobbySession lobbySession, out LobbySessionParticipant participant)
        {
            participant = lobbySession.Participants.FirstOrDefault(x => x.lobbyParticipantID == id);
            if (participant == null)
            {
                return false;
            }
            return true;
        }

        private bool TryGetLobbySession(string id, out LobbySession lobbySession)
        {
            lobbySession = _sessions.FirstOrDefault(x => x.lobbySessionID == id);
            if (lobbySession == null)
            {
                return false;
            }
            return true;
        }

        public async Task SendCoins(SendCoinsRequest request, WebSocketBehavior webSocket)
        {
            SendCoinsResponse response = new SendCoinsResponse();
            bool result = false;
            string txid = await _moiraiService.Send(request.walletTarget.Trim(), request.amount);
            Console.WriteLine("SendCoins 1 " +  txid);
            if (txid != "error")
            {
                Console.WriteLine("SendCoins 2");
                User user = await _databaseService.GetUserAsync(request.userID);
                Console.WriteLine("SendCoins 3");
  
                Console.WriteLine("SendCoins 6");
                var transactions = await _moiraiService.GetTransactions(user.WalletAddres, true);
                response.transactions = transactions.TakeLast(5).ToList();
                Console.WriteLine("SendCoins 7");

                if (user != null)
                {
                    Console.WriteLine("SendCoins 8");

                    if (user.Balance > request.amount)
                    {
                        Console.WriteLine("SendCoins 9");

                        await _databaseService.SubtractCurrencyAsync(request.userID, request.amount);
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
                else
                {
                    result = false;
                }
            }
            else
            {
                result = false;
            }
            Console.WriteLine("SendCoins 10");

            response.result = result;
            User user1 = await _databaseService.GetUserAsync(request.userID);
            Console.WriteLine("SendCoins 11");
            response.user = user1;

            string data = JsonConvert.SerializeObject(response);
            Message message = new Message(Handlers.HandlerTypes.Lobby, (int)LobbyHandlersType.SendCoinsResponse, "", data);
            string dataSend = JsonConvert.SerializeObject(message);

            _ = webSocket.SendMessageAsync(dataSend);
        }

        public async Task GetProfile(GetProfileRequest getProfileRequest, WebSocketBehavior webSocket)
        {
            User user = await _databaseService.GetUserAsync(getProfileRequest.userID);
            bool result = false;
            List<Transaction> processedTransactions = JsonConvert.DeserializeObject<List<Transaction>>(user.ProccesedTransactionsJSon);
            List<Transaction> transactions = new List<Transaction>();
            List<Transaction> fullTransactions = new List<Transaction>();

            if (user != null)
            {
                result = true;
                fullTransactions = await _moiraiService.GetTransactions(user.WalletAddres, true);
                transactions = fullTransactions.TakeLast(5).ToList();
            }
            decimal startBalance = user.Balance;

            if (processedTransactions == null)
                processedTransactions = new List<Transaction>();
            foreach (var transaction in fullTransactions)
            {
                if (!processedTransactions.Contains(transaction))
                {

                    if (transaction.Category == "receive")
                    {
                        user.Balance += Math.Abs(transaction.Amount);
                    }
                    //else if (transaction.Category == "send")
                    //{
                    //    user.Balance -= Math.Abs(transaction.Amount);
                    //}
                    Console.WriteLine("user.Balance " + user.Balance);

                    processedTransactions.Add(transaction);
                }
            }
            if (!startBalance.Equals(user.Balance))
            {
                user.ProccesedTransactionsJSon = JsonConvert.SerializeObject(processedTransactions);
            }
            GetProfileResponse getLobbySessionResponse = new GetProfileResponse();

            getLobbySessionResponse.user = user;
            getLobbySessionResponse.result = result;
            getLobbySessionResponse.transactions = transactions;

            string data = JsonConvert.SerializeObject(getLobbySessionResponse);
            Message message = new Message(Handlers.HandlerTypes.Lobby, (int)LobbyHandlersType.GetProfileResponse, "", data);
            string dataSend = JsonConvert.SerializeObject(message);

            _ = webSocket.SendMessageAsync(dataSend);
        }

        public async void ChangeTank(ChangeTankRequest request)
        {
            try
            {
                ChangeTankResponse changeTankResponse = new ChangeTankResponse();
                bool result = false;
                if (TryGetLobbySession(request.lobbySession, out LobbySession lobbySession))
                {
                    LobbySessionParticipant participant = lobbySession.participants.FirstOrDefault(x => x.lobbyParticipantID == request.lobbyParticipantID);

                    if (participant != null)
                    {
                        //changeTankResponse.lobbyParticipantID = participant.lobbyParticipantID;
                        //changeTankResponse.tankID = request.tankID;
                        participant.tankID = request.tankID;
                        result = true;
                    }
                    changeTankResponse.participants = lobbySession.participants;
                }
                changeTankResponse.result = result;
                string data = JsonConvert.SerializeObject(changeTankResponse);
                foreach (var item in lobbySession.Participants)
                {
                    Message message = new Message(Handlers.HandlerTypes.Lobby, (int)LobbyHandlersType.ChangeTankResponse, "", data);
                    string dataSend = JsonConvert.SerializeObject(message);
                    if (TryGetParticipant(item.lobbyParticipantID, out Participant participant))
                    {
                        participant.WebSocket.SendMessageAsync(dataSend);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async void GetLobbySession(GetLobbySessionsRequest getLobbySessionsRequest, WebSocketBehavior WebSocket)
        {
            try
            {
                GetLobbySessionResponse getLobbySessionResponse = new GetLobbySessionResponse();
                getLobbySessionResponse.sessions = _sessions;
                string data = JsonConvert.SerializeObject(getLobbySessionResponse);
                Message message = new Message(Handlers.HandlerTypes.Lobby, (int)LobbyHandlersType.GetLobbySessionResponse, "", data);
                string dataSend = JsonConvert.SerializeObject(message);

                WebSocket.SendMessageAsync(dataSend);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private List<LobbySessionParticipant> DistrebuteParticipantsInLobby(LobbySession lobbySession ,List<LobbySessionParticipant> lobbySessionParticipants)
        {
            try
            {
                if(lobbySession.lobbyMode == LobbyMode.Solo)
                {
                    for(int i = 0; i < lobbySessionParticipants.Count; i++)
                    {
                        lobbySessionParticipants[i].command = Command.Solo;
                        lobbySessionParticipants[i].ordinal = (short)i;
                    }

                    return lobbySessionParticipants;
                }
                else
                {
                    int maxPlayerCount = 5;

                    List<LobbySessionParticipant> redCommand = new List<LobbySessionParticipant>();
                    List<LobbySessionParticipant> blueCommand = new List<LobbySessionParticipant>();

                    int FindFirstFreeOrdinal(List<LobbySessionParticipant> command, int maxPlayers)
                    {
                        for (int i = 0; i < maxPlayers; i++)
                        {
                            if (!command.Any(p => p.ordinal == i))
                            {
                                return i; 
                            }
                        }
                        return -1; 
                    }


                    foreach (var participant in lobbySessionParticipants)
                    {
                        if (lobbySession.addToRed && redCommand.Count < maxPlayerCount)
                        {
                            int freeSlot = FindFirstFreeOrdinal(redCommand, maxPlayerCount);
                            if (freeSlot != -1)
                            {
                                participant.command = Command.Red;
                                participant.ordinal = (short)freeSlot;
                                redCommand.Add(participant);
                                lobbySession.addToRed = false; 
                            }
                        }
                        else if (!lobbySession.addToRed && blueCommand.Count < maxPlayerCount)
                        {
                            int freeSlot = FindFirstFreeOrdinal(blueCommand, maxPlayerCount);
                            if (freeSlot != -1)
                            {
                                participant.command = Command.Blue;
                                participant.ordinal = (short)freeSlot;
                                blueCommand.Add(participant);
                                lobbySession.addToRed = true; 
                            }
                        }
                    }
                    List<LobbySessionParticipant> result = new List<LobbySessionParticipant>(redCommand);
                    result.AddRange(blueCommand);
                    return result;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return lobbySessionParticipants;
            }

        }

        public async void ConnectoToLobby(ConnectToLobbyRequest request, WebSocketBehavior WebSocket)
        {
            try
            {
                List<Participant> participantsToSendConnection = new List<Participant>();
                ConnectToLobbyResponse response = new ConnectToLobbyResponse();

                if (TryGetLobbySession(request.sessionID, out LobbySession lobbySession))
                {
                    if (TryGetParticipant(request.playerID, out Participant participant))
                    {
                        LobbySessionParticipant lobbySessionParticipant = ConvertToLobbySessionParticipant(participant);
                        lobbySessionParticipant.bid = lobbySession.minBid;
                        lobbySession.AddParticipant(lobbySessionParticipant);
                    }
                    lobbySession.SetParticipants(DistrebuteParticipantsInLobby(lobbySession, lobbySession.Participants.ToList()));
                    foreach(var part in lobbySession.Participants)
                    {
                        if(part.lobbyParticipantID != request.playerID 
                            && TryGetParticipant(part.lobbyParticipantID, out Participant participant1))
                        {
                            participantsToSendConnection.Add(participant1);
                        }
                    }
                    if (lobbySession.participants.Count == 2)
                    {
                        lobbySession.StartLobbyTimer();
                    }
                    participant.LobbySessionParticipant = lobbySession.lobbySessionID;
                    response.lobbyMode = lobbySession.lobbyMode;
                    response.lobbySession = lobbySession;
                    response.lobbyName = lobbySession.lobbyName;
                    response.maxPlayerCount = lobbySession.maxPlayerCount;
                    response.participants = lobbySession.Participants.ToList();
                    response.time = lobbySession.timeRemaining;
                    lobbySession.playerCount++;


                }
                string data = JsonConvert.SerializeObject(response);

                Message message = new Message(Handlers.HandlerTypes.Lobby, (int)LobbyHandlersType.ConnectToLobbyResponse, "", data);
                string dataSend = JsonConvert.SerializeObject(message);

                WebSocket.SendMessageAsync(dataSend);

                PlayerConnectToLobby playerConnectToLobby = new PlayerConnectToLobby(lobbySession.timeRemaining,lobbySession.Participants.ToList(), lobbySession.lobbySessionID, lobbySession.lobbyName, lobbySession.maxPlayerCount);
                string dataplayerConnectToLobby = JsonConvert.SerializeObject(playerConnectToLobby);

                foreach (var participant in participantsToSendConnection)
                {
                    Message message1 = new Message(Handlers.HandlerTypes.Lobby, (int)LobbyHandlersType.PlayerConnectToLobby, "", dataplayerConnectToLobby);
                    string dataSend1 = JsonConvert.SerializeObject(message1);

                    participant.WebSocket.SendMessageAsync(dataSend1);
                }

                foreach (var item in _participants)
                {
                    GetLobbySessionResponse getLobbySessionResponse = new GetLobbySessionResponse();
                    getLobbySessionResponse.sessions = _sessions;
                    string data1 = JsonConvert.SerializeObject(getLobbySessionResponse);
                    Message message1 = new Message(Handlers.HandlerTypes.Lobby, (int)LobbyHandlersType.GetLobbySessionResponse, "", data1);
                    string dataSend1 = JsonConvert.SerializeObject(message1);
                    LobbySession lobbySession1 = _sessions.FirstOrDefault();
                    if (lobbySession1 != null)
                        Console.WriteLine("Sending Lobbys " + item.Id + "   " + lobbySession1.participants.Count);

                    _ = item.WebSocket.SendMessageAsync(dataSend1);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private LobbySessionParticipant ConvertToLobbySessionParticipant(Participant participant)
        {
            return new LobbySessionParticipant(participant.Id, participant.WebSocket , participant.Name);
        }

        public void StartGameSession(LobbySession lobbySession)
        {
            foreach(var participant in lobbySession.participants)
            {
                if(TryGetParticipant(participant.lobbyParticipantID, out Participant participant1))
                {
                    StartGameSessionResponse startGameSessionResponse = new StartGameSessionResponse();
                    startGameSessionResponse.gameSessionID = lobbySession.lobbySessionID;
                    startGameSessionResponse.gameMode = lobbySession.lobbyMode;

                    string data = JsonConvert.SerializeObject(startGameSessionResponse);
                    Message message = new Message(Handlers.HandlerTypes.Lobby, (int)LobbyHandlersType.StartGameSessionResponse, "", data);
                    string dataSend = JsonConvert.SerializeObject(message);
                    participant1.WebSocket.SendMessageAsync(dataSend);
                }
            }
            _gameController.CreateGameSession(lobbySession.lobbySessionID, lobbySession.lobbyMode);
            _sessions.Remove(lobbySession);
            lobbySession.StopLobbyTimer();

            foreach (var item in _participants)
            {
                GetLobbySessionResponse getLobbySessionResponse = new GetLobbySessionResponse();
                getLobbySessionResponse.sessions = _sessions;
                string data1 = JsonConvert.SerializeObject(getLobbySessionResponse);
                Message message1 = new Message(Handlers.HandlerTypes.Lobby, (int)LobbyHandlersType.GetLobbySessionResponse, "", data1);
                string dataSend1 = JsonConvert.SerializeObject(message1);

                _ = item.WebSocket.SendMessageAsync(dataSend1);
            }
        }

        public async Task MakeBid(MakeBidRequest request)
        {
            var lobbySession = _sessions.FirstOrDefault(session => session.lobbySessionID == request.lobbyID);

            LobbySessionParticipant lobbySessionParticipant = lobbySession.participants.FirstOrDefault(x => x.lobbyParticipantID == request.playerID);

            if(lobbySessionParticipant != null)
            {
                lobbySessionParticipant.bid = request.bid;
                foreach (var item in lobbySession.Participants)
                {
                    MakeBidResponse response = new MakeBidResponse();
                    response.participants = lobbySession.participants;
                    string data = JsonConvert.SerializeObject(response);
                    Message message = new Message(Handlers.HandlerTypes.Lobby, (int)LobbyHandlersType.MakeBidResponse, "", data);
                    string dataSend = JsonConvert.SerializeObject(message);
                    await item.WebSocket.SendMessageAsync(dataSend);

                }
            }
        }

        public async void ParticipantReady(LobbyParticipantReadyRequest request, WebSocketBehavior WebSocket)
        {
            if(TryGetLobbySession(request.lobbyID, out LobbySession lobbySession))
            {
                LobbySessionParticipant participant = lobbySession.participants.FirstOrDefault(x => x.lobbyParticipantID == request.participantID);
                if(participant != null)
                {
                    participant.isReady = request.ready;
                }
                if(lobbySession.participants.Count >= 2)
                {
                    if (lobbySession.participants.Where(x => x.isReady).Count() == lobbySession.participants.Count())
                    {
                        lobbySession.isStartedGame = true;
                        StartGameSession(lobbySession);
                    }
                }
                foreach(var item in  lobbySession.participants)
                {
                    LobbyParticipantReadyResponse response = new LobbyParticipantReadyResponse();
                    response.isStartedGame = lobbySession.isStartedGame;
                    response.result = true;
                    response.ready = request.ready;
                    response.participantID = request.participantID;
                    string data = JsonConvert.SerializeObject(response);
                    Message message = new Message(Handlers.HandlerTypes.Lobby, (int)LobbyHandlersType.LobbyParticipantReadyResponse, "", data);
                    string dataSend = JsonConvert.SerializeObject(message);
                    await item.WebSocket.SendMessageAsync(dataSend);
                }
            }
        }

        public async Task GetUser(GetUserRequest request, WebSocketBehavior webSocket)
        {
            User user = await _databaseService.GetUserAsync(request.Id);

            GetUserResponse response = new GetUserResponse();
            response.user = user;

            string data = JsonConvert.SerializeObject(response);
            Message message = new Message(Handlers.HandlerTypes.Lobby, (int)LobbyHandlersType.GetUserResponse, "", data);
            string dataSend = JsonConvert.SerializeObject(message);
            await webSocket.SendMessageAsync(dataSend);
        }

        public async void CreateLobby(CreateLobbyRequest createLobbyRequest , WebSocketBehavior WebSocket)
        {
            try
            {
                int lobbyTime = 5;
                LobbySession lobbySession = new LobbySession(Guid.NewGuid().ToString(),
                    createLobbyRequest.lobbyName, createLobbyRequest.owner, createLobbyRequest.maxPlayers, 1, createLobbyRequest.lobbyMode, createLobbyRequest.password, createLobbyRequest.minBid);
                CreateLobbyResponse createLobbyResponse = new CreateLobbyResponse(lobbySession, lobbySession.lobbyName, lobbySession.maxPlayerCount,
                    lobbySession.password);
                if (TryGetParticipant(createLobbyRequest.participantCreatorID, out Participant participant))
                {
                    Console.WriteLine("TryGetParticipant true");
                    LobbySessionParticipant lobbySessionParticipant = ConvertToLobbySessionParticipant(participant);
                    lobbySessionParticipant.bid = lobbySession.minBid;
                    lobbySession.AddParticipant(lobbySessionParticipant);
                    lobbySession.SetParticipants(DistrebuteParticipantsInLobby(lobbySession,lobbySession.Participants.ToList()));
                    _sessions.Add(lobbySession);
                    participant.LobbySessionParticipant = lobbySession.lobbySessionID;
                    createLobbyResponse.participants = lobbySession.Participants.ToList();
                    
                    createLobbyResponse.time = lobbyTime;
                    createLobbyResponse.result = true;
                }
                else
                {
                    Console.WriteLine("TryGetParticipant false  " + createLobbyRequest.participantCreatorID);

                    createLobbyResponse.result = false;
                }
                string data = JsonConvert.SerializeObject(createLobbyResponse);
                Message message = new Message(Handlers.HandlerTypes.Lobby, (int)LobbyHandlersType.CreateLobbyResponse, "", data);
                string dataSend = JsonConvert.SerializeObject(message);


                WebSocket.SendMessageAsync(dataSend);

                foreach(var item in _participants)
                {
                    GetLobbySessionResponse getLobbySessionResponse = new GetLobbySessionResponse();
                    getLobbySessionResponse.sessions = _sessions;
                    string data1 = JsonConvert.SerializeObject(getLobbySessionResponse);
                    Message message1 = new Message(Handlers.HandlerTypes.Lobby, (int)LobbyHandlersType.GetLobbySessionResponse, "", data1);
                    string dataSend1 = JsonConvert.SerializeObject(message1);

                    _ = item.WebSocket.SendMessageAsync(dataSend1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error " + ex.ToString());
            }
        }


    }
}