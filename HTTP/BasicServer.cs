using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace HTTP
{
    public abstract class BasicServer : IDisposable
    {
        private Socket ListenSocket;

        public readonly IPAddress SERVER_IP;
        public readonly IReadOnlyCollection<Method> AcceptedMethods;

        private readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        public static bool Request_Must_Have_Body(Method Method) => Method == Method.POST || Method == Method.PUT || Method == Method.PATCH;
        public static bool Request_May_Have_Body(Method Method) => Request_Must_Have_Body(Method) || Method == Method.DELETE;
        public static bool Response_Has_Body(Method Method) => Method == Method.GET || Method == Method.POST || Method == Method.DELETE || Method == Method.CONNECT || Method == Method.PATCH;

        public enum StatusType
        {
            STOPPED,
            RUNNING,
            ERROR
        }

        protected readonly object _statusLock = new object();
        protected StatusType _status;
        public StatusType Status
        {
            get
            {
                return _status;

            }

            private set
            {
                _status = value;

            }
        }
        
        public static Dictionary<string, string> DefaultHeaders()
        {
            var headers = new Dictionary<string, string>();

            headers["Date"] = $"{DateTime.Now.ToUniversalTime().ToString("ddd, dd MMM yyyy HH:mm:ss")} GMT";
            headers["Server"] = $"Millinon's HTTP Server ({Environment.OSVersion.Platform})";
            headers["Connection"] = "Close";

            return headers;
        }

        public struct Response
        {
            public StatusCode Status;

            public Dictionary<string, string> Headers;

            public byte[] Body;
        }

        public abstract Response Handle(Request Request);

        public virtual Response RenderServerError(StatusCode Status)
        {
            return new Response()
            {
                Status = Status,
                Headers = DefaultHeaders(),
                Body = Encoding.UTF8.GetBytes($"{(int)Status} {Status.ToFriendlyString()}"),
            };
        }

        protected object LogLock = new object();
        public abstract void AccessLog(Request Request);
        public abstract void ErrorLog(string Message);

        public BasicServer(IPAddress IP, IEnumerable<Method> AcceptedMethods)
        {
            SERVER_IP = IP;

            this.AcceptedMethods = (IReadOnlyCollection<Method>)new HashSet<Method>(AcceptedMethods);

            if (!this.AcceptedMethods.Contains(Method.GET) || !this.AcceptedMethods.Contains(Method.HEAD))
            {
                throw new ArgumentException("HTTP server must implement GET and HEAD at a minimum");
            }

            Status = StatusType.STOPPED;
        }

        private readonly object _clientThreadsLock = new object();
        private readonly HashSet<Thread> ClientThreads = new HashSet<Thread>();

        private void HandleListeningSocket(Socket ListenSocket, CancellationToken CancelToken)
        {
            while (!CancelToken.IsCancellationRequested)
            {
                try
                {
                    var client_sock = ListenSocket.Accept();

                    var thread = new Thread(() => HandleClientSocket(client_sock));
                    ClientThreads.Add(thread);

                    thread.Start();
                }
                catch (Exception)
                {
                    lock (_statusLock)
                    {
                        Status = StatusType.ERROR;

                        lock (_clientThreadsLock)
                        {
                            foreach (var thread in ClientThreads)
                            {
                                thread.Abort();
                            }
                        }

                        try { ListenSocket.Close(); } catch (Exception) { };

                        return;
                    }
                }
            }

            ListenSocket.Close();

            ClientThreads.Remove(Thread.CurrentThread);
        }

        protected virtual void WriteResponse(Socket ClientSocket, Request Request, Response Response)
        {
            ClientSocket.Send(Encoding.ASCII.GetBytes($"HTTP/1.1 {(int)Response.Status} {Response.Status.ToFriendlyString()}\r\n"));
            
            foreach(var header in Response.Headers)
            {
                ClientSocket.Send(Encoding.ASCII.GetBytes($"{header.Key}: {header.Value}\r\n"));
            }

            ClientSocket.Send(Encoding.ASCII.GetBytes("\r\n"));

            if(Request != null && Response.Body != null)  ClientSocket.Send(Response.Body);

            ClientSocket.Close();
        }

        private readonly Regex RequestRegex = new Regex("^(?<method>GET|HEAD|POST|PUT|DELETE|TRACE|OPTIONS|CONNECT|PATCH)\\s(?<query_string>[^\\s]+)\\sHTTP/1\\.1$");
        private readonly Regex HeaderRegex = new Regex("^(?<name>[A-Za-z0-9_-]+): (?<value>.*)$");

        public struct RequestMetadata
        {
            public IPAddress ClientIP;
            public IReadOnlyDictionary<string, string> Headers;
            public string Query;
        }

        public class Request
        {
            public readonly Method Method;
            public readonly RequestMetadata Metadata;

            public Request(Method Method, RequestMetadata Metadata)
            {
                this.Method = Method;
                this.Metadata = Metadata;
            }
        }

        public class Request_With_Body : Request
        {
            public IReadOnlyCollection<byte> Body;

            public Request_With_Body(Method Method, RequestMetadata Metadata, byte[] Body) : base(Method, Metadata)
            {
                if (! Request_May_Have_Body(Method))
                {
                    throw new ArgumentException();
                }

                this.Body = Body;
            }
        }

        protected Request ReadClientRequest(Socket ClientSocket)
        {
            byte[] buf = new byte[1024 * 1024 * 512];

            int total_read = 0;
            int read;
            int search_idx = 0;
            bool found_query = false;
            byte[] query_buf = null;
            int headers_start_pos = 0;
            string query_string = null;

            while (total_read < buf.Length && !found_query)
            {
                read = ClientSocket.Receive(buf, total_read, buf.Length - total_read, SocketFlags.None);

                if (read < 0)
                {
                    WriteResponse(ClientSocket, null, RenderServerError(StatusCode.BAD_REQUEST));
                    throw new Exception("ClientSocket.Receive failed");
                }
                else
                {
                    total_read += read;

                    for (int i = search_idx; i < total_read; i++)
                    {
                        if (buf[i] == '\r')
                        {
                            if (total_read > i + 1)
                            {
                                if (buf[i + 1] == '\n')
                                {
                                    query_buf = new byte[i];
                                    Array.Copy(buf, query_buf, i);

                                    headers_start_pos = i + 2;

                                    found_query = true;
                                    break;
                                }
                                else
                                {
                                    search_idx = i + 2;
                                }
                            }
                            else search_idx = i;
                        }
                    }
                }
            }

            if (!found_query)
            {
                WriteResponse(ClientSocket, null, RenderServerError(StatusCode.BAD_REQUEST));
                throw new Exception("Invalid HTTP query");
            }
            else
            {
                var match = RequestRegex.Match(Encoding.ASCII.GetString(query_buf));

                if (!match.Success)
                {
                    WriteResponse(ClientSocket, null, RenderServerError(StatusCode.BAD_REQUEST));
                    throw new Exception("Invalid HTTP query");
                }
                else
                {
                    var method = match.Groups["method"].Value;
                    query_string = match.Groups["query_string"].Value;

                    Method requested_method;

                    if (!Enum.TryParse(method, out requested_method))
                    {
                        WriteResponse(ClientSocket, null, RenderServerError(StatusCode.BAD_REQUEST));
                        throw new Exception("Invalid HTTP method");
                    }
                    else if (!AcceptedMethods.Contains(requested_method))
                    {
                        WriteResponse(ClientSocket, null, RenderServerError(StatusCode.BAD_REQUEST));
                        throw new Exception("Invalid HTTP method");
                    }
                    else
                    {
                        int unread_bytes = total_read - headers_start_pos;
                        Array.Copy(buf, headers_start_pos, buf, 0, unread_bytes);

                        bool found_headers_end = false;
                        search_idx = 0;

                        List<string> header_strs = new List<string>();

                        do
                        {
                            for (int i = search_idx; i + 3 < unread_bytes && !found_headers_end; i++)
                            {
                                if (buf[i] == '\r' && buf[i + 1] == '\n')
                                {
                                    if (i > 0)
                                    {
                                        header_strs.Add(Encoding.ASCII.GetString(buf.Take(i).ToArray()));
                                        Array.Copy(buf, i + 2, buf, 0, unread_bytes - (i + 2));

                                        unread_bytes -= (i + 2);
                                        search_idx = 0;
                                        i = 0;

                                        if (buf[i] == '\r' && buf[i + 1] == '\n')
                                        {
                                            found_headers_end = true;

                                            Array.Copy(buf, 2, buf, 0, unread_bytes - 2);

                                            unread_bytes -= 2;

                                            break;
                                        }
                                    }
                                    else if (buf[i + 2] == '\r' && buf[i + 3] == '\n')
                                    {
                                        found_headers_end = true;

                                        Array.Copy(buf, 2, buf, 0, unread_bytes - 2);

                                        unread_bytes -= 2;

                                        break;
                                    }
                                }
                                else
                                {
                                    search_idx = i + 1;
                                }
                            }

                            if (!found_headers_end)
                            {
                                if (unread_bytes >= buf.Length)
                                {
                                    WriteResponse(ClientSocket, null, RenderServerError(StatusCode.BAD_REQUEST));
                                    throw new Exception("Header buffer overflow");
                                }
                                else
                                {
                                    read = ClientSocket.Receive(buf, unread_bytes, buf.Length - unread_bytes, SocketFlags.None);

                                    if (read < 0)
                                    {
                                        WriteResponse(ClientSocket, null, RenderServerError(StatusCode.BAD_REQUEST));
                                        throw new Exception("ClientSocket.Receive failed");
                                    }
                                }
                            }
                        } while (!found_headers_end);

                        Dictionary<string, string> headers = new Dictionary<string, string>();

                        foreach (var header_str in header_strs)
                        {
                            match = HeaderRegex.Match(header_str);

                            if (!match.Success)
                            {
                                WriteResponse(ClientSocket, null, RenderServerError(StatusCode.BAD_REQUEST));
                                throw new Exception($"Invalid header: \"{header_str}\"");
                            }
                            else if (headers.ContainsKey(match.Groups["name"].Value))
                            {
                                WriteResponse(ClientSocket, null, RenderServerError(StatusCode.BAD_REQUEST));
                                throw new Exception($"duplicate header: \"{header_str}\"");
                            }
                            else
                            {
                                headers[match.Groups["name"].Value] = match.Groups["value"].Value;

                                Console.WriteLine($"headers[{match.Groups["name"].Value}] = \"{match.Groups["value"].Value}\"");
                            }
                        }
                        
                        Request request;

                        var metadata = new RequestMetadata()
                        {
                            ClientIP = (ClientSocket.RemoteEndPoint as IPEndPoint).Address,
                            Query = query_string,
                            Headers = headers,
                        };

                        byte[] body = null;

                        if (Request_Must_Have_Body(requested_method))
                        {
                            if (! headers.ContainsKey("Content-Length"))
                            {
                                WriteResponse(ClientSocket, null, RenderServerError(StatusCode.BAD_REQUEST));
                                throw new Exception("Content-Length header missing");
                            }

                            int content_length;

                            if (!int.TryParse(headers["Content-Length"], out content_length))
                            {
                                WriteResponse(ClientSocket, null, RenderServerError(StatusCode.BAD_REQUEST));
                                throw new Exception($"Invalid Content-Length: {metadata.Headers["Content-Length"]}");
                            }
                            else
                            {
                                body = new byte[content_length];

                                Array.Copy(buf, body, unread_bytes);

                                total_read = unread_bytes;

                                while (total_read < content_length)
                                {
                                    read = ClientSocket.Receive(body, total_read, content_length - total_read, SocketFlags.None);

                                    if (read < 0)
                                    {
                                        WriteResponse(ClientSocket, null, RenderServerError(StatusCode.BAD_REQUEST));
                                        throw new Exception("ClientSocket.Receive failed");
                                    }
                                }
                            }
                        }

                        if (body != null && Request_Must_Have_Body(requested_method))
                        {
                            request = new Request_With_Body(requested_method, metadata, body);
                        }
                        else
                        {
                            request = new Request(requested_method, metadata);
                        }

                        AccessLog(request);

                        return request;
                    }
                }
            }
        }

        protected void HandleClientSocket(Socket ClientSocket)
        {
            bool have_error = false;

            Request request = null;

            try
            {
                request = ReadClientRequest(ClientSocket);
            } catch(Exception ex)
            {
                ErrorLog(ex.Message);
                have_error = true;
            }

            if (!have_error)
            {
                Response response;
                
                    try
                    {
                        response = Handle(request);
                    }
                    catch (Exception e)
                    {
                    ErrorLog(e.Message);
                        response = RenderServerError(StatusCode.INTERNAL_SERVER_ERROR);
                    }

                WriteResponse(ClientSocket, request, response);
            }
        }

        public void Start(ushort Port)
        {
            lock (_statusLock)
            {
                if (Status == StatusType.RUNNING)
                {
                    throw new InvalidOperationException("Server already running");
                }

                try
                {
                    ListenSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                    var endpoint = new IPEndPoint(SERVER_IP, Port);
                    ListenSocket.Bind(endpoint);
                    ListenSocket.Listen(1024);

                    var listen_thread = new Thread(() => HandleListeningSocket(ListenSocket, CancellationTokenSource.Token));

                    listen_thread.Start();

                    Status = StatusType.RUNNING;
                }
                catch (Exception)
                {
                    Status = StatusType.ERROR;
                    throw;
                }
            }
        }

        public void Stop()
        {
            lock (_statusLock)
            {
                if (Status != StatusType.RUNNING)
                {
                    throw new InvalidOperationException("Server not running");
                }

                try
                {
                    ListenSocket.Close();
                    
                    foreach(var thread in ClientThreads.Where(thread => thread.IsAlive))
                    {
                        thread.Abort();
                    }

                    Status = StatusType.STOPPED;
                }
                catch (Exception)
                {
                    Status = StatusType.ERROR;
                    throw;
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    try { Stop(); } catch (InvalidOperationException) { }
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}