using System.Collections;
using System.Net.Sockets;
using System.Net;
using Tanks3DServer.LobbyScripts;
using Newtonsoft.Json;
using System.Text;
using System.Timers;
using Tanks3DServer.DTO;
using System.IO;
using EnglishWordsServer.AutofacConfiig;
using Autofac;
using System.Net.WebSockets;
using WebSocketSharp.Server;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting.Server;
using System.Net.Security;
using System.Reflection;
using System.Diagnostics;

namespace Tanks3DServer.ServerTCPScripts
{
    public class ServerTCP 
    {
        private WebSocketServer _server;

        public ServerTCP()
        {

            //var certificatePath = "C:\\Users\\VIP\\source\\repos\\Tanks3DServer\\certificate.pfx";
            //var certificatePassword = "123456";

            //// Создание сертификата
            //var certificate = new X509Certificate2(certificatePath, certificatePassword);

            //// Создание экземпляра WebSocketServer
            _server = new WebSocketServer("ws://212.67.12.133:7070/");
            //_server = new WebSocketServer("ws://localhost:7070/");
            //_server.SslConfiguration.ServerCertificate = certificate;

            //_server.Log.Level = WebSocketSharp.LogLevel.Trace;
            //_server.SslConfiguration.EnabledSslProtocols = (System.Security.Authentication.SslProtocols)(SecurityProtocolType.Tls | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11);
            //_server.SslConfiguration.ClientCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
            //{
            //    Console.WriteLine($"{sender} : {certificate} : {chain} : {sslPolicyErrors}");
            //    return true;
            //};

            _server.AddWebSocketService<WebSocketBehavior>("/", () => new WebSocketBehavior());
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