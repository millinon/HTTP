using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTTP
{
    public struct ContentType
    {
        public readonly string Type;
        public readonly string SubType;

        public readonly Option<string> Boundary;
        public readonly Option<string> Charset;

        private ContentType(string Type, string SubType, string Boundary = null, string Charset = null)
        {
            this.Type = Type;
            this.SubType = SubType;

            this.Boundary = (Boundary ?? Option<string>.None());
            this.Charset = (Charset ?? Option<string>.None());
        }

        public static ContentType Parse(string Str)
        {
            if (Str == null) throw new ArgumentException("null passed to ContentType constructor");

            if (Str.Contains(';'))
            {
                var toks = Str.Split(';').Select(s => s.Trim()).ToArray();

                var ct = Parse(toks[0]);

                string boundary = null;
                string charset = null;

                for(int i = 1; i < toks.Length; i++)
                {
                    if (toks[i].StartsWith("boundary="))
                    {
                        if (boundary != null) throw new ArgumentException();
                        boundary = toks[i].Substring("boundary=".Length);
                    }
                    else if (toks[i].StartsWith("charset="))
                    {
                        if (charset != null) throw new ArgumentException();
                        charset = toks[i].Substring("charset=".Length);
                    }
                    else throw new ArgumentException();
                }

                return new ContentType(ct.Type, ct.SubType, boundary, charset);

            }
            else
            {
                var toks = Str.Split('/');

                if (toks.Length != 2)
                {
                    throw new FormatException($"invalid content-type: {Str}");
                }
                else return new ContentType(toks[0], toks[1]);
            }
        }

        public static bool TryParse(string Str, out ContentType ContentType)
        {
            try
            {
                ContentType = Parse(Str);
                return true;
            }
            catch (Exception e)
            {
                ContentType = default(ContentType);
                return false;
            }
        }

        public override string ToString()
        {
            return $"{Type}/{SubType}";
        }

        public bool Equals(ContentType rhs)
        {
            return Type == rhs.Type && SubType == rhs.SubType;
        }

        public static implicit operator ContentType(string str)
        {
            return Parse(str);
        }
    }
}
