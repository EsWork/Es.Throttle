using System;

namespace Es.Throttle.Mvc
{
    /// <summary>
    /// 开启节流
    /// </summary>
    public class EnableThrottlingAttribute : Attribute
    {
        /// <summary>
        /// 基于<see cref="RateLimitPeriod"/>的最大请求
        /// </summary>
        public long MaxRate { get; set; }

        /// <summary>
        /// 最大突发请求
        /// </summary>
        public long MaxBurst { get; set; }

        /// <summary>
        /// 限制速率的周期类型
        /// </summary>
        public RateLimitPeriod PeriodType { get; set; }

        /// <summary>
        /// EnableThrottlingAttribute
        /// </summary>
        public EnableThrottlingAttribute()
        {
        }

        /// <summary>
        /// 节流属性注入
        /// </summary>
        /// <param name="maxRate"> 基于<see cref="RateLimitPeriod"/>的最大请求</param>
        /// <param name="maxBurst">最大突发请求</param>
        /// <param name="periodType">限制速率的周期类型</param>
        public EnableThrottlingAttribute(long maxRate, long maxBurst, RateLimitPeriod periodType)
        {
            MaxRate = maxRate;
            MaxBurst = maxBurst;
            PeriodType = periodType;
        }

        internal RateQuota GetQuota()
        {
            return new RateQuota(MaxRate, MaxBurst, PeriodType);
        }
    }
}