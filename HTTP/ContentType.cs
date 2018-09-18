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

        private ContentType(string Type, string SubType)
        {
            this.Type = Type;
            this.SubType = SubType;
        }

        public static ContentType Parse(string Str)
        {
            if (Str == null) throw new ArgumentException("null passed to ContentType constructor");

            var toks = Str.Split('/');

            if (toks.Length != 2)
            {
                throw new FormatException($"invalid content-type: {Str}");
            }
            else return new ContentType(toks[0], toks[1]);
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
    }
}
