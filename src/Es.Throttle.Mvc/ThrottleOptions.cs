namespace Es.Throttle.Mvc
{
    /// <summary>
    /// 节流选项
    /// </summary>
    public class ThrottleOptions
    {
        /// <summary>
        /// 开启全局节流过滤
        /// </summary>
        public bool EnableGlobalFilter { get; set; }

        /// <summary>
        /// 是否开启分布式存储
        /// </summary>
        public bool UseDistributed { get; set; }

        /// <summary>
        /// 节流政策
        /// </summary>
        public ThrottlePolicy Policy { get; set; }
    }
}