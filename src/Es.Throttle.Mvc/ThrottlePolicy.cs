using System.Collections.Generic;

namespace Es.Throttle.Mvc
{
    /// <summary>
    /// 节流政策
    /// </summary>
    public class ThrottlePolicy
    {
        /// <summary>
        /// 开启IP验证
        /// </summary>
        public bool EnableIP { get; set; }

        /// <summary>
        /// 开启UserAgent验证
        /// </summary>
        public bool EnableUserAgent { get; set; }

        /// <summary>
        /// 开启HttpMethod验证
        /// </summary>
        public bool EnableHttpMethod { get; set; }

        /// <summary>
        /// 开启RequestPath验证
        /// </summary>
        public bool EnableRequestPath { get; set; }

        /// <summary>
        /// IP白名单
        /// </summary>
        public IList<string> IpWhitelist { get; set; } = new List<string>();

        /// <summary>
        /// UserAgent白名单
        /// </summary>
        public IList<string> UserAgentWhitelist { get; set; } = new List<string>();

        /// <summary>
        /// RequestPath白名单
        /// </summary>
        public IList<string> RequestPathWhitelist { get; set; } = new List<string>();

        /// <summary>
        /// 自定义UserAgent限制规则
        /// </summary>
        public Dictionary<string, RateQuota> UserAgentRules { get; set; } = new Dictionary<string, RateQuota>();

        /// <summary>
        /// 自定义IP限制规则
        /// </summary>
        public Dictionary<string, RateQuota> IPRules { get; set; } = new Dictionary<string, RateQuota>();

        /// <summary>
        /// 自定义RequestPath限制规则
        /// </summary>
        public Dictionary<string, RateQuota> RequestPathRules { get; set; } = new Dictionary<string, RateQuota>();
    }
}