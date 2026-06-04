using Core.Models.WebPage;
namespace Core.Models.AppObserver
{
    /// <summary>
    /// 当前活跃应用摘要信息 ---- 时长限制检查用 
    /// </summary>
    public class AppActiveInfo
    {
        /// <summary>
        /// 进程名（如 chrome、firefox、notepad）
        /// </summary>
        public string ProcessName { get; set; }
        /// <summary>
        /// 是否浏览器应用
        /// </summary>
        public bool IsBrowser { get; set; }
        /// <summary>
        /// 当前浏览的网站（浏览器场景）
        /// </summary>
        public Site WebSite { get; set; }
        /// <summary>
        /// 从旧 AppInfo 转换
        /// </summary>
        public static AppActiveInfo FromAppInfo(AppInfo appInfo)
        {
            if (appInfo == null) return null;
            return new AppActiveInfo
            {
                ProcessName = appInfo.Process,
                IsBrowser = false,
                WebSite = null
            };
        }
        /// <summary>
        /// 从 AppInfo 并指定浏览器/站点信息
        /// </summary>
        public static AppActiveInfo FromAppInfo(AppInfo appInfo, bool isBrowser, Site webSite = null)
        {
            if (appInfo == null) return null;
            return new AppActiveInfo
            {
                ProcessName = appInfo.Process,
                IsBrowser = isBrowser,
                WebSite = webSite
            };
        }
        public override string ToString()
        {
            return $"Process:{ProcessName}, IsBrowser:{IsBrowser}, Website:{WebSite?.Url}";
        }
    }
}
