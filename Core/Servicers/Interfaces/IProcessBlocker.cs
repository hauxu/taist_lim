using System;
using System.Collections.Generic;
using Core.Models.DurationLimit;

namespace Core.Servicers.Interfaces
{
    public interface IProcessBlocker
    {
        void Start();
        void Stop();
        void BlockProcess(string processName, DurationLimitRule rule);
        bool IsBlocked(string processName);
        void UnblockAll();
        event EventHandler<BlockedProcessEventArgs> ProcessBlocked;
    }

    public class BlockedProcessEventArgs : EventArgs
    {
        public string ProcessName { get; set; }
        public DurationLimitRule Rule { get; set; }
    }
}