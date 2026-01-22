using System;

using Microsoft.AspNetCore.Http;

using System.Threading.Tasks;
using Autofac;


namespace SmartStore.Core.Infrastructure.DependencyManagement
{
    /// <summary>
    /// An <see cref="IHttpModule"/> and <see cref="ILifetimeScopeProvider"/> implementation
    /// that creates a nested lifetime scope for each HTTP request.
    /// </summary>
    public class AutofacRequestLifetimeHttpModule     {
RequestDelegate _next = null;
        /* This method is used to register events.
        public void Init(HttpApplication context)
        {
            Guard.NotNull(context, nameof(context));

            context.EndRequest += OnEndRequest;
        }*/
        public static void OnEndRequest(HttpContext context)
        {
            if (LifetimeScopeProvider != null)
            {
                LifetimeScopeProvider.Dispose();
            }

            // Dispose all other disposable object in HttpContext.Items
            PurgeContextItems(context);
        }

private static void PurgeContextItems(HttpContext app)
        {
            var items = app?.Items;

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

        public static void SetLifetimeScopeProvider(ILifetimeScope lifetimeScopeProvider)
        {
            LifetimeScopeProvider = lifetimeScopeProvider ?? throw new ArgumentNullException("lifetimeScopeProvider");
        }


        internal static ILifetimeScope LifetimeScopeProvider
        {
            get;
            private set;
        }

public void Dispose()
        {
        }
    }
}
