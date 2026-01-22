using System.Collections.Generic;
using SmartStore.Services.Payments;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Http;


namespace SmartStore.Web.Framework.Controllers
{
    public abstract class PaymentControllerBase : SmartController
    {
        public abstract IList<string> ValidatePaymentForm(FormCollection form);
        public abstract ProcessPaymentRequest GetPaymentInfo(FormCollection form);

        public virtual string GetPaymentSummary(FormCollection form)
        {
            return null;
        }
    }
}
