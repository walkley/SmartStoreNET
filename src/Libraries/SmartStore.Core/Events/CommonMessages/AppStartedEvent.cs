using Microsoft.AspNetCore.Http;


namespace SmartStore.Core.Events
{
    public class AppStartedEvent
    {
        public HttpContextBase HttpContext { get; set; }
    }
}
