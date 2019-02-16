namespace Es.Throttle
{
    /// <summary>
    /// 限制速率的周期类型
    /// </summary>
    public enum RateLimitPeriod
    {
        /// <summary>
        /// 秒为单位
        /// </summary>
        Sec = 1,

        /// <summary>
        /// 分钟为单位
        /// </summary>
        Min,

        /// <summary>
        /// 小时为单位
        /// </summary>
        Hour,

        /// <summary>
        /// 天为单位
        /// </summary>
        Day,

        /// <summary>
        /// 周为单位
        /// </summary>
        Week
    }
}