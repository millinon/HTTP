using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HTTP;

namespace WebApp
{
    using MatchAction = Func<AdvancedServer.Request,  BasicServer.Response>;
    using VarMatchAction = Func<AdvancedServer.Request, Dictionary<string, string>, BasicServer.Response>;

    public class Router
    {
        public readonly string Path;
        private Func<AdvancedServer.Request, Dictionary<string, string>, BasicServer.Response> Handler;
        private readonly bool IsVar;
        public readonly string VarKey;
        private readonly Router Root;

        public bool HaveVarKey(string key)
        {
            return VarChildren.Keys.Select(k => k.TrimStart('$')).Contains(key) || VarChildren.Values.Where(child => child.HaveVarKey(key)).Any() || Children.Values.Where(child => child.HaveVarKey(key)).Any();
        }

        public bool IsTerminal
        {
            get
            {
                return Handler != null;
            }
        }

        private readonly Dictionary<string, Router> Children;
        private readonly Dictionary<string, Router> VarChildren;

        public IEnumerable<string> Routes
        {
            get
            {
                IEnumerable<string> routes;
                if (IsTerminal) routes = new List<string>() { Path };
                else routes = new List<string>();

                routes = routes.Concat(Children.Values.Select(child => child.Path));
                routes = routes.Concat(VarChildren.Values.Select(child => child.Path));

                return routes;
            }
        }

        private Router(string Path, Router Root, string VarKey)
        {

            this.Path = Path;

            Children = new Dictionary<string, Router>();
            VarChildren = new Dictionary<string, Router>();

            Handler = null;

            this.Root = (Root == null ? this : Root);

            this.VarKey = VarKey;
            this.IsVar = (VarKey == null);
        }

        public Router(string Path = "/", Router Root = null) : this(Path, Root, null)
        {
        }
        
        public Router(string Path, string VarKey, Router Root = null) : this(Path, Root, VarKey)
        {
        }

        public Router AddRoute(IEnumerable<string> PathToks, VarMatchAction Handler)
        {
            if(PathToks.Count() == 0)
            {
                if(IsTerminal)
                {
                    throw new ArgumentException($"Duplicate handler for path: {Path}");
                } else
                {
                    this.Handler = Handler;

                    return this;
                }
            } else
            {
                var next = PathToks.First();
                var remaining = PathToks.Skip(1);

                if (next.StartsWith("$"))
                {
                    if (! VarChildren.ContainsKey(next))
                    {
                        var varkey = next.TrimStart('$');

                        if (Root.HaveVarKey(varkey))
                        {
                            throw new ArgumentException($"Duplicate var key detected: {varkey}");
                        }

                        VarChildren[next] = new Router($"{Path.TrimStart('/')}/{next}", varkey, Root);
                    }

                    return VarChildren[next].AddRoute(remaining, Handler);
                } else
                {
                    if (!Children.ContainsKey(next))
                    {
                        Children[next] = new Router($"{Path.TrimStart('/')}/{next}", next, Root);
                    }

                    return Children[next].AddRoute(remaining, Handler);
                }
            }
        }

        public Router AddRoute(string Path, VarMatchAction Handler) => AddRoute(Path.Split('/').Where(s => s.Length > 0), Handler);

        public MatchAction Match(IEnumerable<string> PathToks, Dictionary<string, string> Vars)
        {
            var newvars = new Dictionary<string, string>(Vars);

            if(PathToks.Count() == 0)
            {
                if (IsTerminal)
                {
                    return (r => Handler(r, newvars));
                }
                else
                {
                    throw new ArgumentException("No matching route found");
                }
            } else
            {
                var next = PathToks.First();
                var remaining = PathToks.Skip(1);

                if (Children.ContainsKey(next))
                {
                    return Children[next].Match(remaining, newvars);
                }

                if(VarChildren.Count() > 0)
                {
                    var matches = VarChildren.Values.Where(route => route.IsMatch(remaining));

                    if(matches.Count() > 1)
                    {
                        throw new InvalidOperationException("Ambiguous path detected");
                    } else if(matches.Count() == 0)
                    {
                        throw new ArgumentException("No matching route found");
                    } else
                    {
                        var matching_router = matches.First();

                        newvars[matching_router.VarKey] = next;

                        return matching_router.Match(remaining, newvars);
                    }
                }

                throw new ArgumentException("No matching route found");
            }
        }

        public MatchAction Match(string Path) => Match(Path.Split('/').Where(s => s.Length > 0), new Dictionary<string, string>());

