using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HTTP;

namespace EchoServer
{
    class Program
    {
        class Server : HTTP.BasicServer
        {
            public Server() : base(IPAddress.Parse("0.0.0.0"), new List<Method>() { Method.GET, Method.HEAD, Method.POST })
            {

            }

            public override Response Handle(Request Request)
            {
                if (Request.Method == Method.POST)
                {
                    var body = (Request as POST_Request).Body.ToArray();

                    return new Response()
                    {
                        Status = StatusCode.OK,
                        Body = body,
                        Headers = DefaultHeaders(),
                    };
                }
                else
                {
                    return new Response()
                    {
                        Status = StatusCode.OK,
                        Body = Encoding.ASCII.GetBytes("Hello, world!"),
                        Headers = DefaultHeaders(),
                    };
                }
            }

            public override void AccessLog(Request Request)
            {
                Console.WriteLine($"[{DateTime.Now}] {Request.Metadata.ClientIP}: {Request.Method} {Request.Metadata.Query}");
                foreach (var header in Request.Metadata.Headers)
                {
                    Console.WriteLine($"  {header.Key}: {header.Value}");
                }
            }

            public override void ErrorLog(string Message)
            {
                Console.WriteLine($"[{DateTime.Now}] ERROR: {Message}");
            }
        }

        static void Main(string[] args)
        {
            var dir = args.Length == 0 ? Directory.GetCurrentDirectory() : args[1];

            if (!Directory.Exists(dir)) Console.WriteLine($"Directory {dir} not found");

            using (var server = new Server())
            {
                server.Start(5555);

                Console.WriteLine("Press enter to end the server...");

                Console.ReadLine();
            }
        }
    }
}
