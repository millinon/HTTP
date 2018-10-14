using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.SessionStores
{
    public class DictionarySession : WebApp.Session
    {
        private readonly MemoryStore Store;

        public DictionarySession(MemoryStore Store, string ID) : base(ID)
        {
            this.Store = Store;
        }

        public string this[string Key]
        {
            get
            {
                return Store.Lookup(ID, Key);
            }
            set
            {
                Store.Set(ID, Key, value);
            }
        }

        public bool HasKey(string Key) => Store.HasKey(ID, Key);
        public IEnumerable<string> Keys => Store.Keys(ID);
    }

    public class MemoryStore : WebApp.ISessionStore<DictionarySession>
    {
        private readonly Dictionary<string, Dictionary<string, string>> Data;

        public MemoryStore()
        {
            Data = new Dictionary<string, Dictionary<string, string>>();
        }

        public void Close(DictionarySession Session)
        {
        }

        public DictionarySession Create()
        {
            var id = Guid.NewGuid().ToString();

            if(!Data.ContainsKey(id))
            {
                Data[id] = new Dictionary<string, string>();
            }

            return Open(id);
        }

        public DictionarySession Open(string ID)
        {
            if(! HasSession(ID))
            {
                throw new ArgumentException();
            }

            return new DictionarySession(this, ID);
        }

        public void Destroy(DictionarySession Session)
        {
            if (!Data.ContainsKey(Session.ID))
            {
                throw new ArgumentException();
            }

            Data.Remove(Session.ID);
        }

        public bool HasSession(string ID) => Data.ContainsKey(ID);
        public bool HasKey(string ID, string Key) => Data[ID].ContainsKey(Key);
        public IEnumerable<string> Keys(string ID) => Data[ID].Keys;

        public string Lookup(string ID, string Key) => Data[ID][Key];
        public void Set(string ID, string Key, string Value) => Data[ID][Key] = Value;

        public IEnumerable<string> Sessions => Data.Keys;
    }
}
