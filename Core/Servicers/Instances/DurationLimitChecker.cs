using System;
using System.Linq;
using Core.Librarys;
using Core.Models.AppObserver;
using Core.Models.DurationLimit;
using Core.Servicers.Interfaces;
using System.Collections.Generic;

namespace Core.Servicers.Instances
{
    public class DurationLimitChecker : IDurationLimitChecker
    {
        private readonly IDurationLimitRuleRepository _ruleRepository;
        private readonly IData _dataSvc;
        private readonly IWebData _webDataSvc;
        private DateTime _lastCheckDate;
        private HashSet<string> _exceededTodayCache = new HashSet<string>();
        private List<DurationLimitRule> _cachedEnabledRules;
        private DateTime _rulesCacheTime = DateTime.MinValue;
        private static readonly TimeSpan RulesCacheDuration = TimeSpan.FromSeconds(10);
        public event EventHandler<DurationLimitExceededEventArgs> LimitExceeded;
        public DurationLimitChecker(IDurationLimitRuleRepository ruleRepository, IData dataSvc, IWebData webDataSvc)
        {
            _ruleRepository = ruleRepository;
            _dataSvc = dataSvc;
            _webDataSvc = webDataSvc;
            _lastCheckDate = DateTime.Now.Date;
        }

        /// <summary>
        /// 获取已启用的规则（带缓存，避免每次查库）
        /// </summary>
        private List<DurationLimitRule> GetEnabledRules()
        {
            if (_cachedEnabledRules == null ||
                (DateTime.Now - _rulesCacheTime) > RulesCacheDuration)
            {
                _cachedEnabledRules = _ruleRepository.GetAllEnabledRules();
                _rulesCacheTime = DateTime.Now;
            }
            return _cachedEnabledRules;
        }

        /// <summary>
        /// 当规则变更时清除缓存
        /// </summary>
        public void InvalidateRulesCache()
        {
            _cachedEnabledRules = null;
            _rulesCacheTime = DateTime.MinValue;
        }

