using System;
namespace Core.Models.DurationLimit
{
    /// <summary>
    /// 时长限制规则实体
    /// </summary>
    public class DurationLimitRule
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// 规则类型
        /// </summary>
        public RuleType RuleType { get; set; }
        
        /// <summary>
        /// 目标名称（显示名）
        /// </summary>
        public string TargetName { get; set; }
        
        /// <summary>
        /// 目标进程名（软件和浏览器使用）
        /// </summary>
        public string TargetProcessName { get; set; }
        
        /// <summary>
        /// 每日最大允许分钟数
        /// </summary>
        public int MaxDailyMinutes { get; set; }
        
        /// <summary>
        /// 超限后执行动作
        /// </summary>
        public LockAction LockAction { get; set; }
        
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }
        
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }
    }
    /// <summary>
    /// 规则类型
    /// </summary>
    public enum RuleType
    {
        /// <summary>
        /// 软件限制
        /// </summary>
        Software = 0,
        
        /// <summary>
        /// 网站限制
        /// </summary>
        Website = 1,
        
        /// <summary>
        /// 浏览器整体限制
        /// </summary>
        Browser = 2
    }
    /// <summary>
    /// 超限执行动作
    /// </summary>
    public enum LockAction
    {
        /// <summary>
        /// 锁屏遮挡
        /// </summary>
        LockScreen = 0,
        
        /// <summary>
        /// 关闭进程
        /// </summary>
        CloseProcess = 1,
        
        /// <summary>
        /// 今日禁止启动
        /// </summary>
        BlockLaunch = 2
    }
    /// <summary>
    /// 超限事件参数
    /// </summary>
    public class DurationLimitExceededEventArgs : EventArgs
    {
        public DurationLimitRule Rule { get; set; }
        public string ProcessName { get; set; }
        public TimeSpan CurrentUsage { get; set; }
        public int MaxAllowed { get; set; }
    }
}