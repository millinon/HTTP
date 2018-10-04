using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HTTP
{
    public abstract class AdvancedServer : BasicServer
    {
        public static Dictionary<string, string> ParseParams(string ParamsStr)
        {
            var dict = new Dictionary<string, string>();

            foreach (var pair in ParamsStr.Split('&'))
            {
                var idx = pair.IndexOf('=');

                var key = pair.Substring(0, idx);
                if (dict.ContainsKey(key)) throw new ArgumentException();

                var value = Uri.UnescapeDataString(pair.Substring(idx + 1));

                dict[key] = value;
            }

            return dict;
        }

        public struct QType<T>
        {
            public T Value;
            public float Quality;
        }

        public struct StandardRequestHeaders
        {
            public Option<string> A_IM;
            public Option<List<QType<ContentType>>> Accept;
            public Option<List<QType<string>>> Accept_Charset;
            public Option<List<QType<string>>> Accept_Encoding;
            public Option<List<QType<string>>> Accept_Language;
            public Option<DateTime> Accept_Datetime;
            /* Access-Control-Request-Method,
            Access-Control-Request-Headers */
            public Option<string> Authorization;
            public Option<string> Cache_Control;
            public Option<string> Connection;
            public Option<uint> Content_Length;
            public Option<string> Content_MD5;
            public Option<ContentType> Content_Type;
            public Option<string> Cookie;
            public Option<DateTime> Date;
            public Option<string> Expect;
            public Option<string> Forwarded;
            public Option<string> From;
            public Option<string> Host;
            public Option<string> If_Match;
            public Option<DateTime> If_Modified_Since;
            public Option<string> If_None_Match;
            public Option<string> If_Range;
            public Option<uint> Max_Forwards;
            public Option<string> Origin;
            public Option<string> Pragma;
            public Option<string> Proxy_Authorization;
            public Option<string> Range;
            public Option<string> Referer;
            public Option<string> TE;
            public Option<string> User_Agent;
            public Option<string> Upgrade;
            public Option<string> Via;
            public Option<string> Warning;
        }

        public static List<QType<T>> ParseQType<T>(String Str, Func<string, T> ParseFunc)
        {
            var ret = new List<QType<T>>();

            foreach (var tok in Str.Split(','))
            {
                if (tok.Contains(";q="))
                {
                    var idx = tok.IndexOf(";q=");

                    ret.Add(new QType<T>()
                    {
                        Value = ParseFunc(tok.Substring(0, idx)),
                        Quality = float.Parse(tok.Substring(idx + 3)),
                    });
                }
                else
                {
                    ret.Add(new QType<T>()
                    {
                        Value = ParseFunc(tok),
                        Quality = 1.0F,
                    });
                }
            }
            return ret;
        }

        public static List<QType<ContentType>> ParseAccept(string Accept)
        {
            return ParseQType(Accept, ContentType.Parse);
        }

        public static List<QType<string>> ParseAcceptCharset(string Accept_Charset)
        {
            return ParseQType(Accept_Charset, s => s);
        }

        public static List<QType<string>> ParseAcceptEncoding(string Accept_Encoding)
        {
            return ParseQType(Accept_Encoding, s => s);
        }

        public static List<QType<string>> ParseAcceptLanguage(string Accept_Language)
        {
            return ParseQType(Accept_Language, s => s);
        }

        public static DateTime ParseDate(string Date)
        {
            return DateTime.Parse(Date);
        }

        public static StandardRequestHeaders ParseRequestHeaders(IReadOnlyDictionary<string, string> RawHeaders)
        {
            Option<T> parse<T>(string key, Func<string, T> parsefunc)
            {
                if (RawHeaders.ContainsKey(key))
                {
                    return Option<T>.Some(parsefunc(RawHeaders[key]));
                }
                else
                {
                    return Option<T>.None();
                }
            }

            Option<string> parse_s(string key)
            {
                return parse<string>(key, s => s);
            }

            return new StandardRequestHeaders()
            {
                A_IM = parse_s("A-IM"),
                Accept = parse("Accept", ParseAccept),
                Accept_Charset = parse("Accept-Charset", ParseAcceptCharset),
                Accept_Encoding = parse("Accept-Encoding", ParseAcceptEncoding),
                Accept_Language = parse("Accept-Language", ParseAcceptLanguage),
                Accept_Datetime = parse("Accept-Datetime", ParseDate),
                Authorization = parse_s("Authorization"),
                Cache_Control = parse_s("Cache-Control"),
                Connection = parse_s("Connection"),
                Content_Length = parse("Content-Length", uint.Parse),
                Content_MD5 = parse_s("Content-MD5"),
                Content_Type = parse("Content-Type", ContentType.Parse),
                Cookie = parse_s("Cookie"),
                Date = parse("Date", ParseDate),
                Expect = parse_s("Expect"),
                Forwarded = parse_s("Forwarded"),
                From = parse_s("From"),
                Host = parse_s("Host"),
                If_Match = parse_s("If-Match"),
                If_Modified_Since = parse("If-Modified-Since", ParseDate),
                If_None_Match = parse_s("If-None-Match"),
                If_Range = parse_s("If-Range"),
                Max_Forwards = parse("Max-Forwards", uint.Parse),
                Origin = parse_s("Origin"),
                Pragma = parse_s("Pragma"),
                Proxy_Authorization = parse_s("Proxy-Authorization"),
                Range = parse_s("Range"),
                Referer = parse_s("Referer"),
                TE = parse_s("TE"),
                Upgrade = parse_s("Upgrade"),
                User_Agent = parse_s("User-Agent"),
                Via = parse_s("Via"),
                Warning = parse_s("Warning"),
            };
        }

        protected static Request Refine(BasicServer.Request Raw)
        {
            if (Raw is BasicServer.Request_With_Body)
            {
                return new Request_With_Body(Raw as BasicServer.Request_With_Body, ParseRequestHeaders(Raw.Metadata.Headers));
            }
            else
            {
                return new Request(Raw, ParseRequestHeaders(Raw.Metadata.Headers));
            }
        }

        public struct Query
        {
            public readonly string Path;
            public readonly IReadOnlyDictionary<string, string> Params;

            public Query(string Query)
            {
                if (Query.Contains('?'))
                {
                    var idx = Query.IndexOf('?');

                    Path = Query.Substring(0, idx);
                    Params = ParseParams(Query.Substring(idx + 1));
                }
                else
                {
                    Path = Query;
                    Params = new Dictionary<string, string>();
                }
            }
        }

        new public class Request
        {
            public readonly BasicServer.Request RawRequest;
            public readonly IReadOnlyDictionary<string, string> RawHeaders;
            public readonly Method Method;

            public readonly Query Query;
            public readonly StandardRequestHeaders Headers;


            public Request(BasicServer.Request Raw, StandardRequestHeaders Headers)
            {
                this.RawRequest = Raw;
                this.RawHeaders = Raw.Metadata.Headers;
                this.Method = Raw.Method;

                Query = new Query(Raw.Metadata.Query);
                this.Headers = Headers;
            }
        };


        public enum BodyType
        {
            Raw,
            URL_Encoded,
            FormData_Encoded
        }

        public class RequestBody
        {
            public readonly BodyType Type;

            public readonly IReadOnlyCollection<byte> Raw;

            protected RequestBody(BodyType Type, byte[] Raw)
            {
                this.Type = Type;
                this.Raw = Raw;
            }

            public RequestBody(byte[] Raw)
            {
                this.Type = BodyType.Raw;
                this.Raw = Raw;
            }
        }

        public class RequestBody_URLEncoded : RequestBody
        {
            public readonly IReadOnlyDictionary<string, string> Params;

            public RequestBody_URLEncoded(byte[] Raw) : base(BodyType.URL_Encoded, Raw)
            {
                var dict = new Dictionary<string, string>();

                foreach (var pair in Encoding.UTF8.GetString(Raw).Split('&'))
                {
                    var idx = pair.IndexOf('=');

                    var key = pair.Substring(0, idx);
                    if (dict.ContainsKey(key)) throw new ArgumentException();

                    var value = Uri.UnescapeDataString(pair.Substring(idx + 1));

                    dict[key] = value;
                }

                Params = dict;
            }
        }

        public class RequestBody_FormDataEncoded : RequestBody
        {
            public RequestBody_FormDataEncoded(byte[] Raw) : base(BodyType.FormData_Encoded, Raw)
            {
                throw new NotImplementedException();
            }
        }

        new public class Request_With_Body : Request
        {
            public readonly RequestBody Body;

            public Request_With_Body(BasicServer.Request_With_Body Raw, StandardRequestHeaders Headers) : base(Raw, Headers)
            {
                if (Headers.Content_Type.HasValue)
                {
                    if (Headers.Content_Type.Value.Equals("application/x-www-form-urlencoded"))
                    {
                        Body = new RequestBody_URLEncoded(Raw.Body);
                    }
                    else if (Headers.Content_Type.Value.Equals("multipart/form-data"))
                    {
                        Body = new RequestBody_FormDataEncoded(Raw.Body);
                    }
                    else Body = new RequestBody(Raw.Body);
                }
                else Body = new RequestBody(Raw.Body);
            }
        }

        public AdvancedServer(IPEndPoint Endpoint, IEnumerable<Method> AcceptedMethods) : base(Endpoint, AcceptedMethods)
        {

        }

        public virtual Response Handle_GET(Request Request) => RenderServerError(StatusCode.NOT_IMPLEMENTED);

        public virtual Response Handle_HEAD(Request Request) => RenderServerError(StatusCode.NOT_IMPLEMENTED);

        public virtual Response Handle_POST(Request_With_Body Request) => RenderServerError(StatusCode.NOT_IMPLEMENTED);

        public virtual Response Handle_PUT(Request_With_Body Request) => RenderServerError(StatusCode.NOT_IMPLEMENTED);

        public virtual Response Handle_DELETE(Request Request) => RenderServerError(StatusCode.NOT_IMPLEMENTED);

        public virtual Response Handle_TRACE(Request Request) => RenderServerError(StatusCode.NOT_IMPLEMENTED);

        public virtual Response Handle_OPTIONS(Request Request) => RenderServerError(StatusCode.NOT_IMPLEMENTED);

        public virtual Response Handle_CONNECT(Request Request) => RenderServerError(StatusCode.NOT_IMPLEMENTED);

        public virtual Response Handle_PATCH(Request_With_Body Request) => RenderServerError(StatusCode.NOT_IMPLEMENTED);

        public sealed override Response Handle(BasicServer.Request Request)
        {
            Request request;

            try
            {
                request = Refine(Request);
            }
            catch (Exception e)
            {
                ErrorLog(e);
                return RenderServerError(StatusCode.BAD_REQUEST);
            }

            Response response;

            try
            {
                switch (request.Method)
                {
                    case Method.GET:
                        response = Handle_GET(request);
                        break;
                    case Method.HEAD:
                        response = Handle_HEAD(request);
                        break;
                    case Method.POST:
                        response = Handle_POST(request as Request_With_Body);
                        break;
                    case Method.PUT:
                        response = Handle_PUT(request as Request_With_Body);
                        break;
                    case Method.DELETE:
                        response = Handle_DELETE(request);
                        break;
                    case Method.TRACE:
                        response = Handle_TRACE(request);
                        break;
                    case Method.OPTIONS:
                        response = Handle_OPTIONS(request);
                        break;
                    case Method.CONNECT:
                        response = Handle_CONNECT(request);
                        break;
                    case Method.PATCH:
                        response = Handle_PATCH(request as Request_With_Body);
                        break;
                    default:
                        response = RenderServerError(StatusCode.BAD_REQUEST);
                        break;
                }

            } catch(Exception e)
            {
                ErrorLog(e);
                response = RenderServerError(StatusCode.INTERNAL_SERVER_ERROR);
            }

            if (response.Body != null && !response.Headers.ContainsKey("Content-Encoding"))
            {
                if (request.Headers.Accept_Encoding.HasValue)
                {
                    foreach (var encoding in request.Headers.Accept_Encoding.Value.OrderBy(encoding => -encoding.Quality).Select(qval => qval.Value))
                    {
                        switch (encoding)
                        {
                            case "gzip":
                                using (var compressed = new MemoryStream())
                                {
                                    using (var gzip = new GZipStream(compressed, CompressionMode.Compress))
                                    {
                                        gzip.Write(response.Body, 0, response.Body.Length);
                                    }

                                    response.Body = compressed.ToArray();
                                    response.Headers["Content-Length"] = response.Body.Length.ToString();
                                    response.Headers["Content-Encoding"] = "gzip";
                                }
                                break;

                            case "deflate":
                                using (var compressed = new MemoryStream())
                                {
                                    using (var deflate = new DeflateStream(compressed, CompressionMode.Compress))
                                    {
                                        deflate.Write(response.Body, 0, response.Body.Length);
                                    }

                                    response.Body = compressed.ToArray();
                                    response.Headers["Content-Length"] = response.Body.Length.ToString();
                                    response.Headers["Content-Encoding"] = "deflate";
                                }
                                break;

                            case "identity":
                                response.Headers["Content-Length"] = response.Body.Length.ToString();
                                response.Headers["Content-Encoding"] = "identity";
                                break;
                        }

                        if (response.Headers.ContainsKey("Content-Encoding")) break;
                    }
                }
            }

            return response;
        }

        protected override void WriteResponse(Socket ClientSocket, BasicServer.Request Request, Response Response)
        {
            if (Response.Headers == null) throw new ArgumentException("headers missing");

            if (Response.Body != null)
            {
                if (Request == null || Response_Has_Body(Request.Method))
                {
                    if (!Response.Headers.ContainsKey("Content-Length"))
                    {
                        Response.Headers["Content-Length"] = Response.Body.Length.ToString();
                    }
                }
            }

            base.WriteResponse(ClientSocket, Request, Response);
        }

    }
}
