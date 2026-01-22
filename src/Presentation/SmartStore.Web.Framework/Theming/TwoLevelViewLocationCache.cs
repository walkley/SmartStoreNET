using System;
using System.Collections.Generic;
using SmartStore.Utilities;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Http;


namespace SmartStore.Web.Framework.Theming
{
    public class TwoLevelViewLocationCache : IViewLocationCache
    {
        private static readonly object s_key = new object();
        private readonly IViewLocationCache _innerCache;

        public TwoLevelViewLocationCache()
        {
            var cacheDuringDebug = CommonHelper.GetAppSetting<bool>("sm:EnableViewLocationCacheDuringDebug");

            if (!cacheDuringDebug && (/* Added by CTA: TODO: Replace HttpContext.Current with dependency injection pattern using IHttpContextAccessor. */
HttpContext.Current == null || /* Added by CTA: TODO: Replace HttpContext.Current with dependency injection pattern using IHttpContextAccessor. */
HttpContext.Current.IsDebuggingEnabled))
            {
                _innerCache = DefaultViewLocationCache.Null;
            }
            else
            {
                _innerCache = new DefaultViewLocationCache(TimeSpan.FromHours(2));
            }

        }

        private static IDictionary<string, string> GetRequestCache(HttpContext httpContext)
        {
            var d = httpContext.Items[s_key] as IDictionary<string, string>;
            if (d == null)
            {
                d = new Dictionary<string, string>();
                httpContext.Items[s_key] = d;
            }
            return d;
        }

        public string GetViewLocation(HttpContext httpContext, string key)
        {
            var d = GetRequestCache(httpContext);
            string location;
            if (!d.TryGetValue(key, out location))
            {
                location = _innerCache.GetViewLocation(httpContext, key);
                d[key] = location;
            }
            return location;
        }

        public void InsertViewLocation(HttpContext httpContext, string key, string virtualPath)
        {
            _innerCache.InsertViewLocation(httpContext, key, virtualPath);
            GetRequestCache(httpContext)[key] = virtualPath;
        }
    }
}
