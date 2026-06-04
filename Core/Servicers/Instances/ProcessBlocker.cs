using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using Core.Models.DurationLimit;
using Core.Librarys;
using Core.Servicers.Interfaces;

namespace Core.Servicers.Instances
{
    public class ProcessBlocker : IProcessBlocker, IDisposable
    {
        private readonly Dictionary<string, BlockedProcessEntry> _blockedProcesses;
        private Timer _watchTimer;
        private Timer _dateResetTimer;
        private readonly object _lockObj = new object();
        private DateTime _lastResetDate;
        private bool _isRunning;

        public event EventHandler<BlockedProcessEventArgs> ProcessBlocked;

        public ProcessBlocker()
        {
            _blockedProcesses = new Dictionary<string, BlockedProcessEntry>(StringComparer.OrdinalIgnoreCase);
            _lastResetDate = DateTime.Now.Date;
        }

        public void Start()
        {
            if (_isRunning) return;
            _isRunning = true;

            _watchTimer = new Timer(2000);
            _watchTimer.Elapsed += WatchTimer_Elapsed;
            _watchTimer.Start();

            _dateResetTimer = new Timer(60000);
            _dateResetTimer.Elapsed += DateResetTimer_Elapsed;
            _dateResetTimer.Start();
        }

        public void Stop()
        {
            _isRunning = false;
            _watchTimer?.Stop();
            _dateResetTimer?.Stop();
        }

        public void BlockProcess(string processName, DurationLimitRule rule)
        {
            if (string.IsNullOrEmpty(processName) || rule == null) return;

            string key = processName.Replace(".exe", "");
            lock (_lockObj)
            {
                _blockedProcesses[key] = new BlockedProcessEntry
                {
                    ProcessName = key,
                    Rule = rule,
                    BlockTime = DateTime.Now
                };
            }

            KillProcess(key, rule);
        }

        public bool IsBlocked(string processName)
        {
            if (string.IsNullOrEmpty(processName)) return false;
            lock (_lockObj)
            {
                return _blockedProcesses.ContainsKey(processName.Replace(".exe", ""));
            }
        }

        public void UnblockAll()
        {
            lock (_lockObj)
            {
                _blockedProcesses.Clear();
            }
        }

        private void WatchTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!_isRunning) return;

            List<BlockedProcessEntry> entries;
            lock (_lockObj)
            {
                entries = _blockedProcesses.Values.ToList();
            }

            foreach (var entry in entries)
            {
                KillProcess(entry.ProcessName, entry.Rule);
            }
        }

        private void DateResetTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var today = DateTime.Now.Date;
                if (_lastResetDate != today)
                {
                    _lastResetDate = today;
                    UnblockAll();
                    Logger.Info("已重置进程拦截列表（新的一天）");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("DateResetTimer 异常: " + ex.Message);
            }
        }

        private void KillProcess(string processName, DurationLimitRule rule)
        {
            try
            {
                var processes = Process.GetProcessesByName(processName);
                if (processes.Length > 0)
                {
                    foreach (var process in processes)
                    {
                        try
                        {
                            process.Kill();
                            process.WaitForExit(3000);
                            Logger.Info($"已禁止启动进程: {processName}");
                            ProcessBlocked?.Invoke(this, new BlockedProcessEventArgs
                            {
                                ProcessName = processName,
                                Rule = rule
                            });
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"禁止进程 {processName} 启动失败: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"检查进程列表失败 ({processName}): {ex.Message}");
            }
        }

        public void Dispose()
        {
            Stop();
            _watchTimer?.Dispose();
            _dateResetTimer?.Dispose();
        }
    }

    internal class BlockedProcessEntry
    {
        public string ProcessName { get; set; }
        public DurationLimitRule Rule { get; set; }
        public DateTime BlockTime { get; set; }
    }
}