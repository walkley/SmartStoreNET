using System;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;


namespace SmartStore.Core.Security
{
    /// <summary>
    /// Checks request permission for the current customer.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public partial class PermissionAttribute : Attribute, IAuthorizationFilter
    {
        /// <summary>
        /// e.g. [Permission(PermissionSystemNames.Customer.Read)]
        /// </summary>
        /// <param name="systemName">The system name of the permission.</param>
        /// <param name="showUnauthorizedMessage">Whether to show an unauthorization message.</param>
        public PermissionAttribute(
            string systemName,
            bool showUnauthorizedMessage = true)
        {
            Guard.NotEmpty(systemName, nameof(systemName));

            SystemName = systemName;
            ShowUnauthorizedMessage = showUnauthorizedMessage;
        }

        /// <summary>
        /// The system name of the permission.
        /// </summary>
        public string SystemName { get; private set; }

        /// <summary>
        /// Whether to show an unauthorization message.
        /// </summary>
        public bool ShowUnauthorizedMessage { get; private set; }

        public IWorkContext WorkContext { get; set; }
        public IPermissionService PermissionService { get; set; }

        public virtual void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            Guard.NotNull(filterContext, nameof(filterContext));

            if (PermissionService.Authorize(SystemName, WorkContext.CurrentCustomer))
            {
                return;
            }

            try
            {
                HandleUnauthorizedRequest(filterContext);
            }
            catch
            {
                filterContext.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
            }
        }

        protected virtual void HandleUnauthorizedRequest(AuthorizationFilterContext filterContext)
        {
            HttpContext httpContext = filterContext.HttpContext;
            var request = httpContext?.Request;

            if (request == null)
            {
                return;
            }

            var message = ShowUnauthorizedMessage
                ? PermissionService.GetUnauthorizedMessage(SystemName)
                : string.Empty;

            if (request.IsAjaxRequest())
            {
                if (message.HasValue())
                {
                    httpContext.Response.Headers.Append("X-Message-Type", "error");
                    httpContext.Response.Headers.Append("X-Message", message);
                }

                if (request.Headers["Accept"].Any(x => x.IsCaseInsensitiveEqual("text/html")))
                {
                    filterContext.Result = AccessDeniedResult(message);
                }
                else
                {
                    var controllerActionDescriptor = filterContext.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
                    filterContext.Result = new JsonResult(new
                    {
                        error = true,
                        success = false,
                        controller = controllerActionDescriptor?.ControllerName,
                        action = controllerActionDescriptor?.ActionName,
                        //message
                    });
                }
            }
            else
            {
                var linkGenerator = httpContext.RequestServices.GetService<LinkGenerator>();
                var rawUrl = request.Path + request.QueryString;
                var url = linkGenerator?.GetPathByAction("AccessDenied", "Security", new { pageUrl = rawUrl, area = "Admin" });

                if (url == null)
                {
                    url = "/Admin/Security/AccessDenied";
                }

                httpContext.Items["UnauthorizedMessage"] = message;
                filterContext.Result = new RedirectResult(url);
            }
        }

        protected virtual ActionResult AccessDeniedResult(string message)
        {
            var content = message.HasValue() ? $"<div class=\"alert alert-danger\">{message}</div>" : string.Empty;

            return new ContentResult
            {
                Content = content,
                ContentType = "text/html",
                ContentEncoding = Encoding.UTF8
            };
        }
    }
}
