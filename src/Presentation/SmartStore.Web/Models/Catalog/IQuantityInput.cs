using System.Collections.Generic;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Services.Localization;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc.Rendering;


namespace SmartStore.Web.Models.Catalog
{
    public interface IQuantityInput
    {
        int EnteredQuantity { get; }
        int MinOrderAmount { get; }
        int MaxOrderAmount { get; }
        int QuantityStep { get; }
        LocalizedValue<string> QuantityUnitName { get; }
        List<SelectListItem> AllowedQuantities { get; }
        QuantityControlType QuantiyControlType { get; }
    }
}