using System;
using System.IO;
using System.Linq;
using System.Text;

using System.Web.UI;

using HTTP;

namespace WebApp
{
    public class AssetServer
    {
        private readonly Application Application;

        public AssetServer(Application Application)
        {
            this.Application = Application;
        }

        private BasicServer.Response Get(string LocalPath, string QueryPath, bool IndexDirectories)
        {
            var headers = AdvancedServer.DefaultHeaders();

            if (Directory.Exists(LocalPath))
            {
                if (IndexDirectories)
                {
                    headers["Content-Type"] = "text/html";

                    using (var string_writer = new StringWriter())
                    {
                        using (var html_writer = new HtmlTextWriter(string_writer))
                        {
                            html_writer.RenderBeginTag(HtmlTextWriterTag.Html);

                            html_writer.RenderBeginTag(HtmlTextWriterTag.Head);

                            html_writer.AddAttribute("charset", "UTF-8");
                            html_writer.RenderBeginTag(HtmlTextWriterTag.Meta);
                            html_writer.RenderEndTag();

                            html_writer.RenderBeginTag(HtmlTextWriterTag.Title);
                            html_writer.Write($"Index of {QueryPath}");
                            html_writer.RenderEndTag();

                            html_writer.RenderEndTag();

                            html_writer.RenderBeginTag(HtmlTextWriterTag.Body);

                            html_writer.RenderBeginTag(HtmlTextWriterTag.H1);
                            html_writer.Write($"Index of directory {QueryPath}");
                            html_writer.RenderEndTag();

                            html_writer.RenderBeginTag(HtmlTextWriterTag.Hr);
                            html_writer.RenderEndTag();

                            html_writer.RenderBeginTag(HtmlTextWriterTag.Ul);

                            if (QueryPath != "/")
                            {
                                var toks = QueryPath.TrimEnd('/').Split('/');
                                var path = string.Join("/", toks.Take(toks.Count() - 1)).TrimEnd('/');

                                html_writer.RenderBeginTag(HtmlTextWriterTag.Ul);

                                html_writer.AddAttribute(HtmlTextWriterAttribute.Href, $"{path}/");
                                html_writer.RenderBeginTag(HtmlTextWriterTag.A);
                                html_writer.Write("..");
                                html_writer.RenderEndTag();

                                html_writer.RenderEndTag();
                            }

                            var dirinfo = new DirectoryInfo(LocalPath);

                            foreach (var dir in dirinfo.GetDirectories().Select(subdir => subdir.Name))
                            {
                                html_writer.RenderBeginTag(HtmlTextWriterTag.Ul);

                                html_writer.AddAttribute(HtmlTextWriterAttribute.Href, $"{QueryPath.TrimEnd('/')}/{dir}/");
                                html_writer.RenderBeginTag(HtmlTextWriterTag.A);
                                html_writer.Write($"{dir}/");
                                html_writer.RenderEndTag();

                                html_writer.RenderEndTag();
                            }

                            foreach (var file in dirinfo.GetFiles().Select(file => file.Name))
                            {
                                html_writer.RenderBeginTag(HtmlTextWriterTag.Ul);

                                html_writer.AddAttribute(HtmlTextWriterAttribute.Href, $"{QueryPath.TrimEnd('/')}/{file}");
                                html_writer.RenderBeginTag(HtmlTextWriterTag.A);
                                html_writer.Write(file);
                                html_writer.RenderEndTag();

                                html_writer.RenderEndTag();
                            }

                            html_writer.RenderEndTag();

                            html_writer.RenderEndTag();

                            html_writer.RenderBeginTag(HtmlTextWriterTag.Hr);
                            html_writer.RenderEndTag();

                            html_writer.RenderBeginTag("footer");
                            html_writer.Write($"Millinon's HTTP Server ({Environment.OSVersion.Platform})");
                            html_writer.RenderEndTag();

                            html_writer.RenderEndTag();
                        }

                        return new BasicServer.Response()
                        {
                            Status = StatusCode.OK,
                            Headers = headers,
                            Body = Encoding.UTF8.GetBytes($"<!DOCTYPE html>\n{string_writer.ToString()}")
                        };
                    }
                } else
                {
                    return new BasicServer.Response()
                    {
                        Status = StatusCode.UNAUTHORIZED,
                        Headers = headers,
                        Body = Encoding.UTF8.GetBytes("403 unauthorized"),
                    };
                }
            } else if (File.Exists(LocalPath))
            {
                var fileinfo = new FileInfo(LocalPath);

                var ct = ContentType.Lookup(Path.GetExtension(LocalPath));

                if (!ct.HasValue)
                {
                    headers["Content-Type"] = "application/octet-stream";
                }
                else
                {
                    headers["Content-Type"] = ct.Value.ToString();
                }

                return new BasicServer.Response()
                {
                    Status = StatusCode.OK,
                    Headers = headers,
                    Body = File.ReadAllBytes(LocalPath),
                };
            } else
            {
                return new BasicServer.Response()
                {
                    Status = StatusCode.NOT_FOUND,
                    Headers = headers,
                    Body = Encoding.UTF8.GetBytes("404 not found"),
                };
            }
        }

        private BasicServer.Response Head(string Path, string QueryPath, bool IndexDirectories)
        {
            var get_response = Get(Path, QueryPath, IndexDirectories);

            if (get_response.Status == StatusCode.OK)
            {
                return new BasicServer.Response()
                {
                    Status = StatusCode.OK,
                    Headers = get_response.Headers,
                };
            }
            else return get_response;
        }

        public void Register(string AssetDir, RouterCollection Router, string URLPrefix = "/assets", bool IndexDirectories = false)
        {
            if(!Directory.Exists(AssetDir))
            {
                throw new ArgumentException($"Directory not found: {AssetDir}");
            }

            var dirinfo = new DirectoryInfo(AssetDir);

            if (IndexDirectories)
            {
                Router.GET($"{URLPrefix}", r => Get(dirinfo.FullName, $"{URLPrefix}", true));
                Router.HEAD($"{URLPrefix}", r => Head(dirinfo.FullName, $"{URLPrefix}", true));
            }

            foreach (var subdir in dirinfo.GetDirectories())
            {
                Register(subdir.FullName, Router, $"{URLPrefix}/{subdir.Name}", IndexDirectories);                
            }

            foreach (var file in dirinfo.GetFiles())
            {
                Router.GET($"{URLPrefix}/{file.Name}", r => Get(file.FullName, $"{URLPrefix}/{file.Name}", false));
                Router.HEAD($"{URLPrefix}/{file.Name}", r => Head(file.FullName, $"{URLPrefix}/{file.Name}", false));
            }
        }

    }
}
