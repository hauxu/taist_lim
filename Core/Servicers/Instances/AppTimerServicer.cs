using Core.Enums;
using Core.Event;
using Core.Models.AppObserver;
using Core.Servicers.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
namespace Core.Servicers.Instances
{
    public class AppTimerServicer : IAppTimerServicer
    {
        public event AppTimerEventHandler OnAppDurationUpdated;
        private readonly IAppObserver _appObserver;
        private readonly IDurationLimitChecker _limitChecker;
        private bool _isStart = false;
        private HashSet<string> _exceededProcessCache = new HashSet<string>();
        private int _appDuration = 0;
        private DateTime _startTime = DateTime.MinValue;
        private DateTime _endTime = DateTime.MinValue;
        private string _activeProcess;
        private Dictionary<string, AppData> _appData;
        private System.Timers.Timer _timer;
        private AppDurationUpdatedEventArgs _lastInvokeEventArgs;
        public struct AppData
        {
            public AppInfo App { get; set; }
            public WindowInfo Window { get; set; }
        }
        public AppTimerServicer(IAppObserver appObserver_, IDurationLimitChecker limitChecker_)
        {
            _appObserver = appObserver_;
            _limitChecker = limitChecker_;
        }
        private void Init()
        {
            _appData = new Dictionary<string, AppData>();
            _appDuration = 0;
            _activeProcess = string.Empty;
            _timer = new System.Timers.Timer();
            _timer.Interval = 1000;
            _timer.Elapsed += Timer_Elapsed;
        }
        public void Start()
        {
            if (_isStart) { return; }
            Init();
            _isStart = true;
            _appObserver.OnAppActiveChanged += AppObserver_OnAppActiveChanged;
        }
        public void Stop()
        {
            _isStart = false;
            StopTimer();
            _appObserver.OnAppActiveChanged -= AppObserver_OnAppActiveChanged;
        }
        private void AppObserver_OnAppActiveChanged(object sender, AppActiveChangedEventArgs e)
        {
            string processName = e.App.Process;
            bool isStatistical = IsStatistical(e.App);
            Debug.WriteLine(processName+ " -> " + _activeProcess);
            if (processName != _activeProcess)
            {
                StopTimer();
                InvokeEvent();
                if (isStatistical)
                {
                    StartTimer();
                }
                _activeProcess = e.App.Type == AppType.SystemComponent ? string.Empty : processName;
            }
            if (isStatistical)
            {
                var data = new AppData()
                {
                    App = e.App,
                    Window = e.Window,
                };
                if (!_appData.ContainsKey(processName))
                {
                    //  缓存应用信息
                    _appData.Add(processName, data);
                }
                else
                {
                    //  更新缓存
                    _appData[processName] = data;
                }
            }
        }
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _appDuration++;
            // 每30秒检查一次时长限制（仅针对持续使用同一应用的情况）
            if (_appDuration > 0 && _appDuration % 30 == 0 && !string.IsNullOrEmpty(_activeProcess))
            {
                CheckDurationLimit();
            }
        }
        private void CheckDurationLimit()
        {
            try
            {
                if (_appData.ContainsKey(_activeProcess))
                {
                    var data = _appData[_activeProcess];
                    var appInfo = data.App;
                    bool isBrowser = IsBrowser(appInfo.Process);
                    var activeInfo = AppActiveInfo.FromAppInfo(appInfo, isBrowser, null);
                    _limitChecker.CheckIfExceeded(activeInfo);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DurationLimit check error: {ex.Message}");
            }
        }
        private bool IsBrowser(string processName)
        {
            if (string.IsNullOrEmpty(processName))
                return false;
            string name = processName.ToLower().Replace(".exe", "");
            string[] browsers = { "chrome", "firefox", "msedge", "iexplore", "opera", "brave",
                "vivaldi", "sogouexplorer", "360se", "360chrome", "qqbrowser", "browser", "ucbrowser" };
            foreach (var b in browsers)
            {
                if (name == b || name.Contains(b))
                    return true;
            }
            return false;
        }
        private void StartTimer()
        {
            _timer.Start();
            _startTime = DateTime.Now;
            _appDuration = 0;
        }
        private void StopTimer()
        {
            _timer.Stop();
            _endTime = DateTime.Now;
        }
        private void InvokeEvent()
        {
            var info = GetAppDuration();
            if (info != null)
            {
                if (_lastInvokeEventArgs?.ActiveTime.ToString() != _startTime.ToString() || _lastInvokeEventArgs?.EndTime.ToString() != _endTime.ToString())
                {
                    Debug.WriteLine("【计时更新】" + info);
                    OnAppDurationUpdated?.Invoke(this, info);
                }
                else
                {
                    Debug.WriteLine("【重复！！】" + _lastInvokeEventArgs + "，【now】" + info);
                }
                _lastInvokeEventArgs = info;
            }
        }
        /// <summary>
        /// 判断应用是否需要被统计
        /// </summary>
        /// <param name="app_"></param>
        /// <returns>需要统计时返回 true </returns>
        private bool IsStatistical(AppInfo app_)
        {
            return !string.IsNullOrEmpty(app_.Process) && app_.Type != AppType.SystemComponent && !string.IsNullOrEmpty(app_.ExecutablePath);
        }
        public AppDurationUpdatedEventArgs GetAppDuration()
        {
            if (_appDuration > 0 && !string.IsNullOrEmpty(_activeProcess) && _appData.ContainsKey(_activeProcess))
            {
                var data = _appData[_activeProcess];
                var args = new AppDurationUpdatedEventArgs(_appDuration, data.App, data.Window, _startTime, _endTime);
                return args;
            }
            return null;
        }
    }
}