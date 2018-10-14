using System;
using System.IO;
using System.Net;
using System.Text;

using System.Linq;

using HTTP;
using WebApp;

namespace HelloWorldApp
{
    class HelloWorld : Application
    {
        protected readonly TemplateEngine Views;
        protected readonly WebApp.SessionStores.MemoryStore Sessions;

        public HelloWorld() : base("HelloWorld", new IPEndPoint(IPAddress.Parse("0.0.0.0"), 8080), new Method[] { Method.GET, Method.HEAD }, Path.Combine(Directory.GetCurrentDirectory(), "log"), "/HTTP")
        {

            Sessions = new WebApp.SessionStores.MemoryStore();

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

            BasicServer.Response count(AdvancedServer.Request request)
            {
                var response_headers = BasicServer.DefaultHeaders();

                WebApp.SessionStores.DictionarySession session;

                if (!request.Headers.Cookie.HasValue)
                {
                    session = Sessions.Create();

                    response_headers["Set-Cookie"] = $"id={session.ID}";
                }
                else
                {
                    var cookie_str = request.Headers.Cookie.Value;
                    var tok = cookie_str.Trim().Split(' ').Where(str => str.StartsWith("id=")).Select(s => s.TrimEnd(';')).Last();
                    var id = tok.Substring(3);

                    if (!Sessions.HasSession(id))
                    {
                        session = Sessions.Create();
                        response_headers["Set-Cookie"] = $"id={session.ID}";
                    }
                    else
                    {
                        session = Sessions.Open(id);
                    }
                }

                int hit_count;

                if (!session.HasKey("count"))
                {
                    hit_count = 0;
                }
                else
                {
                    hit_count = int.Parse(session["count"]);
                }

                session["count"] = $"{++hit_count}";

                var response_text = $"count: {hit_count}";

                return new BasicServer.Response()
                {
                    Status = StatusCode.OK,
                    Headers = response_headers,
                    Body = Encoding.UTF8.GetBytes(response_text),
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

            //Router.GET("/foo/$bar/baz/$bar", r => render_plaintext("this shouldn't work!"));

            Router.GET("/count", count);
            Router.GET("/count/reset", request =>
            {
                if (!request.Headers.Cookie.HasValue)
                {
                    return Server.RenderServerError(StatusCode.BAD_REQUEST);
                }

                var cookie_str = request.Headers.Cookie.Value;
                var tok = cookie_str.Trim().Split(' ').Where(str => str.StartsWith("id=")).Select(s => s.TrimEnd(';')).Last();
                var id = tok.Substring(3);

                if (Sessions.HasSession(id))
                {
                    Sessions.Destroy(Sessions.Open(id));
                }

                var redirect = Redirect("/HTTP/count");

                redirect.Headers["Set-Cookie"] = "id=";

                return redirect;
            });

            var assetsdir = Path.Combine(Directory.GetCurrentDirectory(), "assets");
            if (Directory.Exists(assetsdir)) AssetServer.Register(assetsdir, Router, "/assets", true);

            var viewsdir = Path.Combine(Directory.GetCurrentDirectory(), "views");
            if (Directory.Exists(viewsdir))
            {
                Views = new TemplateEngine(viewsdir);

                var name_template = Views.Load("name.html.template");
                Router.GET("/rendername/$name", (r, vars) => render(name_template.With(vars), "text/html"));
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