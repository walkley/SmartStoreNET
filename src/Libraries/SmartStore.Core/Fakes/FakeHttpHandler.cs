using System;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace SmartStore.Core.Fakes
{
    public class FakeHttpHandler
    {
        private readonly RequestDelegate _next;

        public bool IsReusable => true;

        public async Task Invoke(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public FakeHttpHandler(RequestDelegate next)
        {
            _next = next;
        }
    }
}
