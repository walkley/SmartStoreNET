using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using SmartStore.AmazonPay.Models;
using SmartStore.AmazonPay.Services;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Customers;
using SmartStore.Services.Authentication.External;
using SmartStore.Services.Customers;
using SmartStore.Services.Payments;
using SmartStore.Services.Tasks;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Settings;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Mvc.ModelBinding;


namespace SmartStore.AmazonPay.Controllers
{
    public class AmazonPayController : PaymentControllerBase
    {
        private readonly HttpContext _httpContext;
        private readonly IAmazonPayService _apiService;
        private readonly Lazy<IScheduleTaskService> _scheduleTaskService;
        private readonly Lazy<IOpenAuthenticationService> _openAuthenticationService;
        private readonly Lazy<ExternalAuthenticationSettings> _externalAuthenticationSettings;

        public AmazonPayController(
            HttpContext httpContext,
            IAmazonPayService apiService,
            Lazy<IScheduleTaskService> scheduleTaskService,
            Lazy<IOpenAuthenticationService> openAuthenticationService,
            Lazy<ExternalAuthenticationSettings> externalAuthenticationSettings)
        {
            _httpContext = httpContext;
            _apiService = apiService;
            _scheduleTaskService = scheduleTaskService;
            _openAuthenticationService = openAuthenticationService;
            _externalAuthenticationSettings = externalAuthenticationSettings;
        }

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            return paymentInfo;
        }

        [AdminAuthorize, LoadSetting]
        public ActionResult Configure(AmazonPaySettings settings)
        {
            var model = new ConfigurationModel();

            MiniMapper.Map(settings, model);
            _apiService.SetupConfiguration(model);

            return View(model);
        }

