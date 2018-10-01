using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using System.Web.UI;

using HTTP;

namespace FileServer
{
    class FileServer : AdvancedServer
    {
        static void Main(string[] args)
        {
            using (var server = new FileServer(Path.Combine(Directory.GetCurrentDirectory(), "www")))
            {
                server.Start();

                Console.WriteLine("Press Enter to end the program...");

                Console.ReadLine();
            }
        }

        private string base_dir;

    public FileServer(string base_dir) : base(new IPEndPoint(IPAddress.Parse("0.0.0.0"), 8080), new Method[] { Method.GET, Method.HEAD })
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
                } else if(tok != ".")
                {
                    path_stack.Push(tok);
                }
            }

            return System.IO.Path.Combine(base_dir, System.IO.Path.Combine(path_stack.Reverse().ToArray()));
        }

        private bool is_dir(string sanitized_path) => Directory.Exists(sanitized_path);
        private bool is_file(string sanitized_path) => File.Exists(sanitized_path);

        private string mk_html(string Title, Action<HtmlTextWriter> RenderBody)
        {
            using (var str_writer = new StringWriter())
            {
                using (var html_writer = new HtmlTextWriter(str_writer))
                {
                    html_writer.RenderBeginTag(HtmlTextWriterTag.Html);

                    html_writer.RenderBeginTag(HtmlTextWriterTag.Head);

                    html_writer.AddAttribute("charset", "UTF-8");
                    html_writer.RenderBeginTag(HtmlTextWriterTag.Meta);
                    html_writer.RenderEndTag();

                    html_writer.RenderBeginTag(HtmlTextWriterTag.Title);
                    html_writer.Write(Title);
                    html_writer.RenderEndTag();

                    html_writer.RenderEndTag();

                    html_writer.RenderBeginTag(HtmlTextWriterTag.Body);
                    RenderBody(html_writer);
                    html_writer.RenderEndTag();

                    html_writer.RenderBeginTag(HtmlTextWriterTag.Hr);
                    html_writer.RenderEndTag();

                    html_writer.RenderBeginTag("footer");
                    html_writer.Write($"Millinon's HTTP Server ({Environment.OSVersion.Platform})");
                    html_writer.RenderEndTag();

                    html_writer.RenderEndTag();
                }

                return str_writer.ToString();
            }
        }

        private Response ListDirectory(string Query_Path, string Sanitized_Directory_Path)
        {

            void render_body(HtmlTextWriter html_writer)
            {
                html_writer.RenderBeginTag(HtmlTextWriterTag.H1);
                html_writer.Write($"Index of directory {Query_Path}");
                html_writer.RenderEndTag();

                html_writer.RenderBeginTag(HtmlTextWriterTag.Hr);
                html_writer.RenderEndTag();

                html_writer.RenderBeginTag(HtmlTextWriterTag.Ul);

                if (Query_Path != "/")
                {
                    var toks = Query_Path.TrimEnd('/').Split('/');
                    var path = string.Join("/", toks.Take(toks.Count() - 1)).TrimEnd('/');

                    html_writer.RenderBeginTag(HtmlTextWriterTag.Ul);

                    html_writer.AddAttribute(HtmlTextWriterAttribute.Href, $"{path}/");
                    html_writer.RenderBeginTag(HtmlTextWriterTag.A);
                    html_writer.Write("..");
                    html_writer.RenderEndTag();

                    html_writer.RenderEndTag();
                }

                var dirinfo = new DirectoryInfo(Sanitized_Directory_Path);
                
                foreach(var dir in dirinfo.GetDirectories().Select(subdir => subdir.Name))
                {
                    html_writer.RenderBeginTag(HtmlTextWriterTag.Ul);

                    html_writer.AddAttribute(HtmlTextWriterAttribute.Href, $"{Query_Path}{dir}/");
                    html_writer.RenderBeginTag(HtmlTextWriterTag.A);
                    html_writer.Write($"{dir}/");
                    html_writer.RenderEndTag();

                    html_writer.RenderEndTag();
                }

                foreach(var file in dirinfo.GetFiles().Select(file => file.Name))
                {
                    html_writer.RenderBeginTag(HtmlTextWriterTag.Ul);

                    html_writer.AddAttribute(HtmlTextWriterAttribute.Href, $"{Query_Path}{file}");
                    html_writer.RenderBeginTag(HtmlTextWriterTag.A);
                    html_writer.Write(file);
                    html_writer.RenderEndTag();

                    html_writer.RenderEndTag();
                }

                html_writer.RenderEndTag();
            }

            var headers = DefaultHeaders();
            headers["Content-Type"] = "text/html; charset=utf-8";

            return new Response()
            {
                Status = StatusCode.OK,
                Headers = headers,
                Body = Encoding.UTF8.GetBytes(mk_html(Query_Path, render_body))
            };
        }

        public override Response RenderServerError(StatusCode Status)
        {
            void render_body(HtmlTextWriter html_writer)
            {
                html_writer.RenderBeginTag(HtmlTextWriterTag.H1);
                html_writer.Write($"{(int) Status} {Status.ToFriendlyString()}");
                html_writer.RenderEndTag();
            }

            var headers = DefaultHeaders();
            headers["Content-Type"] = "text/html; charset=utf-8";

            return new Response()
            {
                Status = Status,
                Headers = headers,
                Body = Encoding.UTF8.GetBytes(mk_html($"{(int)Status}", render_body)),
            };
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
                    return ListDirectory(Request.Query.Path, sanitized_path);
                }
            }
            else return RenderServerError(StatusCode.NOT_FOUND);
        }

        public override Response Handle_HEAD(Request Request)
        {
            var get_response = Handle_GET(Request);

            return new Response()
            {
                Status = get_response.Status,
                Headers = get_response.Headers,
            };
        }
    }
}
