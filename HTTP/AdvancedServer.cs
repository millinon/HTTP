using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HTTP
{
    public abstract class AdvancedServer : BasicServer
    {
        public AdvancedServer(IPAddress IP, List<Method> AcceptedMethods) : base(IP, AcceptedMethods)
        {

        }

        protected override void WriteResponse(Socket ClientSocket, Response Response)
        {
            var base_headers = Response.Headers;

            if(!base_headers.ContainsKey("Content-Length"))
            {
                base_headers["Content-Length"] = Response.Body.Length.ToString();
            }

            base.WriteResponse(ClientSocket, Response);
        }

        protected override Request ReadClientRequest(Socket ClientSocket)
        {
            var base_request = base.ReadClientRequest(ClientSocket);






        }


    }
}
