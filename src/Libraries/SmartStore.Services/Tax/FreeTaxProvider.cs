using SmartStore;
using SmartStore.Core.Plugins;
using Microsoft.AspNetCore.Routing;


namespace SmartStore.Services.Tax
{
    [SystemName("Tax.Free")]
    [FriendlyName("Free tax rate provider")]
    [DisplayOrder(0)]
    public class FreeTaxProvider : ITaxProvider
    {

        public CalculateTaxResult GetTaxRate(CalculateTaxRequest calculateTaxRequest)
        {
            var result = new CalculateTaxResult()
            {
                TaxRate = decimal.Zero
            };
            return result;
        }

    }
}
