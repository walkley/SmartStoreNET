using SmartStore.Web.Framework.Routing;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Routing;


namespace SmartStore.GoogleMerchantCenter
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("SmartStore.GoogleMerchantCenter",
                 "Plugins/SmartStore.GoogleMerchantCenter/{action}",
                 new { controller = "FeedGoogleMerchantCenter", action = "Configure" },
                 new[] { "SmartStore.GoogleMerchantCenter.Controllers" }
            )
            .DataTokens["area"] = GoogleMerchantCenterFeedPlugin.SystemName;
        }

        public int Priority => 0;
    }
}
