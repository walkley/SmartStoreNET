using SmartStore.ComponentModel;
using SmartStore.Core.Search.Facets;
using SmartStore.Core.Security;
using SmartStore.DevTools.Blocks;
using SmartStore.DevTools.Models;
using SmartStore.DevTools.Security;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Settings;
using SmartStore.Web.Framework.Theming;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc.ModelBinding;

using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Authorization;


namespace SmartStore.DevTools.Controllers
{
    public class DevToolsController : SmartController
    {
        [AdminAuthorize, Permission(DevToolsPermissions.Read)]
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
[ChildActionOnly, LoadSetting]
        public ActionResult Configure(ProfilerSettings settings)
        {
            var model = MiniMapper.Map<ProfilerSettings, ConfigurationModel>(settings);

            return View(model);
        }

        [AdminAuthorize, Permission(DevToolsPermissions.Update)]
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
[HttpPost, ChildActionOnly, SaveSetting]
        [ValidateAntiForgeryToken]
        public ActionResult Configure(ConfigurationModel model, ProfilerSettings settings)
        {
            if (!ModelState.IsValid)
            {
                return Configure(settings);
            }

            ModelState.Clear();
            MiniMapper.Map(model, settings);

            return RedirectToConfiguration("SmartStore.DevTools");
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
        public ActionResult MiniProfiler()
        {
            return View();
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
        public ActionResult MachineName()
        {
            ViewBag.EnvironmentIdentifier = Services.ApplicationEnvironment.EnvironmentIdentifier;

            return View();
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
        public ActionResult WidgetZone(string widgetZone)
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
            var settings = Services.Settings.LoadSetting<ProfilerSettings>(storeScope);

            if (settings.DisplayWidgetZones)
            {
                ViewData["widgetZone"] = widgetZone;

                return View();
            }

            return new EmptyResult();
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
        public ActionResult SampleBlock(SampleBlock block)
        {
            // Do something here with your block instance and return a result that should be rendered by the Page Builder.
            return View(block);
        }

        [AdminAuthorize, AdminThemed]
        public ActionResult BackendExtension()
        {
            var model = new BackendExtensionModel
            {
                Welcome = "Hello world!"
            };

            return View(model);
        }

        [AdminAuthorize]
        public ActionResult ProductEditTab(int productId, FormCollection form)
        {
            var model = new BackendExtensionModel
            {
                Welcome = "Hello world!"
            };

            var result = PartialView(model);
            result.ViewData.TemplateInfo = new TemplateInfo { HtmlFieldPrefix = "CustomProperties[DevTools]" };
            return result;
        }

        public ActionResult MyDemoWidget()
        {
            return Content("Hello world! This is a sample widget created for demonstration purposes by Dev-Tools plugin.");
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
[ChildActionOnly, AllowAnonymous]
        public ActionResult MyCustomFacetTemplate(FacetGroup facetGroup, string templateName)
        {
            /// Just a "proxy" for our <see cref="Services.CustomFacetTemplateSelector" />.
            return PartialView(templateName, facetGroup);
        }
    }
}