        public bool CheckIfExceeded(AppActiveInfo currentApp)
        {
            try
            {
                CheckDateReset();
                var rules = GetEnabledRules();
                if (rules == null || rules.Count == 0)
                    return false;
                var matchedRule = MatchRule(rules, currentApp);
                if (matchedRule == null)
                    return false;
                string cacheKey = $"{matchedRule.Id}_{DateTime.Now:yyyyMMdd}";
                if (_exceededTodayCache.Contains(cacheKey))
                    return true;
                var usageTime = GetDailyUsageTimeForRule(matchedRule, currentApp);
                if (usageTime.TotalMinutes >= matchedRule.MaxDailyMinutes)
                {
                    _exceededTodayCache.Add(cacheKey);
                    OnLimitExceeded(matchedRule, currentApp.ProcessName, usageTime);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("CheckIfExceeded 异常: " + ex.Message);
                return false;
            }
        }
        public TimeSpan GetDailyUsageTime(string targetKey, RuleType ruleType)
        {
            var today = DateTime.Now.Date;
            double totalSeconds = 0;
            switch (ruleType)
            {
                case RuleType.Software:
                    try
                    {
                        var dailyLog = _dataSvc.GetDateRangelogList(today, today);
                        if (dailyLog != null)
                        {
                            foreach (var item in dailyLog)
                            {
                                if (item.AppModel != null && 
                                    item.AppModel.Name.Equals(targetKey, StringComparison.OrdinalIgnoreCase))
                                {
                                    totalSeconds += item.Time;
                                }
                            }
                        }
                    }
                    catch { }
                    break;
                case RuleType.Website:
                    try
                    {
                        var todayEnd = today.AddDays(1).AddSeconds(-1);
                        var browseLogs = _webDataSvc.GetBrowseLogList(today, todayEnd);
                        if (browseLogs != null)
                        {
                            foreach (var log in browseLogs)
                            {
                                if (log.Site != null && !string.IsNullOrEmpty(log.Site.Domain) &&
                                    log.Site.Domain.Equals(targetKey, StringComparison.OrdinalIgnoreCase))
                                {
                                    totalSeconds += log.Duration;
                                }
                            }
                        }
                    }
                    catch { }
                    break;
                case RuleType.Browser:
                    try
                    {
                        var dailyLog = _dataSvc.GetDateRangelogList(today, today);
                        if (dailyLog != null)
                        {
                            foreach (var item in dailyLog)
                            {
                                if (item.AppModel != null &&
                                    item.AppModel.Name.Equals(targetKey, StringComparison.OrdinalIgnoreCase))
                                {
                                    totalSeconds += item.Time;
                                }
                            }
                        }
                    }
                    catch { }
                    break;
            }
            return TimeSpan.FromSeconds(totalSeconds);
        }
        public void ResetDaily()
        {
            _lastCheckDate = DateTime.Now.Date;
            _exceededTodayCache.Clear();
        }
        private DurationLimitRule MatchRule(List<DurationLimitRule> rules, AppActiveInfo currentApp)
        {
            // 优先匹配更具体的规则
            // 1. 如果是浏览器且有活跃网页，先尝试匹配网站规则
            if (currentApp.IsBrowser && currentApp.WebSite != null && !string.IsNullOrEmpty(currentApp.WebSite.Url))
            {
                var siteDomain = ExtractDomain(currentApp.WebSite.Url);
                var siteRule = rules.FirstOrDefault(r => 
                    r.RuleType == RuleType.Website && 
                    !string.IsNullOrEmpty(r.TargetName) &&
                    (r.TargetName.Equals(siteDomain, StringComparison.OrdinalIgnoreCase)));
                if (siteRule != null)
                    return siteRule;
            }
            // 2. 尝试匹配浏览器整体规则
            if (currentApp.IsBrowser)
            {
                var browserRule = rules.FirstOrDefault(r =>
                    r.RuleType == RuleType.Browser &&
                    !string.IsNullOrEmpty(r.TargetProcessName) &&
                    r.TargetProcessName.Equals(currentApp.ProcessName, StringComparison.OrdinalIgnoreCase));
                if (browserRule != null)
                    return browserRule;
            }
            // 3. 尝试匹配软件规则
            var appRule = rules.FirstOrDefault(r =>
                r.RuleType == RuleType.Software &&
                !string.IsNullOrEmpty(r.TargetProcessName) &&
                r.TargetProcessName.Equals(currentApp.ProcessName, StringComparison.OrdinalIgnoreCase));
            return appRule;
        }
        private TimeSpan GetDailyUsageTimeForRule(DurationLimitRule rule, AppActiveInfo currentApp)
        {
            string targetKey;
            switch (rule.RuleType)
            {
                case RuleType.Software:
                    targetKey = rule.TargetProcessName;
                    break;
                case RuleType.Website:
                    targetKey = rule.TargetName;
                    break;
                case RuleType.Browser:
                    targetKey = rule.TargetProcessName;
                    break;
                default:
                    targetKey = string.Empty;
                    break;
            }
            return GetDailyUsageTime(targetKey, rule.RuleType);
        }
        private void CheckDateReset()
        {
            var today = DateTime.Now.Date;
            if (_lastCheckDate != today)
            {
                ResetDaily();
            }
        }
        private string ExtractDomain(string url)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;
            try
            {
                var uri = new Uri(url);
                return uri.Host;
            }
            catch
            {
                return url;
            }
        }
        private void OnLimitExceeded(DurationLimitRule rule, string processName, TimeSpan currentUsage)
        {
            LimitExceeded?.Invoke(this, new DurationLimitExceededEventArgs
            {
                Rule = rule,
                ProcessName = processName,
                CurrentUsage = currentUsage,
                MaxAllowed = rule.MaxDailyMinutes
            });
        }
    }
}
