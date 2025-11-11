using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.Caching;
using SmartStore.Core;
using SmartStore.Core.Fakes;
using SmartStore.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;


namespace SmartStore
{
    public static class HttpExtensions
    {
        const string CacheRegionName = "SmartStoreNET:";
        const string RememberPathKey = "AppRelativeCurrentExecutionFilePath.Original";

        private static readonly List<Tuple<string, string>> _sslHeaders = new List<Tuple<string, string>>
        {
            new Tuple<string, string>("HTTP_CLUSTER_HTTPS", "on"),
            new Tuple<string, string>("HTTP_X_FORWARDED_PROTO", "https"),
            new Tuple<string, string>("X-Forwarded-Proto", "https"),
            new Tuple<string, string>("x-arr-ssl", null),
            new Tuple<string, string>("X-Forwarded-Protocol", "https"),
            new Tuple<string, string>("X-Forwarded-Ssl", "on"),
            new Tuple<string, string>("X-Url-Scheme", "https")
        };

        /// <summary>
        /// Tries to get the <see cref="HttpRequest"/> instance without throwing exceptions
        /// </summary>
        /// <returns>The <see cref="HttpRequest"/> instance or <c>null</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HttpRequest SafeGetHttpRequest(this HttpContext httpContext)
        {
            if (httpContext == null)
            {
                return null;
            }

            if (httpContext is FakeHttpContext)
            {
                return httpContext.Request;
            }

            try
            {
                return httpContext.Request;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Returns wether the specified url is local to the host or not
        /// </summary>
        /// <param name="request"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool IsAppLocalUrl(this HttpRequest request, string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            url = url.Trim();

            if (url.StartsWith("~/"))
            {
                return true;
            }

            if (url.StartsWith("//") || url.StartsWith("/\\"))
            {
                return false;
            }

            // At this point when the url starts with "/" it is local
            if (url.StartsWith("/"))
            {
                return true;
            }

            // At this point, check for a fully qualified url
            try
            {
                var uri = new Uri(url);

                if (!uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) && !uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                if (uri.Authority.Equals(request.Headers["Host"], StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                // Finally, check the base url from the settings
                var storeContext = EngineContext.Current.Resolve<IStoreContext>();
                if (storeContext != null)
                {
                    var baseUrl = storeContext.CurrentStore.Url;
                    if (baseUrl.HasValue())
                    {
                        if (uri.Authority.Equals(new Uri(baseUrl).Authority, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            catch
            {
                // mall-formed url e.g, "abcdef"
                return false;
            }
        }

        /// <summary>
        /// Gets a value which indicates whether the HTTP connection uses secure sockets (HTTPS protocol).
        /// Works with Cloud's load balancers.
        /// </summary>
        public static bool IsHttps(this HttpRequest request)
        {
            if (request.IsHttps)
            {
                return true;
            }

            foreach (var tuple in _sslHeaders)
            {
                if (request.Headers.TryGetValue(tuple.Item1, out var headerValue))
                {
                    return tuple.Item2 == null || tuple.Item2.Equals(headerValue.ToString(), StringComparison.OrdinalIgnoreCase);
                }
            }

            return false;
        }

        /// <summary>
        /// Gets a value which indicates whether the current request requests a static resource, like .txt, .pdf, .js, .css etc.
        /// </summary>
        public static bool IsStaticResourceRequested(this HttpContext context)
        {
            if (context?.Request == null)
                return false;

            return context.GetItem<bool>(
                "IsStaticResourceRequested",
                () => WebHelper.IsStaticResourceRequested(context.Request),
                true);
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetFormsAuthenticationCookie(this HttpWebRequest webRequest, HttpRequest httpRequest)
        {
            CopyCookie(webRequest, httpRequest, ".ASPXAUTH");
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetAnonymousIdentCookie(this HttpWebRequest webRequest, HttpRequest httpRequest)
        {
            CopyCookie(webRequest, httpRequest, "SMARTSTORE.ANONYMOUS");
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVisitorCookie(this HttpWebRequest webRequest, HttpRequest httpRequest)
        {
            CopyCookie(webRequest, httpRequest, "SMARTSTORE.VISITOR");
        }

        private static void CopyCookie(HttpWebRequest webRequest, HttpRequest sourceHttpRequest, string cookieName)
        {
            Guard.NotNull(webRequest, nameof(webRequest));
            Guard.NotNull(sourceHttpRequest, nameof(sourceHttpRequest));
            Guard.NotEmpty(cookieName, nameof(cookieName));

            var sourceCookieValue = sourceHttpRequest.Cookies[cookieName];
            if (string.IsNullOrEmpty(sourceCookieValue))
                return;

            var requestHost = sourceHttpRequest.Host.Host;
            var sendCookie = new Cookie(cookieName, sourceCookieValue, "/", requestHost);

            if (webRequest.CookieContainer == null)
            {
                webRequest.CookieContainer = new CookieContainer();
            }

            webRequest.CookieContainer.Add(sendCookie);
        }

        public static string BuildScopedKey(this MemoryCache cache, string key)
        {
            return key.HasValue() ? CacheRegionName + key : null;
        }

        public static T GetOrAdd<T>(this MemoryCache cache, string key, Func<T> acquirer, TimeSpan? duration = null)
        {
            Guard.NotEmpty(key, nameof(key));
            Guard.NotNull(acquirer, nameof(acquirer));

            object obj = cache.Get(key);

            if (obj != null)
            {
                return (T)obj;
            }

            var value = acquirer();

            var absoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration;
            if (duration.HasValue)
            {
                absoluteExpiration = DateTime.UtcNow + duration.Value;
            }

            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = absoluteExpiration
            };

            cache.Set(key, value, policy);

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RememberAppRelativePath(this HttpContext httpContext)
        {
            httpContext.Items[RememberPathKey] = httpContext.Request.Path.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetOriginalAppRelativePath(this HttpContext httpContext)
        {
            return GetItem<string>(httpContext, RememberPathKey, forceCreation: false) ?? httpContext.Request.Path.Value;
        }

        public static T GetItem<T>(this HttpContext httpContext, string key, Func<T> factory = null, bool forceCreation = true)
        {
            Guard.NotEmpty(key, nameof(key));

            var items = httpContext?.Items;
            if (items == null)
            {
                return default(T);
            }

            object keyObj = key;
            if (items.TryGetValue(keyObj, out var value))
            {
                return (T)value;
            }
            else
            {
                if (forceCreation)
                {
                    var item = (factory ?? (() => Activator.CreateInstance<T>())).Invoke();
                    items[keyObj] = item;
                    return item;
                }
                else
                {
                    return default(T);
                }
            }
        }

        public static void RemoveByPattern(this MemoryCache cache, string pattern)
        {
            var keys = cache.AllKeys(pattern);

            foreach (var key in keys.ToArray())
            {
                cache.Remove(key);
            }
        }

        public static string[] AllKeys(this MemoryCache cache, string pattern)
        {
            pattern = pattern == "*" ? CacheRegionName : pattern;

            var keys = cache
                .Where(entry => entry.Key.StartsWith(pattern, StringComparison.OrdinalIgnoreCase))
                .Select(entry => entry.Key)
                .ToArray();

            return keys;
        }

        public static ControllerContext GetRootControllerContext(this ControllerContext controllerContext)
        {
            Guard.NotNull(controllerContext, nameof(controllerContext));

            return controllerContext;
        }

        public static bool IsBareBonePage(this ControllerContext controllerContext)
        {
            var ctx = controllerContext.GetRootControllerContext();

            if (ctx?.HttpContext != null)
            {
                var controller = ctx.ActionDescriptor?.Properties.Values.OfType<Controller>().FirstOrDefault();
                if (controller != null && controller.ViewData != null)
                {
                    var viewBag = controller.ViewBag;
                    // IsPopUp or Framed
                    if (viewBag.IsPopup == true || viewBag.Framed == true)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
