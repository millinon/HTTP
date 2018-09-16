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
        public static Dictionary<string, string> ParseParams(string ParamsStr)
        {
            var dict = new Dictionary<string, string>();

            foreach(var pair in ParamsStr.Split('&'))
            {
                var idx = pair.IndexOf('=');

                var key = pair.Substring(0, idx);
                if (dict.ContainsKey(key)) throw new ArgumentException();

                var value = Uri.UnescapeDataString(pair.Substring(idx + 1));

                dict[key] = value;
            }

            return dict;
        }

        protected static Request Refine(BasicServer.Request Raw)
        {
            if(Raw is BasicServer.Request_With_Body)
            {
                return new Request_With_Body(Raw as BasicServer.Request_With_Body);
            } else
            {
                return new Request(Raw);
            }
        }

        public struct Query
        {
            public readonly string Path;
            public readonly IReadOnlyDictionary<string, string> Params;

            public Query(string Query)
            {
                if (Query.Contains('?'))
                {
                    var idx = Query.IndexOf('?');

                    Path = Query.Substring(0, idx);
                    Params = ParseParams(Query.Substring(idx + 1));
                } else
                {
                    Path = Query;
                    Params = new Dictionary<string, string>();
                }
            }
        }

        new public class Request
        {
            public readonly BasicServer.Request RawRequest;
            public readonly IReadOnlyDictionary<string, string> RawHeaders;
            public readonly Method Method;

            public readonly Query Query;

            public Request(BasicServer.Request Raw)
            {
                this.RawRequest = Raw;
                this.RawHeaders = Raw.Metadata.Headers;
                this.Method = Raw.Method;

                Query = new Query(Raw.Metadata.Query);
            }
        };

        new public class Request_With_Body : Request
        {
            public readonly IReadOnlyCollection<byte> Body;

            public Request_With_Body(BasicServer.Request_With_Body Raw) : base(Raw) => this.Body = Raw.Body;
        }

        public AdvancedServer(IPAddress IP, IEnumerable<Method> AcceptedMethods) : base(IP, AcceptedMethods)
        {

        }

        public virtual Response Handle_GET(Request Request) => RenderServerError(StatusCode.NOT_IMPLEMENTED);

        public virtual Response Handle_HEAD(Request Request) => RenderServerError(StatusCode.NOT_IMPLEMENTED);

        public virtual Response Handle_POST(Request_With_Body Request) => RenderServerError(StatusCode.NOT_IMPLEMENTED);

        public virtual Response Handle_PUT(Request_With_Body Request) => RenderServerError(StatusCode.NOT_IMPLEMENTED);

        public virtual Response Handle_DELETE(Request Request) => RenderServerError(StatusCode.NOT_IMPLEMENTED);

        public virtual Response Handle_TRACE(Request Request) => RenderServerError(StatusCode.NOT_IMPLEMENTED);

        public virtual Response Handle_OPTIONS(Request Request) => RenderServerError(StatusCode.NOT_IMPLEMENTED);

        public virtual Response Handle_CONNECT(Request Request) => RenderServerError(StatusCode.NOT_IMPLEMENTED);

        public virtual Response Handle_PATCH(Request_With_Body Request) => RenderServerError(StatusCode.NOT_IMPLEMENTED);

        public sealed override Response Handle(BasicServer.Request Request)
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

            Response response;

            switch (request.Method)
            {
                case Method.GET:
                    response = Handle_GET(request);
                    break;
                case Method.HEAD:
                    response = Handle_HEAD(request);
                    break;
                case Method.POST:
                    response = Handle_POST(request as Request_With_Body);
                    break;
                case Method.PUT:
                    response = Handle_PUT(request as Request_With_Body);
                    break;
                case Method.DELETE:
                    response = Handle_DELETE(request);
                    break;
                case Method.TRACE:
                    response = Handle_TRACE(request);
                    break;
                case Method.OPTIONS:
                    response = Handle_OPTIONS(request);
                    break;
                case Method.CONNECT:
                    response = Handle_CONNECT(request);
                    break;
                case Method.PATCH:
                    response = Handle_PATCH(request as Request_With_Body);
                    break;
                default:
                    response = RenderServerError(StatusCode.BAD_REQUEST);
                    break;
            }

            if(response.Body != null)
            {
                if (! response.Headers.ContainsKey("Content-Length"))
                {
                    response.Headers["Content-Length"] = response.Body.Length.ToString();
                }
            }

            return response; 
        }
        


    }
}
