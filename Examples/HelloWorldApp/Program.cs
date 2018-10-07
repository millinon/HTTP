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
        protected readonly TemplateEngine Views;

        public HelloWorld() : base("HelloWorld", new IPEndPoint(IPAddress.Parse("0.0.0.0"), 8080), new Method[] { Method.GET, Method.HEAD }, Path.Combine(Directory.GetCurrentDirectory(), "log"), "/HTTP")
        {
            BasicServer.Response render(string text, string ct = "text/plain")
            {
                var headers = BasicServer.DefaultHeaders();

                headers["Content-Type"] = ct;

                return new BasicServer.Response()
                {
                    Status = StatusCode.OK,
                    Body = Encoding.UTF8.GetBytes(text),
                    Headers = headers,
                };
            }

            Router.GET("/", (r => render("Hello, world!")));
            Router.GET("/test", (r => render("test successful")));
            Router.GET("/name/$name", (r, vars) => render($"hello, {vars["name"]}!"));
            Router.GET("/doublename/$name", (r, vars) => render($"{vars["name"]}{vars["name"]}"));

            Router.HEAD("/", (r => render("Hello, world!")));
            Router.HEAD("/test", (r => render("test successful")));
            Router.HEAD("/name/$name", (r, vars) => render($"hello, {vars["name"]}!"));
            Router.HEAD("/doublename/$name", (r, vars) => render($"{vars["name"]}{vars["name"]}"));

            var assetsdir = Path.Combine(Directory.GetCurrentDirectory(), "assets");

            if (Directory.Exists(assetsdir)) AssetServer.Register(assetsdir, Router, "/assets", true);

            //Router.GET("/foo/$bar/baz/$bar", r => render_plaintext("this shouldn't work!"));*

            var viewsdir = Path.Combine(Directory.GetCurrentDirectory(), "views");

            if (Directory.Exists(viewsdir))
            {
                Views = new TemplateEngine(viewsdir);

                var name_template = Views.Load("name.html.template");
                Router.GET("/rendername/$name", (r, vars) => render(name_template.With(vars), "text/html"));
            }
               
            Console.WriteLine("routes:");
            foreach (var route in Router.Routes)
            {
                Console.WriteLine($"  {route}");
            }
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