        [HttpPost, AdminAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult Configure(ConfigurationModel model, FormCollection form)
        {
            var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
            var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
            var settings = Services.Settings.LoadSetting<AmazonPaySettings>(storeScope);

            if (!ModelState.IsValid)
                return Configure(settings);

            ModelState.Clear();

            model.AccessKey = model.AccessKey.TrimSafe();
            model.ClientId = model.ClientId.TrimSafe();
            model.SecretKey = model.SecretKey.TrimSafe();
            model.SellerId = model.SellerId.TrimSafe();

            MiniMapper.Map(model, settings);

            using (Services.Settings.BeginScope())
            {
                storeDependingSettingHelper.UpdateSettings(settings, form, storeScope, Services.Settings);
            }

            using (Services.Settings.BeginScope())
            {
                Services.Settings.SaveSetting(settings, x => x.DataFetching, 0, false);
                Services.Settings.SaveSetting(settings, x => x.PollingMaxOrderCreationDays, 0, false);
            }

            var task = _scheduleTaskService.Value.GetTaskByType<DataPollingTask>();
            if (task != null)
            {
                task.Enabled = settings.DataFetching == AmazonPayDataFetchingType.Polling;

                _scheduleTaskService.Value.UpdateTask(task);
            }

            NotifySuccess(T("Plugins.Payments.AmazonPay.ConfigSaveNote"));

            return RedirectToConfiguration(AmazonPayPlugin.SystemName);
        }

        [HttpPost, AdminAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult SaveAccessData(string accessData)
        {
            try
            {
                var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
                _apiService.ShareKeys(accessData, storeScope);

                NotifySuccess(T("Plugins.Payments.AmazonPay.SaveAccessDataSucceeded"));
            }
            catch (Exception exception)
            {
                NotifyError(exception.Message);
            }

            return RedirectToConfiguration(AmazonPayPlugin.SystemName);
        }

        [ValidateInput(false)]
        public ActionResult ShareKey(string payload)
        {
            Response.AddHeader("Access-Control-Allow-Origin", "https://payments.amazon.com");
            Response.AddHeader("Access-Control-Allow-Methods", "GET, POST");
            Response.AddHeader("Access-Control-Allow-Headers", "Content-Type");

            try
            {
                _apiService.ShareKeys(payload, 0);
            }
            catch (Exception exception)
            {
                Response.StatusCode = 400;
                return Json(new { result = "error", message = exception.Message });
            }

            return Json(new { result = "success" });
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult IPNHandler()
        {
            _apiService.ProcessIpn(Request);
            return Content("OK");
        }

        // For payment simulation in sandbox mode. Admin and sandbox only.
        public ActionResult SetSellerNote(string paymentState)
        {
            var customer = Services.WorkContext.CurrentCustomer;
            if (!customer.IsAdmin())
            {
                return HttpNotFound();
            }

            var store = Services.StoreContext.CurrentStore;
            var settings = Services.Settings.LoadSetting<AmazonPaySettings>(store.Id);

            if (!settings.UseSandbox)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Endpoint is for testing purposes and only available in sandbox mode.");
            }

            var state = _httpContext.GetAmazonPayState(Services.Localization);
            var sellerNote = string.Empty;  // Reset payment state.

            // See https://pay.amazon.com/de/developer/documentation/lpwa/201956480
            if (paymentState.IsCaseInsensitiveEqual("PaymentMethodNotAllowed"))
            {
                sellerNote = "{\"SandboxSimulation\":{\"Constraint\":\"PaymentMethodNotAllowed\"}}";
            }
            else if (paymentState.IsCaseInsensitiveEqual("Success"))
            {
                sellerNote = "{\"SandboxSimulation\":{\"PaymentAuthenticationStatus\":{\"State\":\"Success\"}}}";
            }
            else if (paymentState.IsCaseInsensitiveEqual("Failure"))
            {
                sellerNote = "{\"SandboxSimulation\":{\"PaymentAuthenticationStatus\":{\"State\":\"Failure\"}}}";
            }
            else if (paymentState.IsCaseInsensitiveEqual("Abandoned"))
            {
                sellerNote = "{\"SandboxSimulation\":{\"PaymentAuthenticationStatus\":{\"State\":\"Abandoned\"}}}";
            }

            var result = _apiService.SetOrderReferenceDetails(null, settings, state, sellerNote, out var errorMesage);

            var sb = new StringBuilder();
            sb.Append(result ? "ok. " : "failure. ");
            sb.AppendLine(sellerNote);
            sb.AppendLine(errorMesage.EmptyNull());

            return Content(sb.ToString());
        }

        #region Authentication

        /* Added by CTA: This attribute is not available anymore. An alternative is using ViewComponents:
Sample:

public class SampleViewComponent : ViewComponent
    {
        private readonly InjectedService _injectedService;

        public SampleViewComponent (InjectedService injectedService)
        {
            _injectedService = injectedService;
        }


       public IViewComponentResult Invoke(int parameter)
        {
            var object = _injectedService.SampleFunction(parameter);
        // No name is specified, returns the view SampleView (same name as component)
            return View(object);
        }
    }

Then use this to call the view component from any view:

    @await Component.InvokeAsync("SampleView", new { parameter = ""})

https://docs.microsoft.com/en-us/aspnet/core/mvc/views/view-components?view=aspnetcore-3.1 */
[ChildActionOnly]
        public ActionResult AuthenticationPublicInfo()
        {
            var model = _apiService.CreateViewModel(AmazonPayRequestType.AuthenticationPublicInfo, TempData);
            if (model != null)
            {
                return View(model);
            }

            return new EmptyResult();
        }

        public ActionResult AuthenticationButtonHandler()
        {
            var returnUrl = Session["AmazonAuthReturnUrl"] as string;

            var processor = _openAuthenticationService.Value.LoadExternalAuthenticationMethodBySystemName(AmazonPayPlugin.SystemName, Services.StoreContext.CurrentStore.Id);
            if (processor == null || !processor.IsMethodActive(_externalAuthenticationSettings.Value))
            {
                NotifyError(T("Plugins.Payments.AmazonPay.AuthenticationNotActive"));
                return new RedirectResult(Url.LogOn(returnUrl));
            }

            var result = _apiService.Authorize(returnUrl);
            switch (result.AuthenticationStatus)
            {
                case OpenAuthenticationStatus.Error:
                    result.Errors.Each(x => NotifyError(x));
                    return new RedirectResult(Url.LogOn(returnUrl));
                case OpenAuthenticationStatus.AssociateOnLogon:
                    return new RedirectResult(Url.LogOn(returnUrl));
                case OpenAuthenticationStatus.AutoRegisteredEmailValidation:
                    return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.EmailValidation, returnUrl });
                case OpenAuthenticationStatus.AutoRegisteredAdminApproval:
                    return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.AdminApproval, returnUrl });
                case OpenAuthenticationStatus.AutoRegisteredStandard:
                    return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Standard, returnUrl });
                default:
                    if (result.Result != null)
                        return result.Result;

                    if (HttpContext.Request.IsAuthenticated)
                        return RedirectToReferrer(returnUrl, "~/");

                    return new RedirectResult(Url.LogOn(returnUrl));
            }
        }

        #endregion
    }
}
