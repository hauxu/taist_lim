using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Core.Models.DurationLimit;
namespace UI.Views
{
    public partial class DurationLimitLockWindow : Window
    {
        private readonly DurationLimitRule _rule;
        private DispatcherTimer _timer;
        public DurationLimitLockWindow(DurationLimitRule rule, TimeSpan currentUsage)
        {
            InitializeComponent();
            _rule = rule;
            string usageStr = currentUsage.TotalHours >= 1 
                ? $"{currentUsage.TotalHours:F1} 小时" 
                : $"{currentUsage.TotalMinutes:F0} 分钟";
            string limitStr = rule.MaxDailyMinutes >= 60 
                ? $"{rule.MaxDailyMinutes / 60.0:F1} 小时" 
                : $"{rule.MaxDailyMinutes} 分钟";
            MessageText.Text = $"您今日使用「{_rule.TargetName}」已达 {usageStr}，超过每日限制 {limitStr}";
        }
        private void UnlockButton_Click(object sender, MouseButtonEventArgs e)
        {
            this.Hide();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMinutes(5);
            _timer.Tick += (s, ev) =>
            {
                _timer.Stop();
                this.Show();
            };
            _timer.Start();
        }
        protected override void OnClosed(EventArgs e)
        {
            _timer?.Stop();
            base.OnClosed(e);
        }
    }
}
