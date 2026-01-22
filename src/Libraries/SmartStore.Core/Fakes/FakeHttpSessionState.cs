using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.SessionState;

namespace SmartStore.Core.Fakes
{
    public class FakeHttpSessionState : HttpSessionStateBase, IEnumerable
    {
        private readonly Dictionary<string, object> _sessionItems;

        public FakeHttpSessionState(Dictionary<string, object> sessionItems)
        {
            _sessionItems = sessionItems ?? new Dictionary<string, object>();
        }

        public override int Count => _sessionItems.Count;

        public override NameObjectCollectionBase.KeysCollection Keys
        {
            get
            {
                var collection = new NameValueCollection();
                foreach (var key in _sessionItems.Keys)
                {
                    collection.Add(key, null);
                }
                return collection.Keys;
            }
        }

        public override object this[string name]
        {
            get => _sessionItems.ContainsKey(name) ? _sessionItems[name] : null;
            set => _sessionItems[name] = value;
        }

        public bool Exists(string key)
        {
            return _sessionItems.ContainsKey(key) && _sessionItems[key] != null;
        }



        public override void Add(string name, object value)
        {
            _sessionItems[name] = value;
        }

        public IEnumerator GetEnumerator()
        {
            return _sessionItems.Keys.GetEnumerator();
        }

        public override void Remove(string name)
        {
            _sessionItems.Remove(name);
        }
    }
}
