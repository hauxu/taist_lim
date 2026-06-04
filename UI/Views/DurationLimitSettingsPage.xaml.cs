using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UI.Controls.Toggle;
using UI.ViewModels;

namespace UI.Views
{
    public partial class DurationLimitSettingsPage : Page
    {
        private readonly DurationLimitSettingsViewModel _vm;

        public DurationLimitSettingsPage(DurationLimitSettingsViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            _vm = vm;
            Loaded += OnPageLoaded;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnPageLoaded;
            _vm.LoadAsync();
        }

        private void EditRule_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement fe && fe.DataContext is RuleDisplayItem item)
                ((DurationLimitSettingsViewModel)DataContext).EditRule(item.Rule);
        }

        private void DeleteRule_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement fe && fe.DataContext is RuleDisplayItem item)
                ((DurationLimitSettingsViewModel)DataContext).DeleteRule(item.Rule);
        }

        private void Toggle_ToggleChanged(object sender, EventArgs e)
        {
            if (sender is Toggle toggle && toggle.DataContext is RuleDisplayItem item)
                ((DurationLimitSettingsViewModel)DataContext).ToggleEnabled(item.Rule);
        }
    }
}
