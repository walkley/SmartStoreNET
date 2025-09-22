using System.Collections.Generic;
using SmartStore.Web.Framework.Modelling;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc.Rendering;


namespace SmartStore.Admin.Models.Settings
{
    public partial class StoreScopeConfigurationModel : ModelBase
    {
        public StoreScopeConfigurationModel()
        {
            AllStores = new List<SelectListItem>();
        }

        public int StoreId { get; set; }
        public IList<SelectListItem> AllStores { get; set; }
    }
}