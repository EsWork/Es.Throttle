using System;

namespace Es.Throttle
{
    /// <summary>
    /// 限制器状态
    /// </summary>
    public class RateLimitResult
    {
        /// <summary>
        /// 是否已被限制
        /// </summary>
        public bool Limited { get; set; }

        /// <summary>
        /// 限制的最大请求数
        /// </summary>
        public long MaxLimit { get; set; }

        /// <summary>
        /// 剩下的最大请求数
        /// </summary>
        public long Remaining { get; set; }

        /// <summary>
        /// 直到<see cref="IRateLimiter"/>返回它其初始状态给定的最大请求数。
        /// 例如:如果一个每秒速度限制，在接收一个请求200ms前,重置将返回800ms。
        /// 你也可以认为这是剩下的时间限制。
        /// </summary>
        public TimeSpan ResetAfter { get; set; }

        /// <summary>
        /// 直到下一个请求将被允许的时间间隔。正常应该是-1,除非速率限制已经超过了。
        /// </summary>
        public TimeSpan RetryAfter { get; set; }

        public static bool operator false(RateLimitResult func)
        {
            return func.Limited == false;
        }

        public static bool operator true(RateLimitResult func)
        {
            return func.Limited == true;
        }

        public static bool operator ==(RateLimitResult func, bool _bool)
        {
            return func.Limited == _bool;
        }

        public static bool operator !=(RateLimitResult func, bool _bool)
        {
            return func.Limited != _bool;
        }

        public static bool operator !(RateLimitResult func)
        {
            return !func.Limited;
        }

        public override string ToString()
        {
            return $"Limited:{Limited}, MaxLimit:{MaxLimit}, Remaining:{Remaining}, ResetAfter:{ResetAfter}, RetryAfter:{RetryAfter}";
        }

        public override int GetHashCode()
        {
            return HashCodeHelper.GetHashCode(
                Limited.GetHashCode(),
                MaxLimit.GetHashCode(),
                Remaining.GetHashCode(),
                ResetAfter.GetHashCode(),
                RetryAfter.GetHashCode());
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as RateLimitResult);
        }

        public virtual bool Equals(RateLimitResult? other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return Equals(this, other);
        }
    }
}