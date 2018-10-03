using System;
using System.IO;
using System.Net;
using System.Text;

using HTTP;
using WebApp;

namespace HelloWorldApp
{
    class HelloWorld : Application
    {
        public HelloWorld() : base("HelloWorld", new IPEndPoint(IPAddress.Parse("0.0.0.0"), 8080), new Method[] { Method.GET, Method.HEAD }, Path.Combine(Directory.GetCurrentDirectory(), "log"))
        {
            BasicServer.Response render_plaintext(string text)
            {
                var headers = BasicServer.DefaultHeaders();

                headers["Content-Type"] = "text/plain";

                return new BasicServer.Response()
                {
                    Status = StatusCode.OK,
                    Body = Encoding.UTF8.GetBytes(text),
                    Headers = headers,
                };
            }

            Router.GET("/", (r => render_plaintext("Hello, world!")));
            Router.GET("/test", (r => render_plaintext("test successful")));
            Router.GET("/name/$name", (r, vars) => render_plaintext($"hello, {vars["name"]}!"));
            
            //Router.GET("/foo/$bar/baz/$bar", r => render_plaintext("this shouldn't work!"));
        }

        static void Main(string[] args)
        {
            using (var hello = new HelloWorld())
            {
                hello.Start();

                Console.WriteLine("Press Enter to end the program...");

                Console.ReadLine();
            }
        }
    }
}