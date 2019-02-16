namespace Es.Throttle
{
    /// <summary>
    /// 限制配额比例
    /// </summary>
    public class RateQuota
    {
        /// <summary>
        /// None Quota
        /// </summary>
        public static RateQuota None = new RateQuota(0, 0, RateLimitPeriod.Sec);

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
        /// Initializes a new instance of the <see cref="RateQuota"/> class.
        /// </summary>
        /// <param name="maxRate">基于<see cref="RateLimitPeriod" />的最大请求</param>
        /// <param name="maxBurst">最大突发请求</param>
        /// <param name="periodType"><see cref="RateLimitPeriod" /></param>
        public RateQuota(long maxRate, long maxBurst, RateLimitPeriod periodType)
        {
            MaxRate = maxRate;
            MaxBurst = maxBurst;
            PeriodType = periodType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateQuota"/> class.
        /// </summary>
        public RateQuota()
        {

        }
    }
}