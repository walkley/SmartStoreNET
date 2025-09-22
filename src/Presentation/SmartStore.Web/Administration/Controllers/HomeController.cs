using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Xml;
using SmartStore.Admin.Models.Common;
using SmartStore.Core;
using SmartStore.Core.Domain.Common;
using SmartStore.Services;
using SmartStore.Services.Common;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc.ModelBinding;


namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class HomeController : AdminControllerBase
    {
        #region Fields

        private readonly ICommonServices _services;
        private readonly CommonSettings _commonSettings;
        private readonly Lazy<IUserAgent> _userAgent;
        private readonly CustomerController _customerController;

        #endregion

        #region Ctor

        public HomeController(
            ICommonServices services,
            CommonSettings commonSettings,
            Lazy<IUserAgent> userAgent,
            CustomerController customerController
            )
        {
            _commonSettings = commonSettings;
            _services = services;
            _userAgent = userAgent;
            _customerController = customerController;
        }

        #endregion

        #region Methods

        public ActionResult Index()
        {
            Debug.WriteLine("--------------------------");
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult UaTester(string ua = null)
        {
            if (ua.HasValue())
            {
                _userAgent.Value.RawValue = ua;
            }
            return View(_userAgent.Value);
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
        public ActionResult SmartStoreNews()
        {
            try
            {
                string feedUrl = string.Format("https://www.smartstore.com/NewsRSS.aspx?Version={0}&Localhost={1}&HideAdvertisements={2}&StoreURL={3}",
                    SmartStoreVersion.CurrentVersion,
                    Request.Url.IsLoopback,
                    _commonSettings.HideAdvertisementsOnAdminArea,
                    _services.StoreContext.CurrentStore.Url);

                //specify timeout (5 secs)
                var request = WebRequest.Create(feedUrl);
                request.Timeout = 5000;
                using (WebResponse response = request.GetResponse())
                using (var reader = XmlReader.Create(response.GetResponseStream()))
                {
                    var rssData = SyndicationFeed.Load(reader);
                    return PartialView(rssData);
                }
            }
            catch (Exception)
            {
                return Content("");
            }
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
        public ActionResult MarketplaceFeed()
        {
            var watch = new Stopwatch();
            watch.Start();

            var result = _services.Cache.Get("admin:marketplacefeed", () =>
            {
                try
                {
                    string url = "http://community.smartstore.com/index.php?/rss/downloads/";
                    var request = (HttpWebRequest)WebRequest.Create(url);
                    request.Timeout = 3000;
                    request.UserAgent = "Smartstore {0}".FormatInvariant(SmartStoreVersion.CurrentFullVersion);

                    using (WebResponse response = request.GetResponse())
                    {
                        using (var reader = XmlReader.Create(response.GetResponseStream()))
                        {
                            var feed = SyndicationFeed.Load(reader);
                            var model = new List<FeedItemModel>();
                            foreach (var item in feed.Items)
                            {
                                if (!item.Id.EndsWith("error=1", StringComparison.OrdinalIgnoreCase))
                                {
                                    var modelItem = new FeedItemModel();
                                    modelItem.Title = item.Title.Text;
                                    modelItem.Summary = item.Summary.Text.RemoveHtml().Truncate(150, "...");
                                    modelItem.PublishDate = item.PublishDate.LocalDateTime.RelativeFormat();

                                    var link = item.Links.FirstOrDefault();
                                    if (link != null)
                                    {
                                        modelItem.Link = link.Uri.ToString();
                                    }

                                    model.Add(modelItem);
                                }
                            }

                            return model;
                        }
                    }
                }
                catch (Exception ex)
                {
                    return new List<FeedItemModel> { new FeedItemModel { IsError = true, Summary = ex.Message } };
                }
            }, TimeSpan.FromHours(12));

            if (result.Any() && result.First().IsError)
            {
                ModelState.AddModelError("", result.First().Summary);
            }

            watch.Stop();
            Debug.WriteLine("MarketplaceFeed >>> " + watch.ElapsedMilliseconds);

            return PartialView(result);
        }

        [HttpPost]
        public ActionResult SmartStoreNewsHideAdv()
        {
            _commonSettings.HideAdvertisementsOnAdminArea = !_commonSettings.HideAdvertisementsOnAdminArea;
            _services.Settings.SaveSetting(_commonSettings);
            return Content("Setting changed");
        }
        #endregion
    }
}
