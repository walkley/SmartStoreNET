using System.Collections.Generic;
using SmartStore.PayPal.Services;
using SmartStore.PayPal.Settings;
using SmartStore.Web.Framework;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc.Rendering;


namespace SmartStore.PayPal.Models
{
    public class PayPalInstalmentsConfigModel : ApiConfigurationModel
    {
        public PayPalInstalmentsConfigModel()
        {
            TransactMode = TransactMode.AuthorizeAndCapture;
        }

        [SmartResourceDisplayName("Plugins.Payments.PayPalInstalments.FinancingMin")]
        public decimal FinancingMin { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.PayPalInstalments.FinancingMax")]
        public decimal FinancingMax { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.PayPalInstalments.ProductPagePromotion")]
        public PayPalPromotion? ProductPagePromotion { get; set; }
        public IList<SelectListItem> ProductPagePromotions { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.PayPalInstalments.CartPagePromotion")]
        public PayPalPromotion? CartPagePromotion { get; set; }
        public IList<SelectListItem> CartPagePromotions { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.PayPalInstalments.PaymentListPromotion")]
        public PayPalPromotion? PaymentListPromotion { get; set; }
        public IList<SelectListItem> PaymentListPromotions { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.PayPalInstalments.Lender")]
        public string Lender { get; set; }

        //[SmartResourceDisplayName("Plugins.Payments.PayPalInstalments.Promote")]
        //public bool Promote { get; set; }

        //[SmartResourceDisplayName("Plugins.Payments.PayPalInstalments.PromotionWidgetZones")]
        //[UIHint("WidgetZone")]
        //public string[] PromotionWidgetZones { get; set; }

        //[SmartResourceDisplayName("Plugins.Payments.PayPalInstalments.PromotionDisplayOrder")]
        //public int PromotionDisplayOrder { get; set; }
    }
}