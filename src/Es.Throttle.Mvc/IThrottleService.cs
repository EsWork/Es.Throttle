using System.Collections.Generic;
using System.Threading.Tasks;

namespace Es.Throttle.Mvc
{
    /// <summary>
    /// 节流业务服务
    /// </summary>
    public interface IThrottleService
    {
        /// <summary>
        /// 计算Http请求的唯一标示值
        /// </summary>
        /// <param name="requestContext"><see cref="RequestContext"/></param>
        /// <returns></returns>
        Task<string> ComputeThrottleIdentity(RequestContext requestContext);

        /// <summary>
        /// 是否在白名单内
        /// </summary>
        /// <param name="requestContext"><see cref="RequestContext"/></param>
        /// <returns></returns>
        Task<bool> IsWhitelisted(RequestContext requestContext);

        /// <summary>
        /// 获取限制配额比例列表
        /// </summary>
        /// <param name="requestContext"><see cref="RequestContext"/></param>
        /// <returns></returns>
        Task<IEnumerable<RateQuota>> GetRateQuota(RequestContext requestContext);
    }
}