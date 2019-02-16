using System.Net;
using Microsoft.AspNetCore.Http;

namespace Es.Throttle.Mvc
{
    /// <summary>
    /// Http请求上下文参数
    /// </summary>
    public class RequestContext
    {
        public HttpRequest Request { get; set; }

        public IPAddress RequestIP { get; set; }

        public string ControllerName { get; set; }

        public string ActionName { get; set; }
    }
}