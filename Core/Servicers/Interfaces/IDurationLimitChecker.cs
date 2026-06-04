using System;
using Core.Models.AppObserver;
using Core.Models.DurationLimit;
namespace Core.Servicers.Interfaces
{
    public interface IDurationLimitChecker
    {
        /// <summary>
        /// 检查当前应用是否超限
        /// </summary>
        bool CheckIfExceeded(AppActiveInfo currentApp);
        
        /// <summary>
        /// 获取今日累计使用时长
        /// </summary>
        TimeSpan GetDailyUsageTime(string targetKey, RuleType ruleType);
        
        /// <summary>
        /// 每日重置统计
        /// </summary>
        void ResetDaily();
        
        /// <summary>
        /// 清除规则缓存（规则变更后调用）
        /// </summary>
        void InvalidateRulesCache();
        
        /// <summary>
        /// 当检测到超限时触发事件
        /// </summary>
        event EventHandler<DurationLimitExceededEventArgs> LimitExceeded;
    }
}