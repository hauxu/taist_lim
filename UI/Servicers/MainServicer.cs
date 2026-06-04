using Core.Servicers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UI.Views;
namespace UI.Servicers
{
    public class MainServicer : IMainServicer
    {
        private readonly IMain main;
        private readonly IThemeServicer themeServicer;
        private readonly IInputServicer inputServicer;
        private readonly IAppContextMenuServicer appContextMenuServicer;
        private readonly IWebSiteContextMenuServicer _webSiteContext;
        private readonly IStatusBarIconServicer _statusBarIconServicer;
        private readonly IAppConfig _config;
        private readonly ILockActionExecutor _lockExecutor;
        private DurationLimitLockWindow _currentLockWindow;
        private bool isSelfStart = false;
        public MainServicer(
            IMain main,
            IThemeServicer themeServicer,
            IInputServicer inputServicer,
            IAppContextMenuServicer appContextMenuServicer,
            IWebSiteContextMenuServicer webSiteContext_,
            IStatusBarIconServicer statusBarIconServicer_,
            IAppConfig config_,
            ILockActionExecutor lockExecutor_)
        {
            this.main = main;
            this.themeServicer = themeServicer;
            this.inputServicer = inputServicer;
            this.appContextMenuServicer = appContextMenuServicer;
            _webSiteContext = webSiteContext_;
            _statusBarIconServicer = statusBarIconServicer_;
            _config = config_;
            _lockExecutor = lockExecutor_;
        }
        public void Start(bool isSelfStart)
        {
            this.isSelfStart = isSelfStart;
            _statusBarIconServicer.Init();
            main.OnStarted += Main_OnStarted;
            main.OnLimitExceeded += Main_OnLimitExceeded;
            main.Run();
        }
        private void Main_OnLimitExceeded(object sender, Core.Models.DurationLimit.DurationLimitExceededEventArgs e)
        {
            if (e?.Rule == null) return;
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (e.Rule.LockAction == Core.Models.DurationLimit.LockAction.LockScreen)
                {
                    if (_currentLockWindow != null)
                    {
                        _currentLockWindow.Close();
                        _currentLockWindow = null;
                    }
                    _currentLockWindow = new DurationLimitLockWindow(e.Rule, e.CurrentUsage);
                    _currentLockWindow.Show();
                }
                else if (e.Rule.LockAction == Core.Models.DurationLimit.LockAction.CloseProcess)
                {
                    _lockExecutor.ExecuteLock(e.Rule, e.ProcessName);
                }
                else if (e.Rule.LockAction == Core.Models.DurationLimit.LockAction.BlockLaunch)
                {
                    _lockExecutor.ExecuteLock(e.Rule, e.ProcessName);
                    string msg = $"「{e.Rule.TargetName}」今日使用时长已超限，已禁止启动";
                    _statusBarIconServicer?.ShowBalloonTip("时长限制提醒", msg, 5000);
                }
            });
        }
        private void Main_OnStarted(object sender, EventArgs e)
        {
            themeServicer.Init();
            inputServicer.Start();
            appContextMenuServicer.Init();
            _webSiteContext.Init();
            if (!isSelfStart && _config.GetConfig().General.IsStartupShowMainWindow)
            {
                _statusBarIconServicer.ShowMainWindow();
            }
        }
    }
}
