using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp
{
    public class Session
    {
        public readonly string ID;

        public Session(string ID)
        {
            this.ID = ID;
        }
    }

    public interface ISessionStore<T> where T:Session
    {
        T Create();
        T Open(string ID);
        void Close(T Session);
        void Destroy(T Session);
        bool HasSession(string ID);
    }
}
