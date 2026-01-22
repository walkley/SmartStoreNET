using SmartStore.Web.Framework.Routing;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Routing;


namespace SmartStore.GoogleAnalytics
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("SmartStore.GoogleAnalytics",
                 "Plugins/SmartStore.GoogleAnalytics/{action}",
                 new { controller = "WidgetsGoogleAnalytics", action = "Configure" },
                 new[] { "SmartStore.GoogleAnalytics.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.GoogleAnalytics";
        }

        public int Priority => 0;
    }
}
