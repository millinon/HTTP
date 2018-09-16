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
        protected static Request Refine(BasicServer.Request Raw)
        {
            if(Raw is BasicServer.Request_With_Body)
            {
                
            } else
            {

            }

            return null;
        }

        new public class Request
        {
            public readonly HTTP.BasicServer.Request RawRequest;

            public readonly IReadOnlyDictionary<string, string> RawHeaders;

            public readonly Method Method;

            public Request(HTTP.BasicServer.Request Raw)
            {
                this.RawRequest = Raw;
                this.RawHeaders = Raw.Metadata.Headers;
                this.Method = Raw.Method;
            }
        };

        new public class Request_With_Body
        {
            public readonly HTTP.BasicServer.Request_With_Body RawRequest;

            public readonly IReadOnlyDictionary<string, string> RawHeaders;

            public readonly Method Method;

            public readonly IReadOnlyCollection<byte> Body;

            public Request_With_Body(HTTP.BasicServer.Request_With_Body Raw)
            {
                this.RawRequest = Raw;
                this.RawHeaders = Raw.Metadata.Headers;
                this.Method = Raw.Method;
                this.Body = Raw.Body;
            }
        }


        public AdvancedServer(IPAddress IP, List<Method> AcceptedMethods) : base(IP, AcceptedMethods)
        {

        }

        public abstract BasicServer.Response Handle(Request Request);

        public override BasicServer.Response Handle(BasicServer.Request Request)
        {
            Request request;

            try
            {
                request = Refine(Request);
            } catch(Exception e)
            {
                ErrorLog(e.Message);
                return RenderServerError(StatusCode.BAD_REQUEST);
            }

            return Handle(request);

        }
        


    }
}
