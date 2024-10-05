using Autofac;
using EnglishWordsServer.AutofacConfiig;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Tanks3DServer.DTO;
using Tanks3DServer.GameSessionScripts;
using Tanks3DServer.LobbyScripts;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Tanks3DServer.ServerTCPScripts
{
    public class WebSocketBehavior : WebSocketSharp.Server.WebSocketBehavior
    {
        private LobbyModel _lobbyModel;
        private LobbyHandler _lobbyHandler;
        private GameHandler _gameHandler;
        private GameController _gameController;
        protected override void OnMessage(MessageEventArgs e)
        {
            try
            {
                string receivedMessage = e.Data;
                Console.WriteLine(receivedMessage);
                Message message = JsonConvert.DeserializeObject<Message>(receivedMessage);

                HandleMessage(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client error: {ex.Message}");
            }
        }

        protected override void OnOpen()
        {
            _gameHandler = AutofacProjectContext.Container.Resolve<GameHandler>();
            _gameController = AutofacProjectContext.Container.Resolve<GameController>();
            _lobbyModel = AutofacProjectContext.Container.Resolve<LobbyModel>();
            _lobbyHandler = AutofacProjectContext.Container.Resolve<LobbyHandler>();
            Console.WriteLine("Client connected. " + ID);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            _lobbyModel.RemoveParticipant(this);
            Console.WriteLine("Client disconnected.");
        }

        private void HandleMessage(Message message)
        {
            if (message != null)
            {
                Console.WriteLine("message.mainHandler " + message.mainHandler);
                if (message.mainHandler == Handlers.HandlerTypes.Lobby)
                {
                    _lobbyHandler.Handle(message.body, message.handler, this);
                }
            }
            else
            {
                Console.WriteLine("Message is null");
            }
        }

        public async Task SendMessageAsync(string message)
        {
            Console.WriteLine("SendMessageAsync " + message);
            if (State == WebSocketSharp.WebSocketState.Open)
            {
                Console.WriteLine("Sending to " + ID);
    
                Send(message);

            }
        }
    }
}
