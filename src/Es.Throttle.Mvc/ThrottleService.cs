using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Es.Throttle.Mvc
{
    public class ThrottleService : IThrottleService
    {
        private readonly ThrottlePolicy _throttlePolicy;


        private readonly static Task<bool> True = Task.FromResult(true);
        private readonly static Task<bool> False = Task.FromResult(false);

        public ThrottleService(IOptionsSnapshot<ThrottleOptions> throttleOptions)
        {
            _throttlePolicy = throttleOptions.Value.Policy;
        }

        public Task<string> ComputeThrottleIdentity(RequestContext requestContext)
        {
            var keyValues = new List<string>() { "throttle" };

            var request = requestContext.Request;

            if (_throttlePolicy.EnableIP)
            {
                keyValues.Add(requestContext.RequestIP.ToString());
            }

            if (_throttlePolicy.EnableHttpMethod)
            {
                keyValues.Add(request.Method);
            }

            if (_throttlePolicy.EnableRequestPath)
            {
                keyValues.Add(request.Path);
            }

            if (_throttlePolicy.EnableUserAgent)
            {
                keyValues.Add(request.Headers["User-Agent"]);
            }

            var idBytes = Encoding.UTF8.GetBytes(string.Join("_", keyValues));
            using (var sha1 = SHA1.Create())
            {
                return Task.FromResult(BitConverter.ToString(sha1.ComputeHash(idBytes)).Replace("-", ""));
            }
        }

        public Task<bool> IsWhitelisted(RequestContext requestContext)
        {
            if (_throttlePolicy?.IpWhitelist?.Count > 0)
            {
                var ipAddress = requestContext.RequestIP;

                if (_throttlePolicy.EnableIP)
                {
                    if (_throttlePolicy.IpWhitelist != null && _throttlePolicy.IpWhitelist.Any(white =>
                    {
                        IPHelper.GetRange(white, out IPAddress begin, out IPAddress end);
                        if (ipAddress.AddressFamily != begin.AddressFamily) return false;
                        var adrBytes = ipAddress.GetAddressBytes();
                        return Bits.GE(begin.GetAddressBytes(), adrBytes) && Bits.LE(end.GetAddressBytes(), adrBytes);
                    })) return True;
                }

                if (_throttlePolicy.EnableRequestPath)
                {
                    var requestPath = requestContext.Request.Path;
                    if (_throttlePolicy.RequestPathWhitelist != null && _throttlePolicy.RequestPathWhitelist.Any(white =>
                    {
                        return requestPath.Value?.IndexOf(white, 0, StringComparison.OrdinalIgnoreCase) != -1;
                    })) return True;
                }

                if (_throttlePolicy.EnableUserAgent)
                {
                    var userAgent = requestContext.Request.Headers["User-Agent"];

                    //禁止无效的User-Agent访问
                    if (userAgent.Count == 0)
                        return True;

                    if (_throttlePolicy.UserAgentWhitelist != null && _throttlePolicy.UserAgentWhitelist.Any(white =>
                    {
                        return userAgent.ToString().IndexOf(white, 0, StringComparison.OrdinalIgnoreCase) != -1;
                    })) return True;
                }
            }

            return False;
        }

        public Task<IEnumerable<RateQuota>> GetRateQuota(RequestContext requestContext)
        {
            return Task.FromResult(FetchRateQuota(requestContext));
        }

        private IEnumerable<RateQuota> FetchRateQuota(RequestContext requestContext)
        {
            // ip rate limit
            if (_throttlePolicy.IPRules?.Count > 0)
            {
                var ipAddress = requestContext.RequestIP;
                foreach (var entry in _throttlePolicy.IPRules)
                {
                    IPHelper.GetRange(entry.Key, out IPAddress begin, out IPAddress end);
                    if (ipAddress.AddressFamily == begin.AddressFamily)
                    {
                        var adrBytes = ipAddress.GetAddressBytes();
                        if (Bits.GE(begin.GetAddressBytes(), adrBytes) && Bits.LE(end.GetAddressBytes(), adrBytes))
                        {
                            yield return entry.Value;
                        }
                    }
                }
            }

            // UserAgent rate limit
            if (_throttlePolicy.UserAgentRules?.Count > 0)
            {
                var userAgent = requestContext.Request.Headers["User-Agent"];
                if (userAgent.Count > 0)
                {
                    foreach (var entry in _throttlePolicy.UserAgentRules)
                    {
                        if (userAgent.ToString().IndexOf(entry.Key, 0, StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            yield return entry.Value;
                        }
                    }
                }
            }

            // RequestPath rate limit
            if (_throttlePolicy.RequestPathRules?.Count > 0)
            {
                var requestPath = requestContext.Request.Path;
                foreach (var entry in _throttlePolicy.RequestPathRules)
                {
                    if (requestPath.Value?.IndexOf(entry.Key, 0, StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        yield return entry.Value;
                    }
                }
            }
        }
    }
}