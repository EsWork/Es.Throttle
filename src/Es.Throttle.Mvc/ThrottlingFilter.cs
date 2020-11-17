using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Es.Throttle.Mvc
{
    /// <summary>
    /// 全局节流过滤拦截器
    /// </summary>
    public class ThrottlingFilter : ActionFilterAttribute
    {
        private const int StatusCode = 429;

        private readonly IRateLimiter _rateLimiter;
        private readonly IThrottleService _throttleService;

        private readonly ThrottleOptions _throttleOptions;

        public ThrottlingFilter(
            IRateLimiter rateLimiter,
            IThrottleService throttleService,
            IOptionsSnapshot<ThrottleOptions> throttleOptions)
        {
            _throttleOptions = throttleOptions.Value;
            _rateLimiter = rateLimiter;
            _throttleService = throttleService;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            var applyThrottling = ApplyThrottling(context, out EnableThrottlingAttribute enableThrottlingAttribute);

            if (applyThrottling)
            {
                //获取请求的身份标识
                var requestContext = SetRequestContext(context);

                //白名单列表不属于限制内
                if (!(await _throttleService.IsWhitelisted(requestContext)))
                {
                    var identity = await _throttleService.ComputeThrottleIdentity(requestContext);

                    //1.先获取Attribute的限制规则
                    var rateQuota = enableThrottlingAttribute?.GetQuota();

                    RateLimitResult result;

                    //自定义优先覆盖固定Attribute的设置，这样可以交由外部存储的设置动态调整
                    var rateQuotas = await _throttleService.GetRateQuota(requestContext);
                    foreach (var quota in rateQuotas)
                    {
                        if (quota?.MaxBurst > 0)
                        {
                            //2.如果有自定义则排除Attribute的规则
                            rateQuota = null;

                            result = _rateLimiter.RateLimit(quota, identity);
                            if (result)
                            {
                                SetRateLimitResponse(context, result);
                                return;
                            }
                        }
                    }

                    if (rateQuota != RateQuota.None && rateQuota?.MaxBurst > 0)
                    {
                        result = _rateLimiter.RateLimit(rateQuota, identity);
                        if (result)
                        {
                            SetRateLimitResponse(context, result);
                            return;
                        }
                    }
                }
            }

            await next();
        }

        protected virtual void SetRateLimitResponse(ActionExecutingContext context, RateLimitResult rateLimitResult)
        {
            var headers = context.HttpContext.Response.Headers;
            if (rateLimitResult.MaxLimit >= 0)
            {
                headers.Add("X-RateLimit-Limit", rateLimitResult.MaxLimit.ToString());
            }
            if (rateLimitResult.Remaining >= 0)
            {
                headers.Add("X-RateLimit-Remaining", rateLimitResult.Remaining.ToString());
            }
            if (rateLimitResult.ResetAfter.Ticks >= 0)
            {
                headers.Add("X-RateLimit-Reset", rateLimitResult.ResetAfter.TotalSeconds.ToString());
            }
            if (rateLimitResult.RetryAfter.Ticks >= 0)
            {
                headers.Add("Retry-After", rateLimitResult.RetryAfter.TotalSeconds.ToString());
            }
            context.Result = new StatusCodeResult(StatusCode);
        }

        protected virtual RequestContext SetRequestContext(ActionExecutingContext context)
        {
            var rqIdentity = new RequestContext();

            if (context.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
            {
                rqIdentity.ControllerName = controllerActionDescriptor.ControllerName;
                rqIdentity.ActionName = controllerActionDescriptor.ActionName;
            }

            rqIdentity.Request = context.HttpContext.Request;
            rqIdentity.RequestIP = GetClientIP(context.HttpContext, true);

            return rqIdentity;
        }

        protected virtual IPAddress GetClientIP(HttpContext context, bool filterPrivateIP = false)
        {
            var httpConnectionFeature = context.Features.Get<IHttpConnectionFeature>();

            if (httpConnectionFeature == null)
            {
                return IPAddress.Loopback;
            }

            var ipAddress = httpConnectionFeature.RemoteIpAddress;

            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out StringValues xForwardedFor))
            {
                if (!StringValues.IsNullOrEmpty(xForwardedFor))
                {
                    var forwardingIps = xForwardedFor.ToString().Split(',').Select(s =>
                    {
                        return IPAddress.TryParse(s, out IPAddress ipd) ? ipd : null;
                    }).ToList();

                    if (filterPrivateIP)
                    {
                        forwardingIps = forwardingIps.Where(s => s != null && !IPHelper.IsPrivateIpAddress(s)).ToList();
                    }

                    if (forwardingIps.Any())
                    {
                        return forwardingIps.FirstOrDefault();
                    }

                    return ipAddress;
                }
            }
            return ipAddress;
        }

        private bool ApplyThrottling(ActionExecutingContext context, out EnableThrottlingAttribute throttlingAttribute)
        {
            throttlingAttribute = null;

            if (context.Filters.Any(item => item is DisableThrottlingFilter))
                return false;

            //如果开启全局节流
            if (_throttleOptions.EnableGlobalFilter)
                return true;

            if (context.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
            {
                throttlingAttribute = controllerActionDescriptor.ControllerTypeInfo
                    .GetCustomAttribute<EnableThrottlingAttribute>(true);
                if (throttlingAttribute != null)
                    return true;

                throttlingAttribute = controllerActionDescriptor.MethodInfo
                    .GetCustomAttribute<EnableThrottlingAttribute>(inherit: true);
                if (throttlingAttribute != null)
                    return true;
            }

            return false;
        }
    }
}