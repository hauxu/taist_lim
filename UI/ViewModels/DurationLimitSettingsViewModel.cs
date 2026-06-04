using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Core.Models.DurationLimit;
using Core.Servicers.Interfaces;
using UI.Controls;
using UI.Models;

namespace UI.ViewModels
{
    public class DurationLimitSettingsViewModel : ModelBase
    {
        private readonly IDurationLimitRuleRepository _ruleRepository;
        private readonly IDurationLimitChecker _limitChecker;
        private bool _isLoaded;

        public DurationLimitSettingsViewModel(IDurationLimitRuleRepository ruleRepository, IDurationLimitChecker limitChecker)
        {
            _ruleRepository = ruleRepository;
            _limitChecker = limitChecker;
            Rules = new ObservableCollection<DurationLimitRule>();
            FilteredRules = new ObservableCollection<RuleDisplayItem>();
            AddRuleCommand = new Command(AddRule);
            TabbarData = new ObservableCollection<string> { "全部", "软件", "网站", "浏览器" };
        }

        public ObservableCollection<DurationLimitRule> Rules { get; set; }
        public ObservableCollection<RuleDisplayItem> FilteredRules { get; set; }
        public ObservableCollection<string> TabbarData { get; set; }

        private int _tabbarSelectedIndex;
        public int TabbarSelectedIndex
        {
            get => _tabbarSelectedIndex;
            set
            {
                if (_tabbarSelectedIndex == value) return;
                _tabbarSelectedIndex = value;
                OnPropertyChanged();
                if (_isLoaded) FilterRules();
            }
        }

        private Visibility _hasNoRules = Visibility.Collapsed;
        public Visibility HasNoRules
        {
            get => _hasNoRules;
            set { _hasNoRules = value; OnPropertyChanged(); }
        }

        public ICommand AddRuleCommand { get; }

        /// <summary>
        /// 页面加载完成后异步加载规则数据
        /// </summary>
        public void LoadAsync()
        {
            if (_isLoaded) return;
            _isLoaded = true;
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    var rules = _ruleRepository.GetAllRules()?.ToList() ?? new List<DurationLimitRule>();
                    Application.Current?.Dispatcher?.BeginInvoke(new Action(() =>
                    {
                        Rules.Clear();
                        foreach (var rule in rules) Rules.Add(rule);
                        FilterRules();
                    }));
                }
                catch { }
            });
        }

        public void FilterRules()
        {
            FilteredRules.Clear();
            IEnumerable<DurationLimitRule> source = Rules;
            if (_tabbarSelectedIndex == 1) source = Rules.Where(r => r.RuleType == RuleType.Software);
            else if (_tabbarSelectedIndex == 2) source = Rules.Where(r => r.RuleType == RuleType.Website);
            else if (_tabbarSelectedIndex == 3) source = Rules.Where(r => r.RuleType == RuleType.Browser);
            foreach (var rule in source) FilteredRules.Add(new RuleDisplayItem(rule));
            HasNoRules = FilteredRules.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public void AddRule(object obj)
        {
            var dialog = new Views.AddEditRuleDialog(null);
            if (dialog.ShowDialog() == true)
            {
                _ruleRepository.AddRule(dialog.ResultRule);
                _limitChecker.InvalidateRulesCache();
                LoadAsync();
            }
        }

        public void EditRule(DurationLimitRule rule)
        {
            var dialog = new Views.AddEditRuleDialog(rule);
            if (dialog.ShowDialog() == true)
            {
                _ruleRepository.UpdateRule(dialog.ResultRule);
                _limitChecker.InvalidateRulesCache();
                LoadAsync();
            }
        }

        public void DeleteRule(DurationLimitRule rule)
        {
            if (MessageBox.Show($"确定要删除规则「{rule.TargetName}」吗？", "确认删除",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _ruleRepository.DeleteRule(rule.Id);
                _limitChecker.InvalidateRulesCache();
                LoadAsync();
            }
        }

        public void ToggleEnabled(DurationLimitRule rule)
        {
            rule.IsEnabled = !rule.IsEnabled;
            _ruleRepository.UpdateRule(rule);
            _limitChecker.InvalidateRulesCache();
        }
    }

    /// <summary>
    /// 规则显示包装类
    /// </summary>
    public class RuleDisplayItem : INotifyPropertyChanged
    {
        private readonly DurationLimitRule _rule;
        public RuleDisplayItem(DurationLimitRule rule) { _rule = rule; }
        public DurationLimitRule Rule => _rule;
        public int Id => _rule.Id;
        public string TargetName => _rule.TargetName;
        public int MaxDailyMinutes => _rule.MaxDailyMinutes;
        public bool IsEnabled
        {
            get => _rule.IsEnabled;
            set { if (_rule.IsEnabled != value) { _rule.IsEnabled = value; OnPropertyChanged(); } }
        }
        public string RuleTypeText => _rule.RuleType switch
        {
            RuleType.Software => "软件",
            RuleType.Website => "网站",
            RuleType.Browser => "浏览器",
            _ => "未知"
        };
        public string RuleTypeIcon => _rule.RuleType switch
        {
            RuleType.Software => "📱",
            RuleType.Website => "🌐",
            RuleType.Browser => "🔍",
            _ => "❓"
        };
        public string LockActionText => _rule.LockAction switch
        {
            LockAction.LockScreen => "锁屏提醒",
            LockAction.CloseProcess => "关闭进程",
            LockAction.BlockLaunch => "今日禁止启动",
            _ => "未知"
        };
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
