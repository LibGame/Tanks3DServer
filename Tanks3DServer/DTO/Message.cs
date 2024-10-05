using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tanks3DServer.Handlers;

namespace Tanks3DServer.DTO
{
    internal class Message
    {
        public HandlerTypes mainHandler;
        public int handler;
        public string header;
        public string body;

        public Message(HandlerTypes mainHandler, int handler, string header, string body) 
        {
            this.mainHandler = mainHandler;
            this.handler = handler;
            this.header = header;
            this.body = body;
        }
    }
}
