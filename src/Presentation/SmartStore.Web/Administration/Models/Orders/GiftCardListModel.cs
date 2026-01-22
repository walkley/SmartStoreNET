using System.Collections.Generic;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc.Rendering;


namespace SmartStore.Admin.Models.Orders
{
    public class GiftCardListModel : ModelBase
    {
        public GiftCardListModel()
        {
            ActivatedList = new List<SelectListItem>();
        }

        public int GridPageSize { get; set; }

        [SmartResourceDisplayName("Admin.GiftCards.List.CouponCode")]
        [AllowHtml]
        public string CouponCode { get; set; }

        [SmartResourceDisplayName("Admin.GiftCards.List.Activated")]
        public int ActivatedId { get; set; }

        [SmartResourceDisplayName("Admin.GiftCards.List.Activated")]
        public IList<SelectListItem> ActivatedList { get; set; }
    }
}