using SmartStore.Web.Framework.Modelling;
using Microsoft.AspNetCore.Routing;


namespace SmartStore.Web.Models.Customer
{
    public partial class ExternalAuthenticationMethodModel : ModelBase
    {
        public string ActionName { get; set; }
        public string ControllerName { get; set; }
        public RouteValueDictionary RouteValues { get; set; }
    }
}