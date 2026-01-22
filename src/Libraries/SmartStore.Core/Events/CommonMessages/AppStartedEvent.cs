using Microsoft.AspNetCore.Http;


namespace SmartStore.Core.Events
{
    public class AppStartedEvent
    {
        public HttpContext HttpContext { get; set; }
    }
}
