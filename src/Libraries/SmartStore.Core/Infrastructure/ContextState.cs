using System;
using System.Threading;
using Microsoft.AspNetCore.Http;


namespace SmartStore.Core.Infrastructure
{
    /// <summary>
    /// Holds some state for the current HttpContext or thread
    /// </summary>
    /// <typeparam name="T">The type of data to store</typeparam>
    public class ContextState<T> where T : class
    {
        private readonly string _name;
        private readonly Func<T> _defaultValue;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AsyncLocal<T> _asyncLocalData = new AsyncLocal<T>();

        public ContextState(IHttpContextAccessor httpContextAccessor, string name)
        {
            _httpContextAccessor = httpContextAccessor;
            _name = name;
        }

        public ContextState(IHttpContextAccessor httpContextAccessor, string name, Func<T> defaultValue)
        {
            _httpContextAccessor = httpContextAccessor;
            _name = name;
            _defaultValue = defaultValue;
        }

        public T GetState()
        {
            var key = BuildKey();

            if (_httpContextAccessor.HttpContext == null)
            {
                var data = _asyncLocalData.Value;

                if (data == null)
                {
                    if (_defaultValue != null)
                    {
                        _asyncLocalData.Value = data = _defaultValue();
                        return data;
                    }
                }

                return data;
            }

            if (_httpContextAccessor.HttpContext.Items[key] == null)
            {
/* Added by CTA: TODO: Replace HttpContext.Current with dependency injection pattern using IHttpContextAccessor. */
_httpContextAccessor.HttpContext.Items[key] = _defaultValue?.Invoke();
            }

return /* Added by CTA: TODO: Replace HttpContext.Current with dependency injection pattern using IHttpContextAccessor. */
_httpContextAccessor.HttpContext.Items[key] as T;
        }

        public void SetState(T state)
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                _asyncLocalData.Value = state;
            }
            else
            {
/* Added by CTA: TODO: Replace HttpContext.Current with dependency injection pattern using IHttpContextAccessor. */
_httpContextAccessor.HttpContext.Items[BuildKey()] = state;
            }
        }

        public void RemoveState()
        {
            var key = BuildKey();

            if (_httpContextAccessor.HttpContext == null)
            {
                _asyncLocalData.Value = null;
            }
            else
            {
                if (_httpContextAccessor.HttpContext.Items.ContainsKey(key))
                {
                    _httpContextAccessor.HttpContext.Items.Remove(key);
                }
            }
        }

        private string BuildKey()
        {
            return "__ContextState." + _name;
        }
    }
}
