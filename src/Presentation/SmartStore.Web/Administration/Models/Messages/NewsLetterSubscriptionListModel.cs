using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using Telerik.Web.Mvc;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc.Rendering;


namespace SmartStore.Admin.Models.Messages
{
    public class NewsLetterSubscriptionListModel : ModelBase
    {
        public NewsLetterSubscriptionListModel()
        {
            AvailableStores = new List<SelectListItem>();
        }

        public int GridPageSize { get; set; }

        public GridModel<NewsLetterSubscriptionModel> NewsLetterSubscriptions { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.List.SearchEmail")]
        public string SearchEmail { get; set; }

        [UIHint("CustomerRoles")]
        [AdditionalMetadata("multiple", true)]
        [SmartResourceDisplayName("Admin.Customers.Customers.List.CustomerRoles")]
        public int[] SearchCustomerRoleIds { get; set; }

        [SmartResourceDisplayName("Admin.Common.Store.SearchFor")]
        public int StoreId { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }
    }
}