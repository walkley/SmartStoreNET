using System;
using Microsoft.AspNetCore.Routing;

using Microsoft.AspNetCore.Http;


namespace SmartStore.Web.Framework.Routing
{
    public class GuidConstraint : IRouteConstraint
    {
        private readonly bool _allowEmpty;

        public GuidConstraint(bool allowEmpty)
        {
            this._allowEmpty = allowEmpty;
        }
        public bool Match(HttpContext httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (values.ContainsKey(parameterName))
            {
                string stringValue = values[parameterName] != null ? values[parameterName].ToString() : null;

                if (!string.IsNullOrEmpty(stringValue))
                {
                    return Guid.TryParse(stringValue, out var guidValue) &&
                        (_allowEmpty || guidValue != Guid.Empty);
                }
            }

            return false;
        }
    }
}
