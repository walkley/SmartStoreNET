using SmartStore.Web.Framework.Routing;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Routing;


namespace SmartStore.Clickatell
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("SmartStore.Clickatell",
                 "Plugins/SmartStore.Clickatell/{action}",
                 new { controller = "SmsClickatell", action = "Configure" },
                 new[] { "SmartStore.Clickatell.Controllers" }
            )
            .DataTokens["area"] = ClickatellSmsProvider.SystemName;
        }

        public int Priority => 0;
    }
}
