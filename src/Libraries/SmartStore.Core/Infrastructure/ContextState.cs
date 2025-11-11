using System;
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

        public ContextState(string name)
        {
            _name = name;
        }

        public ContextState(string name, Func<T> defaultValue)
        {
            _name = name;
            _defaultValue = defaultValue;
        }

        public T GetState()
        {
            var key = BuildKey();

            if (/* Added by CTA: TODO: Replace HttpContext.Current with dependency injection pattern using IHttpContextAccessor. */
HttpContext.Current == null)
            {
                var data = CallContext.GetData(key);

                if (data == null)
                {
                    if (_defaultValue != null)
                    {
                        CallContext.SetData(key, data = _defaultValue());
                        return data as T;
                    }
                }

                return data as T;
            }

            if (/* Added by CTA: TODO: Replace HttpContext.Current with dependency injection pattern using IHttpContextAccessor. */
HttpContext.Current.Items[key] == null)
            {
                /* Added by CTA: TODO: Replace HttpContext.Current with dependency injection pattern using IHttpContextAccessor. */
HttpContext.Current.Items[key] = _defaultValue?.Invoke();
            }

            return /* Added by CTA: TODO: Replace HttpContext.Current with dependency injection pattern using IHttpContextAccessor. */
HttpContext.Current.Items[key] as T;
        }

        public void SetState(T state)
        {
            if (/* Added by CTA: TODO: Replace HttpContext.Current with dependency injection pattern using IHttpContextAccessor. */
HttpContext.Current == null)
            {
                CallContext.SetData(BuildKey(), state);
            }
            else
            {
                /* Added by CTA: TODO: Replace HttpContext.Current with dependency injection pattern using IHttpContextAccessor. */
HttpContext.Current.Items[BuildKey()] = state;
            }
        }

        public void RemoveState()
        {
            var key = BuildKey();

            if (/* Added by CTA: TODO: Replace HttpContext.Current with dependency injection pattern using IHttpContextAccessor. */
HttpContext.Current == null)
            {
                CallContext.FreeNamedDataSlot(key);
            }
            else
            {
                if (/* Added by CTA: TODO: Replace HttpContext.Current with dependency injection pattern using IHttpContextAccessor. */
HttpContext.Current.Items.Contains(key))
                {
                    /* Added by CTA: TODO: Replace HttpContext.Current with dependency injection pattern using IHttpContextAccessor. */
HttpContext.Current.Items.Remove(key);
                }
            }
        }

        private string BuildKey()
        {
            return "__ContextState." + _name;
        }
    }
}
