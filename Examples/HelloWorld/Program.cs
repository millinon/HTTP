using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using HTTP;

namespace DebugServer
{
    class HelloWorldServer : BasicServer
    {
        public HelloWorldServer() : base(IPAddress.Parse("0.0.0.0"), new List<Method>() { Method.GET, Method.HEAD })
        {

        }

        public override void AccessLog(Request Request)
        {
            Console.WriteLine($"[{DateTime.UtcNow}] {Request.Metadata.ClientIP} {Request.Method} {Request.Metadata.Query}");
        }

        public override void ErrorLog(string Message)
        {
            Console.WriteLine($"[{DateTime.UtcNow}] {Message}");
        }

        public override Response Handle(Request Request)
        {
            return new Response()
            {
                Status = StatusCode.OK,
                Headers = DefaultHeaders(),
                Body = Encoding.ASCII.GetBytes("Hello, world!")
            };
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            using (var server = new HelloWorldServer())
            {
                server.Start(8080);

                Console.WriteLine("Press Enter to end the program...");

                Console.ReadLine();
            }
        }
    }
}
