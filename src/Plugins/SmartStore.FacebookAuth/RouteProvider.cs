using SmartStore.Web.Framework.Routing;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Routing;


namespace SmartStore.FacebookAuth
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("SmartStore.FacebookAuth",
                 "Plugins/SmartStore.FacebookAuth/{action}",
                 new { controller = "ExternalAuthFacebook" },
                 new[] { "SmartStore.FacebookAuth.Controllers" }
            )
            .DataTokens["area"] = FacebookExternalAuthMethod.SystemName;
        }
        public int Priority => 0;
    }
}
