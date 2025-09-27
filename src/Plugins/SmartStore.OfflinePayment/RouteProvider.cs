using SmartStore.Web.Framework.Routing;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Routing;


namespace SmartStore.OfflinePayment
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("SmartStore.OfflinePayment",
                 "Plugins/SmartStore.OfflinePayment/{action}",
                 new { controller = "OfflinePayment", action = "Index" },
                 new[] { "SmartStore.OfflinePayment.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.OfflinePayment";
        }
        public int Priority => 0;
    }
}
