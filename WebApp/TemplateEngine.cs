using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp
{
    public class TemplateEngine
    {
        public class Template
        {
            public readonly string Source;
            public readonly TemplateEngine Engine;

            public Template(TemplateEngine Engine, string Source)
            {
                this.Engine = Engine;
                this.Source = Source;
            }

            public string With(Dictionary<string, string> Vars)
            {
                var builder = new StringBuilder();

                for(int i = 0; i < Source.Length; i++)
                {
                    if(Source[i] == '{' && i < Source.Length - 1 && Source[i+1] == '{')
                    {
                        int end = Match(Source, i);

                        builder.Append(Engine.Evaluate(Source.Substring(i + 2, end - (i+2)), Vars));
                        i = end + 1;
                    } else
                    {
                        builder.Append(Source[i]);
                    }
                }

                return builder.ToString();
            }
        }

        public readonly string TemplateDir;

        public TemplateEngine(string Dir)
        {
            if(!Directory.Exists(Dir))
            {
                throw new ArgumentException($"Directory {Dir} not found");
            }

            TemplateDir = Dir;
        }

        public Template Load(string TemplatePath) => new Template(this, string.Join("\n", File.ReadAllText(Path.Combine(TemplateDir, Path.Combine(TemplatePath.Split('/').Where(s => s.Length > 0).ToArray())))));

        public string Evaluate(string Expression, Dictionary<string, string> Vars)
        {
            var trimmed = Expression.Trim();

            if (trimmed[0] == '$')
            {
                return Vars[trimmed.Substring(1)];
            }
            else throw new ArgumentException($"Unable to evaluate expression {Expression}");
        }

        public static int Match(string Str, int StartIndex)
        {
            string start;
            string end;

            switch (Str[StartIndex])
            {
                case '{':
                    if (StartIndex >= Str.Length - 1) throw new ArgumentException();
                    else if (Str[StartIndex + 1] == '{')
                    {
                        start = "{{";
                        end = "}}";
                    }
                    else throw new ArgumentException();
                    break;

                case '"':
                    if (StartIndex >= Str.Length - 1) throw new ArgumentException();
                    else
                    {
                        start = "\"";
                        end = "\"";
                    }
                    break;

                default:
                    throw new ArgumentException();
            }

            for(int i = StartIndex + start.Length; i < Str.Length; i++)
            {
                if(i + end.Length >= Str.Length)
                {
                    throw new ArgumentException();
                } else if(Str.Substring(i, end.Length) == end)
                {
                    return i;
                }
            }

            throw new ArgumentException();
        }
    }
}