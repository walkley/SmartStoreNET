using System.ServiceModel.Syndication;
using System.Xml;
using Microsoft.AspNetCore.Mvc;


// ReSharper disable once CheckNamespace
namespace SmartStore.Web.Framework.Modelling
{
    public class RssActionResult : ActionResult
    {
        public SyndicationFeed Feed { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.ContentType = "application/rss+xml";

            var rssFormatter = new Rss20FeedFormatter(Feed, false);
            var settings = new XmlWriterSettings { Indent = true, IndentChars = "\t", CheckCharacters = false };

            using (var writer = XmlWriter.Create(context.HttpContext.Response.Output, settings))
            {
                rssFormatter.WriteTo(writer);
            }
        }
    }
}
