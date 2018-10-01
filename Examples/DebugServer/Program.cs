using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using HTTP;

namespace DebugServer
{
    class DebugServer : AdvancedServer
    {
        private Response Render(Request Request)
        {
            return new Response()
            {
                Status = StatusCode.OK,
                Headers = DefaultHeaders(),
                Body = Encoding.ASCII.GetBytes($"{Request.Method} path: {Request.Query.Path} params: [{string.Join(",", Request.Query.Params.Select(kv => $"{kv.Key} => {kv.Value}"))}] UA: {(Request.Headers.User_Agent.HasValue ? Request.Headers.User_Agent.Value : "(none)")}"),
            };
        }

        public DebugServer() : base(new IPEndPoint(IPAddress.Parse("0.0.0.0"), 8080), (Method[]) Enum.GetValues(typeof(Method)))
        {

        }

        public override void AccessLog(BasicServer.Request Request)
        {
            Console.WriteLine($"[{DateTime.UtcNow}] {Request.Metadata.ClientIP} {Request.Method} {Request.Metadata.Query}");
        }

        public override void ErrorLog(string Message)
        {
            Console.WriteLine($"[{DateTime.UtcNow}] {Message}");
        }

        public override Response Handle_GET(Request Request) => Render(Request);

        public override Response Handle_HEAD(Request Request) => Render(Request);

        public override Response Handle_POST(Request_With_Body Request) => Render(Request);

        public override Response Handle_PUT(Request_With_Body Request) => Render(Request);

        public override Response Handle_DELETE(Request Request) => Render(Request);

        public override Response Handle_TRACE(Request Request) => Render(Request);

        public override Response Handle_OPTIONS(Request Request) => Render(Request);

        public override Response Handle_CONNECT(Request Request) => Render(Request);

        public override Response Handle_PATCH(Request_With_Body Request) => Render(Request);

    }

    class Program
    {
        static void Main(string[] args)
        {
            using (var server = new DebugServer())
            {
                server.Start();

                Console.WriteLine("Press Enter to end the program...");

                Console.ReadLine();
            }
        }
    }
}
