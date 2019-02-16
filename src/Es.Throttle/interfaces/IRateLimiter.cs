namespace Es.Throttle
{
    /// <summary>
    /// RateLimiter 基于唯一标识限制速度
    /// </summary>
    public interface IRateLimiter
    {
        /// <summary>
        /// 根据唯一标识值检查是否已超过速度限制。
        /// </summary>
        /// <param name="rateQuota">配额比例<see cref="RateQuota"/></param>
        /// <param name="identity">唯一标识值</param>
        /// <param name="quantity">单个请求的数量（默认:1）</param>
        /// <returns><see cref="RateLimitResult"/></returns>
        RateLimitResult RateLimit(RateQuota rateQuota, string identity, int quantity = 1);
    }
}