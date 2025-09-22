using System.Collections.Generic;
using SmartStore.Web.Framework.Modelling;
using Microsoft.AspNetCore.Routing;


namespace SmartStore.Web.Models.ShoppingCart
{
    public partial class ButtonPaymentMethodModel : ModelBase
    {
        public ButtonPaymentMethodModel()
        {
            Items = new List<ButtonPaymentMethodItem>();
        }

        public IList<ButtonPaymentMethodItem> Items { get; set; }

        public partial class ButtonPaymentMethodItem
        {
            public string ActionName { get; set; }
            public string ControllerName { get; set; }
            public RouteValueDictionary RouteValues { get; set; }
        }
    }
}