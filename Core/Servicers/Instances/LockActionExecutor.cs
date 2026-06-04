using System;
using System.Diagnostics;
using Core.Models.DurationLimit;
using Core.Librarys;
using Core.Servicers.Interfaces;

namespace Core.Servicers.Instances
{
    public class LockActionExecutor : ILockActionExecutor
    {
        private readonly IProcessBlocker _processBlocker;

        public LockActionExecutor(IProcessBlocker processBlocker)
        {
            _processBlocker = processBlocker;
        }

        public void ExecuteLock(DurationLimitRule rule, string processName = null)
        {
            Logger.Info($"执行超限锁定，规则ID: {rule.Id}, 动作: {rule.LockAction}");
            switch (rule.LockAction)
            {
                case LockAction.LockScreen:
                    break;
                case LockAction.CloseProcess:
                    string targetProcess = !string.IsNullOrEmpty(rule.TargetProcessName) 
                        ? rule.TargetProcessName 
                        : processName;
                    CloseProcess(targetProcess);
                    break;
                case LockAction.BlockLaunch:
                    string blockProcess = !string.IsNullOrEmpty(rule.TargetProcessName)
                        ? rule.TargetProcessName
                        : processName;
                    _processBlocker.BlockProcess(blockProcess, rule);
                    break;
            }
        }
        private void CloseProcess(string processName)
        {
            if (string.IsNullOrEmpty(processName))
                return;
            try
            {
                var processes = Process.GetProcessesByName(processName.Replace(".exe", ""));
                foreach (var process in processes)
                {
                    try
                    {
                        process.Kill();
                        process.WaitForExit(3000);
                        Logger.Info($"已关闭超限进程: {processName}");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"关闭进程 {processName} 失败: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"获取进程列表失败: {ex.Message}");
            }
        }
    }
}
