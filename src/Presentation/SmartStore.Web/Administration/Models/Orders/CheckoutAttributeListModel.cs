using System.Collections.Generic;
using SmartStore.Web.Framework.Modelling;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc.Rendering;


namespace SmartStore.Admin.Models.Orders
{
    public class CheckoutAttributeListModel : ModelBase
    {
        public bool IsSingleStoreMode { get; set; }
        public int GridPageSize { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }
    }
}