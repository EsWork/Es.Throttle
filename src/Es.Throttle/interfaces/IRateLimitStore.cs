using System;

namespace Es.Throttle
{
    /// <summary>
    /// 根据唯一标识KEY持久化存储
    /// </summary>
    public interface IRateLimitStore
    {
        /// <summary>
        /// 根据给定的key值获取相应的时间戳
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="timestampMs">时间戳(ms)</param>
        /// <returns>是否存在</returns>
        bool TryGet(string key, out long timestampMs);

        /// <summary>
        /// 根据给定的key插入，如果KEY存在不进行更新
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="timestampMs">时间戳(ms)</param>
        /// <param name="ttl">生存时间</param>
        /// <returns>是否插入成功</returns>
        bool Add(string key, long timestampMs, TimeSpan ttl);

        /// <summary>
        /// 根据给定的key更新，如KEY不存在不进行更新
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="timestampMs">时间戳(ms)</param>
        /// <param name="ttl">生存时间</param>
        /// <returns>是否更新成功</returns>
        bool Update(string key, long timestampMs, TimeSpan ttl);
    }
}