        public bool IsMatch(IEnumerable<string> PathToks)
        {
            if(PathToks.Count() == 0)
            {
                return IsTerminal;
            } else
            {
                var next = PathToks.First();
                var remaining = PathToks.Skip(1);

                if (Children.ContainsKey(next))
                {
                    return Children[next].IsMatch(remaining);
                }

                if(VarChildren.Values.Count() > 0)
                {
                    return VarChildren.Values.Where(route => route.IsMatch(remaining)).Any();
                }

                return false;
            }
        }

        public bool IsMatch(string Path) => IsMatch(Path.Split('/').Where(s => s.Length > 0));

        public Router Lookup(IEnumerable<string> PathToks)
        {
            if(PathToks.Count() == 0)
            {
                return this;
            } else
            {
                var next = PathToks.First();
                var remaining = PathToks.Skip(1);

                if (next.StartsWith("$"))
                {
                    var varkey = next.TrimStart('$');

                    if(!VarChildren.ContainsKey(next))
                    {
                        if (Root.HaveVarKey(varkey))
                        {
                            throw new ArgumentException($"Duplicate var key detected: {varkey}");
                        }

                        VarChildren[next] = new Router($"{Path.TrimStart('/')}/{next}", varkey, Root);
                    }
                    
                    return VarChildren[next].Lookup(remaining);
                    
                } else
                {
                    if(!Children.ContainsKey(next))
                    {
                        Children[next] = new Router($"{Path.TrimStart('/')}/{next}", Root);
                    }

                    return Children[next];
                }
            }
        }

        public Router Lookup(string Path) => Lookup(Path.Split('/').Where(s => s.Length > 0));
    }

    public class RouterCollection
    {
        private readonly Dictionary<Method, Router> RootRouters;

        public IEnumerable<string> Routes
        {
            get
            {
                IEnumerable<string> routes = new List<string>();

                foreach(var key in RootRouters.Keys)
                {
                    routes = routes.Concat(RootRouters[key].Routes.Select(route => $"{key} {route}"));
                }

                return routes;
            }
        }

        public RouterCollection()
        {
            RootRouters = new Dictionary<Method, Router>();
        }

        public MatchAction Match(Method Method, string Path)
        {
            if(!RootRouters.ContainsKey(Method))
            {
                throw new ArgumentException($"No router installed for method {Method}");
            }

            return RootRouters[Method].Match(Path);
        }

        public bool IsMatch(Method Method, string Path)
        {
            if(!RootRouters.ContainsKey(Method))
            {
                return false;
            }

            return RootRouters[Method].IsMatch(Path);
        }

        public Router AddRoute(Method Method, string Path, MatchAction Handler) => AddRoute(Method, Path, (r, d) => Handler(r));

        public Router AddRoute(Method Method, string Path, VarMatchAction Handler)
        {
            if(!RootRouters.ContainsKey(Method))
            {
                RootRouters[Method] = new Router();
            }

            return RootRouters[Method].AddRoute(Path, Handler);
        }

        public Router Lookup(Method Method, string Path)
        {
            if(!RootRouters.ContainsKey(Method))
            {
                RootRouters[Method] = new Router();
            }

            return RootRouters[Method].Lookup(Path);
        }

        public Router GET(string Path) => Lookup(Method.GET, Path);
        public Router GET(string Path, MatchAction Handler) => AddRoute(Method.GET, Path, Handler);
        public Router GET(string Path, VarMatchAction Handler) => AddRoute(Method.GET, Path, Handler);

        public Router HEAD(string Path) => Lookup(Method.HEAD, Path);
        public Router HEAD(string Path, MatchAction Handler) => AddRoute(Method.HEAD, Path, Handler);
        public Router HEAD(string Path, VarMatchAction Handler) => AddRoute(Method.HEAD, Path, Handler);

        public Router POST(string Path) => Lookup(Method.POST, Path);
        public Router POST(string Path, MatchAction Handler) => AddRoute(Method.POST, Path, Handler);
        public Router POST(string Path, VarMatchAction Handler) => AddRoute(Method.POST, Path, Handler);

        public Router PUT(string Path) => Lookup(Method.PUT, Path);
        public Router PUT(string Path, MatchAction Handler) => AddRoute(Method.PUT, Path, Handler);
        public Router PUT(string Path, VarMatchAction Handler) => AddRoute(Method.PUT, Path, Handler);

        public Router DELETE(string Path) => Lookup(Method.DELETE, Path);
        public Router DELETE(string Path, MatchAction Handler) => AddRoute(Method.DELETE, Path, Handler);
        public Router DELETE(string Path, VarMatchAction Handler) => AddRoute(Method.DELETE, Path, Handler);
    }
}
