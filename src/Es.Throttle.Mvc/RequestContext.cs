using System.Net;
using Microsoft.AspNetCore.Http;

namespace Es.Throttle.Mvc
{
    /// <summary>
    /// Http请求上下文参数
    /// </summary>
    public class RequestContext
    {
        public HttpRequest Request { get; set; } = default!;

        public IPAddress RequestIP { get; set; } = default!;

        public string? ControllerName { get; set; }

        public string? ActionName { get; set; }
    }
}