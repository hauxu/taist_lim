using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI.Servicers
{
    /// <summary>
    /// 状态栏图标服务
    /// </summary>
    public interface IStatusBarIconServicer
    {
        void Init();
        void ShowMainWindow();
        void ShowBalloonTip(string title, string text, int timeout = 5000);
    }
}
