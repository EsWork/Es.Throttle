using System;

namespace Es.Throttle
{
    /// <summary>
    /// 参考GCRA算法实现的Dual Leaky Bucket
    /// https://en.wikipedia.org/wiki/Generic_cell_rate_algorithm
    /// </summary>
    public class RateLimiter : IRateLimiter
    {
        private const int MaxCASAttempts = 10;
        private readonly IRateLimitStore _rateLimitStore;

        public static readonly TimeSpan NoneRetry = TimeSpan.FromTicks(-1);

        /// <summary>
        /// 参数构造函数
        /// </summary>
        /// <param name="rateLimitStore"><see cref="IRateLimitStore"/></param>
        public RateLimiter(IRateLimitStore rateLimitStore)
        {
            _rateLimitStore = rateLimitStore;
        }

        /// <summary>
        /// 参数构造函数
        /// </summary>
        /// <param name="rateQuota"><see cref="RateQuota"/></param>
        /// <param name="identity">唯一标识</param>
        /// <param name="quantity">总量</param>
        /// <returns></returns>
        public RateLimitResult RateLimit(RateQuota rateQuota, string identity, int quantity = 1)
        {
            if (rateQuota.MaxRate < 1)
                throw new IndexOutOfRangeException("MaxBurst必须大于0");
            if (rateQuota.MaxBurst < 1)
                throw new IndexOutOfRangeException("MaxBurst必须大于0");

            var gcra = Create(rateQuota);
            long limit = gcra.Item1, csii = gcra.Item2, cdvt = gcra.Item3;

            var result = new RateLimitResult { MaxLimit = limit, RetryAfter = NoneRetry };

            TimeSpan ttl;
            DateTimeOffset tat, newTat;

            var i = 0;

            while (true)
            {
                var now = DateTimeOffset.UtcNow;
                var existKey = _rateLimitStore.TryGet(identity, out long tatVal);
                if (existKey)
                    tat = DateTimeOffset.FromUnixTimeMilliseconds(tatVal);
                else
                    tat = now;

                //根据获取数量计算间隔幅度
                var increment = quantity * csii;
                if (now > tat)
                    newTat = now.AddTicks(increment);
                else
                    newTat = tat.AddTicks(increment);

                var allowAt = newTat.AddTicks(-cdvt);
                var diff = now.Ticks - allowAt.Ticks;
                if (diff < 0)
                {
                    if (increment <= cdvt)
                    {
                        result.RetryAfter = TimeSpan.FromTicks(-diff);
                    }
                    ttl = tat - now;
                    result.Limited = true;
                    break;
                }

                ttl = newTat - now;

                bool updated;
                if (existKey)
                    updated = _rateLimitStore.Update(identity, newTat.ToUnixTimeMilliseconds(), ttl);
                else
                    updated = _rateLimitStore.Add(identity, newTat.ToUnixTimeMilliseconds(), ttl);
                if (updated)
                    break;

                i++;
                if (i > MaxCASAttempts)
                {
                    throw new CASException($"{identity}速率限制存储更新失败超过{i}次。");
                }
            }

            var next = cdvt - ttl.Ticks;
            if (next > -csii)
            {
                result.Remaining = (int)(next / csii);
            }
            result.ResetAfter = ttl;

            return result;
        }

        private Tuple<long, long, long> Create(RateQuota rateQuota)
        {
            long limit;
            long csii;//信元发送的间隔时间
            long cdvt;//信元发送时间间隔的变化容限

            switch (rateQuota.PeriodType)
            {
                case RateLimitPeriod.Min:
                    csii = TimeSpan.FromTicks(TimeSpan.TicksPerMinute / rateQuota.MaxRate).Ticks;
                    break;

                case RateLimitPeriod.Hour:
                    csii = TimeSpan.FromTicks(TimeSpan.TicksPerHour / rateQuota.MaxRate).Ticks;
                    break;

                case RateLimitPeriod.Day:
                    csii = TimeSpan.FromTicks(TimeSpan.TicksPerDay / rateQuota.MaxRate).Ticks;
                    break;

                case RateLimitPeriod.Week:
                    csii = TimeSpan.FromTicks(TimeSpan.TicksPerDay * 7 / rateQuota.MaxRate).Ticks;
                    break;

                default:
                    csii = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / rateQuota.MaxRate).Ticks;
                    break;
            }

            limit = rateQuota.MaxBurst + 1;
            cdvt = csii * limit;
            return Tuple.Create(limit, csii, cdvt);
        }
    }
}