using SmartStore.Core;
using SmartStore.Core.Themes;
using SmartStore.Services.Themes;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using Microsoft.AspNetCore.Mvc;


namespace SmartStore.Web.Controllers
{
    [AdminAuthorize]
    public partial class ThemeController : PublicControllerBase
    {
        #region Fields

        private readonly IThemeRegistry _themeRegistry;
        private readonly IThemeVariablesService _themeVarService;
        private readonly IThemeContext _themeContext;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Constructors

        public ThemeController(
            IThemeRegistry themeRegistry,
            IThemeVariablesService themeVarService,
            IThemeContext themeContext,
            IStoreContext storeContext)
        {
            this._themeRegistry = themeRegistry;
            this._themeVarService = themeVarService;
            this._themeContext = themeContext;
            this._storeContext = storeContext;
        }

        #endregion

        #region Methods

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
        public ActionResult ConfigureTheme(string theme, int storeId)
        {
            if (theme.HasValue())
            {
                _themeContext.SetRequestTheme(theme);
            }

            if (storeId > 0)
            {
                _storeContext.SetRequestStore(storeId);
            }

            var model = TempData["OverriddenThemeVars"] ?? _themeVarService.GetThemeVariables(theme, storeId);

            return View(model);
        }

        #endregion
    }
}
