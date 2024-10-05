using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Tanks3DServer.DTO;
using Tanks3DServer.GameSessionScripts;
using Newtonsoft.Json;
using EnglishWordsServer.AutofacConfiig;
using Autofac;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using Tanks3DServer.LobbyScripts;
using WebSocketSharp.Server;

namespace Tanks3DServer.ServerUDPScripts
{
    internal class ServerUDP
    {
        private WebSocketServer _server;

        public ServerUDP()
        {

            //var certificatePath = "C:\\Users\\VIP\\source\\repos\\Tanks3DServer\\certificate.pfx";
            //var certificatePassword = "123456";

            //// Создание сертификата
            //var certificate = new X509Certificate2(certificatePath, certificatePassword);

            // Создание экземпляра WebSocketServer
            _server = new WebSocketServer("ws://212.67.12.133:8080/");
            //_server = new WebSocketServer("ws://localhost:8080/");

            //_server.SslConfiguration.ServerCertificate = certificate;
            //_server.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;

            // Устанавливаем обработчик для WebSocketBehavior соединений
            _server.AddWebSocketService<WebSocketUDP>("/", () => new WebSocketUDP());
        }

        public async Task Start()
        {
            _server.Start();
            Console.WriteLine("Server WebSocketBehavior started");
        }

        public void Stop()
        {
            _server.Stop();
        }
    }
}
