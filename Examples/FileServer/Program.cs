using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using HTTP;

namespace FileServer
{
    class FileServer : AdvancedServer
    {
        static void Main(string[] args)
        {
            using (var server = new FileServer(Path.Combine(Directory.GetCurrentDirectory(), "www")))
            {
                server.Start(8080);

                Console.WriteLine("Press Enter to end the program...");

                Console.ReadLine();
            }
        }

        private string base_dir;

    public FileServer(string base_dir) : base(IPAddress.Parse("127.0.0.1"), new Method[] { Method.GET, Method.HEAD })
        {
            this.base_dir = base_dir;
            Console.WriteLine($"serving files in {base_dir}...");
        }

        public override void AccessLog(BasicServer.Request Request)
        {
            Console.WriteLine($"[{DateTime.UtcNow}] {Request.Metadata.ClientIP} {Request.Method} {Request.Metadata.Query}");
        }

        public override void ErrorLog(string Message)
        {
            Console.WriteLine($"[{DateTime.UtcNow}] {Message}");
        }

        private string sanitize_path(string Path)
        {
            var path_stack = new Stack<string>();

            var toks = Path.Split('/');

            foreach(var tok in Path.Split('/'))
            {
                if(tok == "..")
                {
                    if (path_stack.Count() == 0) throw new Exception("shenanigans detected");
                    else path_stack.Pop();
                } else
                {
                    path_stack.Push(tok);
                }
            }

            return System.IO.Path.Combine(base_dir, System.IO.Path.Combine(path_stack.Reverse().ToArray()));
        }

        private bool is_dir(string sanitized_path) => Directory.Exists(sanitized_path);
        private bool is_file(string sanitized_path) => File.Exists(sanitized_path);

        private Response ListDirectory(string Path)
        {
           return RenderServerError(StatusCode.UNAUTHORIZED);
        }
        
        public override Response Handle_GET(Request Request)
        {
            var sanitized_path = sanitize_path(Request.Query.Path);

            if (is_file(sanitized_path))
            {
                var ct = ContentType.Lookup(Path.GetExtension(sanitized_path));
                if (!ct.HasValue)
                {
                    ct = Option<ContentType>.Some("application/octet-stream");
                }

                var headers = DefaultHeaders();
                headers["Content-Type"] = ct.Value.ToString();

                return new Response()
                {
                    Body = File.ReadAllBytes(sanitized_path),
                    Headers = headers,
                    Status = StatusCode.OK,
                };

            }
            else if (is_dir(sanitized_path))
            {
                var index_path = Path.Combine(sanitized_path, "index.html");

                if (File.Exists(index_path))
                {
                    var ct = ContentType.Lookup(Path.GetExtension(index_path));
                    if (!ct.HasValue)
                    {
                        ct = Option<ContentType>.Some("application/octet-stream");
                    }

                    var headers = DefaultHeaders();
                    headers["Content-Type"] = ct.Value.ToString();

                    return new Response()
                    {
                        Body = File.ReadAllBytes(index_path),
                        Headers = headers,
                        Status = StatusCode.OK,
                    };
                } else
                {
                    return ListDirectory(sanitized_path);
                }
            }
            else return RenderServerError(StatusCode.NOT_FOUND);
        }

        public override Response Handle_HEAD(Request Request)
        {
            var get_response = Handle_GET(Request);

            var headers = DefaultHeaders();

            if (get_response.Headers.ContainsKey("Content-Type"))
            {
                headers["Content-Type"] = get_response.Headers["Content-Type"];
            }

            if (get_response.Headers.ContainsKey("Content-Length"))
            {
                headers["Content-Length"] = get_response.Headers["Content-Length"];
            }

            return new Response()
            {
                Status = get_response.Status,
                Headers = headers,
            };
        }
    }
}
