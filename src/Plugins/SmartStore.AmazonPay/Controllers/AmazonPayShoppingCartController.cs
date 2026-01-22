using SmartStore.AmazonPay.Services;
using Microsoft.AspNetCore.Mvc;


namespace SmartStore.AmazonPay.Controllers
{
    public class AmazonPayShoppingCartController : AmazonPayControllerBase
    {
        private readonly IAmazonPayService _apiService;

        public AmazonPayShoppingCartController(IAmazonPayService apiService)
        {
            _apiService = apiService;
        }

        public ActionResult PayButtonHandler()
        {
            var model = _apiService.CreateViewModel(AmazonPayRequestType.PayButtonHandler, TempData);
            return GetActionResult(model);
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
        public ActionResult ShoppingCart()
        {
            if (ControllerContext.ParentActionViewContext.RequestContext.RouteData.IsRouteEqual("ShoppingCart", "Cart"))
            {
                var model = _apiService.CreateViewModel(AmazonPayRequestType.ShoppingCart, TempData);

                return GetActionResult(model);
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
        public ActionResult OrderReviewData(bool renderAmazonPayView)
        {
            if (renderAmazonPayView)
            {
                var model = _apiService.CreateViewModel(AmazonPayRequestType.OrderReviewData, TempData);

                return View(model);
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
        public ActionResult MiniShoppingCart(bool renderAmazonPayView)
        {
            if (renderAmazonPayView)
            {
                var model = _apiService.CreateViewModel(AmazonPayRequestType.MiniShoppingCart, TempData);

                return GetActionResult(model);
            }

            return new EmptyResult();
        }
    }
}