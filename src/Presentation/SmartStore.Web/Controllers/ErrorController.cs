using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;


namespace SmartStore.Web.Controllers
{
    public class ErrorController : SmartController
    {
        [MapLegacyRoutes]
        public ActionResult NotFound()
        {
            return NotFound();
        }

        public ActionResult Index()
        {
            this.Response.StatusCode = 500;
            this.Response.TrySkipIisCustomErrors = true;

            return View("Error");
        }

        //public ActionResult DoThrow()
        //{
        //	throw Error.Application("This error was thrown on purpose for testing reasons.");
        //}

    }
}