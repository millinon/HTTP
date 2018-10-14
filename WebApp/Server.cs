using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

using HTTP;

namespace WebApp
{
    public class Server : AdvancedServer
    {
        private readonly StreamWriter ErrorLogStream;
        private readonly StreamWriter AccessLogStream;

        private readonly Application Application;

        public Server(Application Application, IPEndPoint Endpoint, IEnumerable<Method> AcceptedMethods, string ErrorLog, string AccessLog) : base(Endpoint, AcceptedMethods)
        {
            foreach (var log in new string[] { ErrorLog, AccessLog })
            {
                var dir = Path.GetDirectoryName(log);
                if(!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }

            ErrorLogStream = new StreamWriter(new FileStream(ErrorLog, FileMode.OpenOrCreate | FileMode.Append));
            AccessLogStream = new StreamWriter(new FileStream(AccessLog, FileMode.OpenOrCreate | FileMode.Append));

            this.Application = Application;
        }

        public override void AccessLog(BasicServer.Request Request)
        {
            AccessLogStream.WriteLine($"[{DateTime.UtcNow}] {Request.Metadata.ClientIP} {Request.Method} {Request.Metadata.Query}");
        }

        public override void ErrorLog(string Message)
        {
            try
            {
                ErrorLogStream.WriteLine($"[{DateTime.UtcNow}] {Message}");
                ErrorLogStream.Flush();
            }
            catch (Exception) { }
        }
        
        protected override void Dispose(bool disposing)
        {
            ErrorLogStream.Close();
            AccessLogStream.Close();

            base.Dispose(disposing);
        }

        public override Response Handle_CONNECT(Request Request) => Application.Route(Request);
        public override Response Handle_DELETE(Request Request) => Application.Route(Request);
        public override Response Handle_GET(Request Request) => Application.Route(Request);
        public override Response Handle_HEAD(Request Request) => Application.Route(Request);
        public override Response Handle_OPTIONS(Request Request) => Application.Route(Request);
        public override Response Handle_PATCH(Request_With_Body Request) => Application.Route(Request);
        public override Response Handle_POST(Request_With_Body Request) => Application.Route(Request);
        public override Response Handle_PUT(Request_With_Body Request) => Application.Route(Request);
        public override Response Handle_TRACE(Request Request) => Application.Route(Request);
    }
}
