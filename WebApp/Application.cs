using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using HTTP;

namespace WebApp
{
    public abstract class Application : IDisposable
    {
        protected readonly RouterCollection Router;
        private readonly Server Server;
        public readonly string Name;
        protected readonly AssetServer AssetServer;

        private BasicServer.Response Redirect(string URL, bool Permanent = false)
        {
            var headers = Server.DefaultHeaders();

            headers["Location"] = URL;

            return new BasicServer.Response()
            {
                Headers = headers,
                Status = (Permanent ? StatusCode.PERMANENT_REDIRECT : StatusCode.TEMPORARY_REDIRECT),
            };
        }

        public Application(string Name, IPEndPoint Endpoint, IEnumerable<Method> AcceptedMethods, string LogDir, string URLPrefix = "")
        {
            Router = new RouterCollection(URLPrefix);

            Server = new Server(this, Endpoint, AcceptedMethods, Path.Combine(LogDir, "error.log"), Path.Combine(LogDir, "access.log"));
            AssetServer = new AssetServer(this);
        }

        public BasicServer.Response Route(AdvancedServer.Request Request)
        {
            if(! Router.IsMatch(Request.Method, Request.Query.Path))
            {
                return DefaultResponse(Request);
            } else
            {
                return Router.Match(Request.Method, Request.Query.Path)(Request);
            }
        }

        public virtual BasicServer.Response DefaultResponse(AdvancedServer.Request Request) => Redirect("/");

        public void Start()
        {
            Server.Start();
        }

        public void Stop()
        {
            Server.Stop();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Server.Dispose();
                }

                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion




    }
}
