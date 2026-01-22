using Microsoft.AspNetCore.Mvc;

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

            return null;
        }

        public static string GetAreaName(this RouteBase route)
        {
            // IRouteWithArea is not available in ASP.NET Core, removed the cast and check for it.
            var route2 = route as Route;
            if ((route2 != null) && (route2.DataTokens != null))
            {
                return (route2.DataTokens["area"] as string);
            }

            return null;
        }

        /// <summary>
        /// Generates an identifier for the given route in the form "[{area}.]{controller}.{action}"
        /// </summary>
        public static string GenerateRouteIdentifier(this RouteData routeData)
        {
            string area = routeData.GetAreaName();
            string controller = routeData.Values["controller"] as string;
            string action = routeData.Values["action"] as string;

            return "{0}{1}.{2}".FormatInvariant(area.HasValue() ? area + "." : "", controller, action);
        }

        public static bool IsRouteEqual(this RouteData routeData, string controller, string action)
        {
            if (routeData == null)
                return false;

            return (routeData.Values["controller"] as string).IsCaseInsensitiveEqual(controller) && (routeData.Values["action"] as string).IsCaseInsensitiveEqual(action);
        }

    }
}
