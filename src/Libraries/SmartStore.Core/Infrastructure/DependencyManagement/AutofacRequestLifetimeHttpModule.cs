using System;
using Microsoft.AspNetCore.Http;
using Autofac;
using System.Threading.Tasks;


namespace SmartStore.Core.Infrastructure.DependencyManagement
{
    /// <summary>
    /// An <see cref="IHttpModule"/> and <see cref="ILifetimeScopeProvider"/> implementation
    /// that creates a nested lifetime scope for each HTTP request.
    /// </summary>
    public class AutofacRequestLifetimeHttpModule
    {
        RequestDelegate _next = null;

        public async Task InvokeAsync(HttpContext context)
        {
            Guard.NotNull(context, nameof(context));

            try
            {
                await _next(context);
            }
            finally
            {
                OnEndRequest(context);
            }
        }

        private static void OnEndRequest(HttpContext context)
        {
            if (LifetimeScopeProvider != null)
            {
                LifetimeScopeProvider.Dispose();
            }

            // Dispose all other disposable object in HttpContext.Items
            PurgeContextItems(context);
        }

        private static void PurgeContextItems(HttpContext context)
        {
            var items = context?.Items;

            if (items != null)
            {
                int size = items.Count;
                if (size > 0)
                {
                    var keys = new object[size];
                    items.Keys.CopyTo(keys, 0);

                    for (int i = 0; i < size; i++)
                    {
                        var obj = items[keys[i]] as IDisposable;
                        if (obj != null)
                        {
                            try
                            {
                                obj.Dispose();
                            }
                            catch { }
                        }
                    }
                }
            }
        }

        public static void SetLifetimeScopeProvider(ILifetimeScope lifetimeScope)
        {
            LifetimeScopeProvider = lifetimeScope ?? throw new ArgumentNullException("lifetimeScope");
        }


        internal static ILifetimeScope LifetimeScopeProvider
        {
            get;
            private set;
        }

        public void Dispose()
        {
        }

        public AutofacRequestLifetimeHttpModule(RequestDelegate next)
        {
            _next = next;
        }
    }
}
