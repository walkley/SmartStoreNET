using System.Collections.Generic;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc.Rendering;


namespace SmartStore.Admin.Models.DataExchange
{
    public partial class ImportProfileListModel : EntityModelBase
    {
        [SmartResourceDisplayName("Admin.Common.Entity")]
        public ImportEntityType EntityType { get; set; }
        public List<SelectListItem> AvailableEntityTypes { get; set; }

        public List<ImportProfileModel> Profiles { get; set; }
    }
}