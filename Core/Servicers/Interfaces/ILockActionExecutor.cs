using Core.Models.DurationLimit;
namespace Core.Servicers.Interfaces
{
    public interface ILockActionExecutor
    {
        /// <summary>
        /// 执行锁定动作
        /// </summary>
        void ExecuteLock(DurationLimitRule rule, string processName = null);
    }
}