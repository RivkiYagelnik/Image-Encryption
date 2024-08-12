using Serilog;
using System.Net;
namespace Image_encryption.middleWare
{
    
    public class Action_documentation
    {
        private readonly RequestDelegate _next;
        public Action_documentation(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext httpContext)
        {
            
                var myAction = httpContext.GetRouteData().Values["action"]?.ToString();
                Log.Information("action:" + " " + myAction);
                Log.Information("from new middleware");
                await _next(httpContext);
           
        }
    }
}
