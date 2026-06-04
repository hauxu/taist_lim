using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Core.Models.DurationLimit;
namespace UI.Views
{
    public partial class AddEditRuleDialog : Window
    {
        public DurationLimitRule ResultRule { get; private set; }

        public AddEditRuleDialog(DurationLimitRule existingRule)
        {
            InitializeComponent();

            RuleTypeBox.SelectedIndex = 0;
            LockActionBox.SelectedIndex = 0;

            if (existingRule != null)
            {
                ResultRule = new DurationLimitRule
                {
                    Id = existingRule.Id,
                    RuleType = existingRule.RuleType,
                    TargetName = existingRule.TargetName,
                    TargetProcessName = existingRule.TargetProcessName,
                    MaxDailyMinutes = existingRule.MaxDailyMinutes,
                    LockAction = existingRule.LockAction,
                    IsEnabled = existingRule.IsEnabled,
                    CreateTime = existingRule.CreateTime,
                    UpdateTime = existingRule.UpdateTime
                };
                TitleText.Text = "编辑规则";
                TargetNameBox.Text = existingRule.TargetName;
                ProcessNameBox.Text = existingRule.TargetProcessName ?? string.Empty;
                RuleTypeBox.SelectedIndex = (int)existingRule.RuleType;
                MaxMinutesBox.Text = existingRule.MaxDailyMinutes.ToString();
                LockActionBox.SelectedIndex = (int)existingRule.LockAction;
                EnabledToggle.IsChecked = existingRule.IsEnabled;
            }
            else
            {
                ResultRule = new DurationLimitRule();
                TitleText.Text = "添加规则";
            }

            UpdateFieldsForRuleType();
        }

        private void RuleTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateFieldsForRuleType();
        }

        private void UpdateFieldsForRuleType()
        {
            var type = (RuleType)RuleTypeBox.SelectedIndex;
            switch (type)
            {
                case RuleType.Software:
                    TargetNameLabel.Text = "显示名称";
                    ProcessNameLabel.Text = "进程名";
                    ProcessNameHint.Text = "如 WeChat、chrome";
                    ProcessNameLabel.Visibility = Visibility.Visible;
                    ProcessNameBox.Visibility = Visibility.Visible;
                    ProcessNameHint.Visibility = Visibility.Visible;
                    break;
                case RuleType.Website:
                    TargetNameLabel.Text = "网站域名";
                    ProcessNameLabel.Visibility = Visibility.Collapsed;
                    ProcessNameBox.Visibility = Visibility.Collapsed;
                    ProcessNameHint.Visibility = Visibility.Collapsed;
                    break;
                case RuleType.Browser:
                    TargetNameLabel.Text = "浏览器名称";
                    ProcessNameLabel.Text = "进程名";
                    ProcessNameHint.Text = "如 chrome、msedge、firefox";
                    ProcessNameLabel.Visibility = Visibility.Visible;
                    ProcessNameBox.Visibility = Visibility.Visible;
                    ProcessNameHint.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void OkButton_Click(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TargetNameBox.Text))
            {
                MessageBox.Show("请输入目标名称", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!int.TryParse(MaxMinutesBox.Text, out int minutes) || minutes <= 0)
            {
                MessageBox.Show("请输入有效的分钟数（大于0的整数）", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var type = (RuleType)RuleTypeBox.SelectedIndex;
            if (type == RuleType.Software && string.IsNullOrWhiteSpace(ProcessNameBox.Text))
            {
                MessageBox.Show("软件类型规则需要填写进程名", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (type == RuleType.Browser && string.IsNullOrWhiteSpace(ProcessNameBox.Text))
            {
                MessageBox.Show("浏览器类型规则需要填写进程名", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ResultRule.TargetName = TargetNameBox.Text.Trim();
            ResultRule.RuleType = type;
            ResultRule.MaxDailyMinutes = minutes;
            ResultRule.LockAction = (LockAction)LockActionBox.SelectedIndex;
            ResultRule.IsEnabled = EnabledToggle.IsChecked;

            if (type == RuleType.Website)
            {
                ResultRule.TargetProcessName = null;
            }
            else
            {
                ResultRule.TargetProcessName = ProcessNameBox.Text.Trim().Replace(".exe", "");
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, MouseButtonEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Border border && border == (sender as Border))
            {
                DragMove();
            }
        }
    }
}
