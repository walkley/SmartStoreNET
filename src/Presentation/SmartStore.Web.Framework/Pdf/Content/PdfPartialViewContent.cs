using SmartStore.Services.Pdf;
using Microsoft.AspNetCore.Mvc;


namespace SmartStore.Web.Framework.Pdf
{
    public class PdfPartialViewContent : PdfHtmlContent
    {
        public PdfPartialViewContent(string partialViewName, object model, ControllerContext controllerContext)
            : base(PdfViewContent.ViewToString(partialViewName, null, model, true, controllerContext, false))
        {
        }
    }
}
