using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;


// use base SmartStore Namespace to ensure the extension methods are always available
namespace SmartStore
{
    public static class RouteExtensions
    {
        public static string GetAreaName(this RouteData routeData)
        {
            if (routeData.DataTokens.TryGetValue("area", out object area))
            {
                return (area as string);
            }

            return routeData.Route.GetAreaName();
        }

        public static string GetAreaName(this Endpoint endpoint)
        {
            if (endpoint is RouteEndpoint routeEndpoint)
            {
                if (routeEndpoint.RoutePattern?.Defaults != null &&
                    routeEndpoint.RoutePattern.Defaults.TryGetValue("area", out var areaValue))
                {
                    return areaValue as string;
                }

                if (routeEndpoint.RoutePattern?.RequiredValues != null &&
                    routeEndpoint.RoutePattern.RequiredValues.TryGetValue("area", out var requiredArea))
                {
                    return requiredArea as string;
                }
            }

            return null;
        }

        /// <summary>
        /// Generates an identifier for the given route in the form "[{area}.]{controller}.{action}"
        /// </summary>
        public static string GenerateRouteIdentifier(this RouteData routeData)
        {
            string area = routeData.GetAreaName();
            string controller = routeData.Values["controller"]?.ToString();
            string action = routeData.Values["action"]?.ToString();

            return "{0}{1}.{2}".FormatInvariant(area.HasValue() ? area + "." : "", controller, action);
        }

        public static bool IsRouteEqual(this RouteData routeData, string controller, string action)
        {
            if (routeData == null)
                return false;

            return routeData.Values["controller"]?.ToString().IsCaseInsensitiveEqual(controller) == true && routeData.Values["action"]?.ToString().IsCaseInsensitiveEqual(action) == true;
        }

    }
}
