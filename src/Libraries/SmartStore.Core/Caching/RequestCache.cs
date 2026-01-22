using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SmartStore.Utilities;
using Microsoft.AspNetCore.Http;


namespace SmartStore.Core.Caching
{
    public class RequestCache : DisposableObject, IRequestCache
    {
        const string RegionName = "SmartStoreNET:";

        private readonly IDictionary<string, object> _emptyDictionary = new Dictionary<string, object>();

        private readonly HttpContext _context;

        public RequestCache(HttpContext context)
        {
            _context = context;
        }

        public T Get<T>(string key)
        {
            return Get<T>(key, null);
        }

        public T Get<T>(string key, Func<T> acquirer)
        {
            var items = GetItems();

            key = BuildKey(key);

            if (items.Contains(key))
            {
                return (T)items[key];
            }

            if (acquirer != null)
            {
                var value = acquirer();
                items.Add(key, value);
                return value;
            }

            return default(T);
        }

        public void Put(string key, object value)
        {
            var items = GetItems();

            key = BuildKey(key);

            if (items.Contains(key))
                items[key] = value;
            else
                items.Add(key, value);
        }

        public void Clear()
        {
            RemoveByPattern("*");
        }

        public bool Contains(string key)
        {
            return GetItems().Contains(BuildKey(key));
        }

        public void Remove(string key)
        {
            GetItems().Remove(BuildKey(key));
        }

        public void RemoveByPattern(string pattern)
        {
            var items = GetItems();

            var keysToRemove = Keys(pattern).ToArray();

            foreach (string key in keysToRemove)
            {
                items.Remove(BuildKey(key));
            }
        }

        protected IDictionary<string, object> GetItems()
        {
            return (IDictionary<string, object>)_context.Items ?? _emptyDictionary;
        }

        public IEnumerable<string> Keys(string pattern)
        {
            var items = GetItems();

            if (items.Count == 0)
                yield break;

            var prefixLen = RegionName.Length;

            pattern = pattern.NullEmpty() ?? "*";
            var wildcard = new Wildcard(pattern, RegexOptions.IgnoreCase);

            foreach (var item in items)
            {
                var key = item.Key;
                {
                    if (key.StartsWith(RegionName))
                    {
                        key = key.Substring(prefixLen);
                        if (pattern == "*" || wildcard.IsMatch(key))
                        {
                            yield return key;
                        }
                    }
                }
            }
        }

        private string BuildKey(string key)
        {
            return RegionName + key.EmptyNull();
        }

        protected override void OnDispose(bool disposing)
        {
            if (disposing)
                Clear();
        }
    }
}
