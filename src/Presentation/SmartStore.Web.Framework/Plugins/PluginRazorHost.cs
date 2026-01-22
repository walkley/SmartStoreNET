using System.Web.Hosting;
using System.Web.Mvc.Razor;
using System.Web.WebPages.Razor;
using SmartStore.Utilities;
using SmartStore.Web.Framework.Theming;
using Microsoft.AspNetCore.Http;


namespace SmartStore.Web.Framework.Plugins
{
    public class PluginRazorHostFactory : WebRazorHostFactory
    {
        public override WebPageRazorHost CreateHost(string virtualPath, string physicalPath)
        {
            // Implementation borrowed from MvcRazorHostFactory
            var host = base.CreateHost(virtualPath, physicalPath);

            if (!host.IsSpecialPage)
            {
                return new PluginRazorHost(virtualPath, physicalPath);
            }

            return host;
        }
    }

    public class PluginRazorHost : MvcWebPageRazorHost
    {
        public PluginRazorHost(string virtualPath, string physicalPath)
            : base(virtualPath, physicalPath)
        {
            if (CommonHelper.IsDevEnvironment && /* Added by CTA: TODO: Replace HttpContext.Current with dependency injection pattern using IHttpContextAccessor. */
HttpContext.Current.IsDebuggingEnabled)
            {
                var file = HostingEnvironment.VirtualPathProvider.GetFile(virtualPath) as DebugVirtualFile;
                if (file != null)
                {
                    PhysicalPath = file.PhysicalPath;
                }
            }
        }
    }
}
