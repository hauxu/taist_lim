# taist_lim

在 Windows 上统计软件使用时长和网站浏览时长，新增**时长限制**功能。

> 基于 [Planshit/Tai](https://github.com/Planshit/Tai) 二次开发

## 新增功能：时长限制

- 对**软件**、**网站**、**浏览器**设置每日使用时长上限
- 超限后可执行：锁屏提醒、关闭进程、今日禁止启动
- 分类筛选切换（全部 / 软件 / 网站 / 浏览器）
- Toggle 开关快速启用/禁用规则

## 环境

需要 [.NET Framework 4.8](https://dotnet.microsoft.com/en-us/download/dotnet-framework)（Win10 以上一般已自带）。

## 使用

1. 在 [Releases](https://github.com/hauxu/taist_lim/releases) 下载 `taist_lim_v*.zip`
2. 解压到任意目录，运行 `Tai.exe`
3. 状态栏出现图标即启动成功
4. 网站统计需安装浏览器插件，启用方法参考[原项目说明](https://github.com/Planshit/Tai/discussions/279)

### 时长限制使用

1. 点击左侧导航 **"时长限制"** 进入设置页
2. 点击 **"添加规则"**，选择类型（软件/网站/浏览器）
3. 输入进程名或域名，设置每日分钟上限和超限动作
4. 也可通过 Toggle 开关随时启用/禁用规则

## 许可证

与原项目一致，详见 [LICENSE](LICENSE)。
