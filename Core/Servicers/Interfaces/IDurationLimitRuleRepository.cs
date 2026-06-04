using System.Collections.Generic;
using Core.Models.DurationLimit;
namespace Core.Servicers.Interfaces
{
    public interface IDurationLimitRuleRepository
    {
        /// <summary>
        /// 获取所有规则
        /// </summary>
        List<DurationLimitRule> GetAllRules();
        
        /// <summary>
        /// 获取所有启用的规则
        /// </summary>
        List<DurationLimitRule> GetAllEnabledRules();
        
        /// <summary>
        /// 添加规则
        /// </summary>
        void AddRule(DurationLimitRule rule);
        
        /// <summary>
        /// 更新规则
        /// </summary>
        void UpdateRule(DurationLimitRule rule);
        
        /// <summary>
        /// 删除规则
        /// </summary>
        void DeleteRule(int ruleId);
        
        /// <summary>
        /// 初始化数据表
        /// </summary>
        void InitializeTable();
    }
}