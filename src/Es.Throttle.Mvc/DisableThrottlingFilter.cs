using Microsoft.AspNetCore.Mvc.Filters;

namespace Es.Throttle.Mvc
{
    /// <summary>
    /// 禁用节流过滤
    /// </summary>
    public class DisableThrottlingFilter : IFilterMetadata
    {
    }
}