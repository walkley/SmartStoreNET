using System;
using SmartStore.Core.Logging;
using SmartStore.Core.Packaging;
using SmartStore.Core.Security;
using SmartStore.Core.Themes;
using SmartStore.Utilities;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using Microsoft.AspNetCore.Mvc;


namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class PackagingController : AdminControllerBase
    {
        private readonly IPackageManager _packageManager;
        private readonly Lazy<IThemeRegistry> _themeRegistry;

        public PackagingController(
            IPackageManager packageManager,
            Lazy<IThemeRegistry> themeRegistry)
        {
            _packageManager = packageManager;
            _themeRegistry = themeRegistry;
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
        public ActionResult UploadPackage(bool isTheme)
        {
            var title = isTheme ? T("Admin.Packaging.UploadTheme").Text : T("Admin.Packaging.UploadPlugin").Text;
            var info = isTheme ? T("Admin.Packaging.Dialog.ThemeInfo").Text : T("Admin.Packaging.Dialog.PluginInfo").Text;

            var model = new { Title = title, Info = info };
            return PartialView(CommonHelper.ToExpando(model));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadPackage(string returnUrl = "")
        {
            var isTheme = false;
            var success = false;
            string message = null;
            string tempFile = "";

            try
            {
                var file = Request.ToPostedFileResult();
                if (file != null)
                {
                    var requiredPermission = (isTheme = PackagingUtils.IsTheme(file.FileName))
                        ? Permissions.Configuration.Theme.Upload
                        : Permissions.Configuration.Plugin.Upload;

                    if (!Services.Permissions.Authorize(requiredPermission))
                    {
                        message = T("Admin.AccessDenied.Description");
                        return Json(new { success, file.FileName, message });
                    }

                    if (!file.FileExtension.IsCaseInsensitiveEqual(".nupkg"))
                    {
                        return Json(new { success, file.FileName, T("Admin.Packaging.NotAPackage").Text, returnUrl });
                    }

                    var location = CommonHelper.MapPath("~/App_Data");
                    var appPath = CommonHelper.MapPath("~/");

                    if (isTheme)
                    {
                        // Avoid getting terrorized by IO events.
                        _themeRegistry.Value.StopMonitoring();
                    }

                    var packageInfo = _packageManager.Install(file.Stream, location, appPath);

                    if (isTheme)
                    {
                        // Create manifest.
                        if (packageInfo != null)
                        {
                            var manifest = ThemeManifest.Create(packageInfo.ExtensionDescriptor.Path);
                            if (manifest != null)
                            {
                                _themeRegistry.Value.AddThemeManifest(manifest);
                            }
                        }

                        // SOFT start IO events again.
                        _themeRegistry.Value.StartMonitoring(false);
                    }
                }
                else
                {
                    return Json(new { success, file.FileName, T("Admin.Common.UploadFile").Text, returnUrl });
                }

                if (!isTheme)
                {
                    message = T("Admin.Packaging.InstallSuccess").Text;
                    Services.WebHelper.RestartAppDomain();
                }
                else
                {
                    message = T("Admin.Packaging.InstallSuccess.Theme").Text;
                }

                success = true;

            }
            catch (Exception ex)
            {
                message = ex.Message;
                Logger.Error(ex);
            }

            return Json(new { success, tempFile, message, returnUrl });
        }
    }
}