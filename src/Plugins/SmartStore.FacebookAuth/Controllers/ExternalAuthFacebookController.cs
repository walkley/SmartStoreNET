using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Security;
using SmartStore.FacebookAuth.Core;
using SmartStore.FacebookAuth.Models;
using SmartStore.Services.Authentication.External;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Settings;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc.ModelBinding;


namespace SmartStore.FacebookAuth.Controllers
{
    public class ExternalAuthFacebookController : PluginControllerBase
    {
        private readonly IOAuthProviderFacebookAuthorizer _oAuthProviderFacebookAuthorizer;
        private readonly IOpenAuthenticationService _openAuthenticationService;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;

        public ExternalAuthFacebookController(
            IOAuthProviderFacebookAuthorizer oAuthProviderFacebookAuthorizer,
            IOpenAuthenticationService openAuthenticationService,
            ExternalAuthenticationSettings externalAuthenticationSettings)
        {
            _oAuthProviderFacebookAuthorizer = oAuthProviderFacebookAuthorizer;
            _openAuthenticationService = openAuthenticationService;
            _externalAuthenticationSettings = externalAuthenticationSettings;
        }

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
[LoadSetting, AdminAuthorize, ChildActionOnly]
        [Permission(Permissions.Configuration.Authentication.Read)]
        public ActionResult Configure(FacebookExternalAuthSettings settings)
        {
            var model = new ConfigurationModel();
            MiniMapper.Map(settings, model);

            var host = Services.StoreContext.CurrentStore.GetHost(true);
            model.RedirectUrl = $"{host}Plugins/SmartStore.FacebookAuth/logincallback/";

            return View(model);
        }

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
[SaveSetting, HttpPost, AdminAuthorize, ChildActionOnly]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Authentication.Update)]
        public ActionResult Configure(FacebookExternalAuthSettings settings, ConfigurationModel model)
        {
            if (!ModelState.IsValid)
            {
                return Configure(settings);
            }

            MiniMapper.Map(model, settings);
            settings.ClientKeyIdentifier = model.ClientKeyIdentifier.TrimSafe();
            settings.ClientSecret = model.ClientSecret.TrimSafe();

            NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));

            return RedirectToConfiguration(FacebookExternalAuthMethod.SystemName, true);
        }

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
        public ActionResult PublicInfo()
        {
            var settings = Services.Settings.LoadSetting<FacebookExternalAuthSettings>(Services.StoreContext.CurrentStore.Id);

            if (settings.ClientKeyIdentifier.HasValue() && settings.ClientSecret.HasValue())
            {
                return View();
            }

            return new EmptyResult();
        }

        public ActionResult Login(string returnUrl)
        {
            return LoginInternal(returnUrl, false);
        }

        public ActionResult LoginCallback(string returnUrl)
        {
            return LoginInternal(returnUrl, true);
        }

        [NonAction]
        private ActionResult LoginInternal(string returnUrl, bool verifyResponse)
        {
            var processor = _openAuthenticationService.LoadExternalAuthenticationMethodBySystemName(FacebookExternalAuthMethod.SystemName, Services.StoreContext.CurrentStore.Id);
            if (processor == null || !processor.IsMethodActive(_externalAuthenticationSettings))
            {
                NotifyError(T("Plugins.CannotLoadModule", T("Plugins.FriendlyName.SmartStore.FacebookAuth")));
                return new RedirectResult(Url.LogOn(returnUrl));
            }

            var viewModel = new LoginModel();
            TryUpdateModel(viewModel);

            var result = _oAuthProviderFacebookAuthorizer.Authorize(returnUrl, verifyResponse);
            switch (result.AuthenticationStatus)
            {
                case OpenAuthenticationStatus.Error:
                    {
                        if (!result.Success)
                        {
                            result.Errors.Each(x => NotifyError(x));
                        }

                        return new RedirectResult(Url.LogOn(returnUrl));
                    }
                case OpenAuthenticationStatus.AssociateOnLogon:
                    {
                        return new RedirectResult(Url.LogOn(returnUrl));
                    }
                case OpenAuthenticationStatus.AutoRegisteredEmailValidation:
                    {
                        return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.EmailValidation, returnUrl });
                    }
                case OpenAuthenticationStatus.AutoRegisteredAdminApproval:
                    {
                        return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.AdminApproval, returnUrl });
                    }
                case OpenAuthenticationStatus.AutoRegisteredStandard:
                    {
                        return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Standard, returnUrl });
                    }
                default:
                    break;
            }

            if (result.Result != null)
            {
                return result.Result;
            }

            return HttpContext.Request.IsAuthenticated ?
                RedirectToReferrer(returnUrl, "~/") :
                new RedirectResult(Url.LogOn(returnUrl));
        }
    }
}