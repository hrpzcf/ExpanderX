using CommonUtils;
using ExpanderXSDK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using static System.Environment;
using Forms = System.Windows.Forms;

namespace ExpanderX
{
    internal static class RES
    {
        public static readonly ResourceDictionary Res = new ResourceDictionary()
        {
            Source = new Uri("ResourceDict.xaml", UriKind.Relative)
        };
    }

    /// <summary>
    /// ExpanderXMainUI.xaml 的交互逻辑。
    /// </summary>
    public partial class ExpanderXMain : Window
    {
        internal const string mutexName = "5sac3as8wa32ac3a6w8p";
        internal static readonly Circulator Cycle = new Circulator();
        internal static ExpanderXMain Instance = null;

        private readonly List<UserControl> ifaceTaskModules = new List<UserControl>
        {
            new UserCtrlKeywordDetector(),
            new UserCtrlExpanderXCtrl(),
            new UserCtrlMessageSender(),
            new UserCtrlCmdExecutor(),
            new UserCtrlTopTipsExecutor(),
        };
        private readonly IFormatter formatter = new BinaryFormatter();
        private readonly List<int> confedMatchers = new List<int>();
        private readonly List<int> confedExecutors = new List<int>();
        private readonly List<AbsRuleModel> confedRuleModels = new List<AbsRuleModel>();
        private readonly List<AbsTaskModule> confedTaskModules = new List<AbsTaskModule>();

        /// <summary>
        /// 服务启动和停止事件，以 Cmd 枚举值表示启动和停止。
        /// </summary>
        private event Action<Cmd> ServiceStartStopEvent = null;

        /// <summary>
        /// 程序退出发起者。<br/>
        /// 0 代表主窗口右上角关闭按钮退出，1 代表托盘图标菜单退出。
        /// </summary>
        private int ApplicationExitSender = 0;

        private Mutex mutex;
        private readonly Forms.NotifyIcon notifyIcon = new Forms.NotifyIcon();
        private readonly WindowInteropHelper windowHelper;

        internal static readonly string basicFolder = Path.Combine(
            GetFolderPath(SpecialFolder.LocalApplicationData),
            "ExpanderX"
        );
        private readonly DirectoryInfo rulesFolder = new DirectoryInfo(
            Path.Combine(basicFolder, "RuleFiles")
        );
        private readonly DirectoryInfo pluginsFolder = new DirectoryInfo(
            Path.Combine(basicFolder, "Plugins")
        );

        /// <summary>
        /// 请求处理器，专门处理外部发起的 ExpanderX 启停、退出请求。
        /// </summary>
        private readonly RequestHandler reqHandler = new RequestHandler();

        /// <summary>
        /// 主窗口初始化构造方法。
        /// </summary>
        public ExpanderXMain()
        {
            this.CheckIfMutexHasBeenCreated();
            this.InitializeComponent();
            this.LoadMainUIWindowSizeSettings();
            this.CheckLicenseAgreement();
            this.LoadPluginsFromLocalFile();
            this.LoadRulesFromLocalFile();
            this.windowHelper = new WindowInteropHelper(this);
            this.Closing += this.ExpanderXMainClosing;
            this.ServiceStartStopEvent += this.SetupByRunningState;
            Application.Current.SessionEnding += this.CurrentSessionEnding;
            this.SetupNotifyIconAndMenuItems();
            Instance = this;
            this.reqHandler.Start();
        }

        private void CheckIfMutexHasBeenCreated()
        {
            this.mutex = new Mutex(true, mutexName, out bool newMutex);
            if (!newMutex)
            {
                USER.MessageBox(
                    IntPtr.Zero,
                    "已经有一个 ExpanderX 实例正在运行！",
                    "程序已运行",
                    MB.MB_TOPMOST | MB.MB_ICONWARNING
                );
                Exit(0);
            }
        }

        private void SetupByRunningState(Cmd cmd)
        {
            if (cmd == Cmd.Start)
            {
                this.uiButton_Start.IsEnabled = false;
                this.uiButton_Stop.IsEnabled = true;
                this.notifyIcon.ContextMenu.MenuItems[0].Enabled = false;
                this.notifyIcon.ContextMenu.MenuItems[1].Enabled = true;
                this.uiListBox_ConfedRulesDisplay.IsEnabled = false;
                this.notifyIcon.Text = "ExpanderX\n循环服务已开启";
                this.uiProgressBar_RunState.IsIndeterminate = true;
            }
            else if (cmd == Cmd.Stop)
            {
                this.uiButton_Start.IsEnabled = true;
                this.uiButton_Stop.IsEnabled = false;
                this.notifyIcon.ContextMenu.MenuItems[0].Enabled = true;
                this.notifyIcon.ContextMenu.MenuItems[1].Enabled = false;
                this.uiListBox_ConfedRulesDisplay.IsEnabled = true;
                this.notifyIcon.Text = "ExpanderX\n循环服务已停止";
                this.uiProgressBar_RunState.IsIndeterminate = false;
            }
            else
            {
                this.uiButton_Start.IsEnabled = true;
                this.uiButton_Stop.IsEnabled = true;
                this.notifyIcon.ContextMenu.MenuItems[0].Enabled = true;
                this.notifyIcon.ContextMenu.MenuItems[1].Enabled = true;
                this.uiListBox_ConfedRulesDisplay.IsEnabled = true;
                this.notifyIcon.Text = "ExpanderX\n循环服务已停止";
                this.uiProgressBar_RunState.IsIndeterminate = false;
            }
        }

        private void SetupNotifyIconAndMenuItems()
        {
            Forms.MenuItem menuItemStartSrv = new Forms.MenuItem();
            menuItemStartSrv.Click += this.MenuItemStartSrvClick;
            menuItemStartSrv.Text = "开启服务";
            Forms.MenuItem menuItemStopSrv = new Forms.MenuItem();
            menuItemStopSrv.Click += this.MenuItemStopSrvClick;
            menuItemStopSrv.Text = "停止服务";
            menuItemStopSrv.Enabled = false;
            Forms.MenuItem menuItemShowMain = new Forms.MenuItem();
            menuItemShowMain.Click += this.TrayDoubleClickOrMenuItemShowMainClick;
            menuItemShowMain.Text = "显示主界面";
            Forms.MenuItem menuItemExitMain = new Forms.MenuItem();
            menuItemExitMain.Click += this.NotifyIconMenuItemExitMainClick;
            menuItemExitMain.Text = "退出";
            Forms.ContextMenu contextMenu = new Forms.ContextMenu(
                new Forms.MenuItem[]
                {
                    menuItemStartSrv,
                    menuItemStopSrv,
                    menuItemShowMain,
                    menuItemExitMain
                }
            );
            this.notifyIcon.Icon = new Icon("logo.ico");
            this.notifyIcon.Text = "ExpanderX";
            this.notifyIcon.ContextMenu = contextMenu;
            this.notifyIcon.DoubleClick += this.TrayDoubleClickOrMenuItemShowMainClick;
        }

        private void TrayDoubleClickOrMenuItemShowMainClick(object sender, EventArgs e)
        {
            if (!this.IsVisible)
                this.Show();
            if (this.WindowState == WindowState.Minimized)
                this.WindowState = WindowState.Normal;
            if (!this.IsActive)
                this.Activate();
        }

        private void ApplicationExitEvent()
        {
            this.reqHandler.Stop();
            this.StopService2();
            this.SaveAllRulesAsLocalFile();
            this.mutex?.Dispose();
            this.notifyIcon.Dispose();
        }

        private void CurrentSessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            this.ApplicationExitEvent();
        }

        private void NotifyIconMenuItemExitMainClick(object sender, EventArgs e)
        {
            this.ApplicationExitSender = 1;
            this.Close();
        }

        internal void CloseByReqHandler()
        {
            this.ApplicationExitSender = 1;
            this.Close();
        }

        private void MenuItemStopSrvClick(object sender, EventArgs e)
        {
            this.StopService2();
        }

        private void MenuItemStartSrvClick(object sender, EventArgs e)
        {
            this.StartService3();
        }

        private void LoadMainUIWindowSizeSettings()
        {
            Settings s = PubSettings.CurSettings;
            try
            {
                if (s.RemMainWinSize)
                {
                    this.Width = s.MainWinSize[0];
                    this.Height = s.MainWinSize[1];
                }
            }
            catch { }
        }

        /// <summary>
        /// 主窗口关闭事件触发此动作。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExpanderXMainClosing(object sender, CancelEventArgs e)
        {
            Settings s = PubSettings.CurSettings;
            if (s.RemMainWinSize)
                s.MainWinSize = new double[] { this.Width, this.Height };
            PubSettings.CurSettings = s;
            if (this.ApplicationExitSender == 0 && s.HideToTrayWhenClose)
            {
                this.Hide();
                e.Cancel = true;
                return;
            }
            this.ApplicationExitSender = 0;
            if (this.confedTaskModules.Count != 0)
            {
                switch (s.ExitWithUnsavedTaskModule)
                {
                    case 0:
                        int res = USER.MessageBox(
                            this.windowHelper.Handle,
                            "还有未合并成规则的任务模块，确定退出吗？",
                            "提示",
                            MB.MB_OKCANCEL | MB.MB_TOPMOST
                        );
                        if (res != 1)
                        {
                            e.Cancel = true;
                            return;
                        }
                        break;
                    case 1:
                        this.CombineTaskModulesAsCommonRule();
                        break;
                    case 2:
                        break;
                }
            }
            this.ApplicationExitEvent();
        }

        /// <summary>
        /// 检查许可协议是否已被接受。
        /// </summary>
        private void CheckLicenseAgreement()
        {
            Settings s = PubSettings.CurSettings;
            if (!s.LicenseAccepted)
            {
                // 启动新线程以让本主窗口初始化完成
                // 再显示 license 窗口，如此就可以将主窗口设置为拥有者
                Thread showLicenseThread = new Thread(this.LicenseChoice) { IsBackground = true };
                showLicenseThread.Start();
            }
            else
            {
                this.notifyIcon.Visible = true;
            }
        }

        /// <summary>
        /// 菜单栏“许可协议”菜单点击事件触发动作。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMenuLicenseClick(object sender, RoutedEventArgs e)
        {
            this.ShowLicenseAsDialog();
        }

        private void LicenseChoice()
        {
            Thread.Sleep(100);
            Application.Current.Dispatcher.Invoke(this.ShowLicenseAsDialog);
        }

        private void ShowLicenseAsDialog()
        {
            Window license = new License() { Owner = this };
            license.ShowDialog();
            this.notifyIcon.Visible = true;
        }

        /// <summary>
        /// 作为回调函数，给别的窗口调用以实现从别的窗口添加任务模块的功能。
        /// </summary>
        /// <param name="tm"></param>
        private void AddTaskModuleFromOther(AbsTaskModule tm)
        {
            this.confedTaskModules.Add(tm);
            this.uiListBox_ConfedTaskModules.Items.Add(tm.CustomName);
            if (tm.IsMatchEnable)
            {
                this.uiListBox_ConfedMatchers.Items.Add(tm.CustomName);
                this.confedMatchers.Add(this.confedTaskModules.Count - 1);
            }
            if (tm.IsExecuteEnable)
            {
                this.uiListBox_ConfedExecutors.Items.Add(tm.CustomName);
                this.confedExecutors.Add(this.confedTaskModules.Count - 1);
            }
        }

        /// <summary>
        /// 从当前非漫游用户应用程序储存库目录下的 ExpanderX/plugins 目录加载可用的任务模块。
        /// </summary>
        private void LoadPluginsFromLocalFile()
        {
            if (!this.pluginsFolder.Exists)
            {
                try
                {
                    this.pluginsFolder.Create();
                }
                catch (Exception)
                {
                    USER.MessageBox(
                        IntPtr.Zero,
                        $"无法创建目录<{this.pluginsFolder.FullName}>。\n",
                        "错误",
                        MB.MB_TOPMOST | MB.MB_ICONERROR
                    );
                    return;
                }
            }
            FileInfo[] pluginsInfo = this.pluginsFolder.GetFiles();
            Type typeIGetWU = typeof(IGetTaskModule);
            Type typeUserCtrl = typeof(UserControl);
            foreach (FileInfo fp in pluginsInfo)
            {
                Assembly a = Assembly.LoadFrom(fp.FullName);
                IEnumerable<Type> userControlTypes = a.GetExportedTypes()
                    .Where(t => t.BaseType == typeUserCtrl);
                IEnumerable<Type> workUnitTypes = userControlTypes.Where(
                    t => t.GetInterfaces().Contains(typeIGetWU)
                );
                foreach (Type t in workUnitTypes)
                {
                    if (Activator.CreateInstance(t) is UserControl u)
                        this.ifaceTaskModules.Add(u);
                }
            }
        }

        /// <summary>
        /// 从本地二进制文件加载已保存的规则，主窗口启动时触发。
        /// </summary>
        /// <returns></returns>
        private void LoadRulesFromLocalFile()
        {
            if (!this.rulesFolder.Exists)
            {
                try
                {
                    this.rulesFolder.Create();
                }
                catch (Exception e)
                {
                    USER.MessageBox(
                        IntPtr.Zero,
                        $"无法创建目录<{this.rulesFolder.FullName}>\n原因：{e.Message}",
                        "错误",
                        MB.MB_TOPMOST | MB.MB_ICONERROR
                    );
                }
                return;
            }
            FileInfo[] rulefiles = this.rulesFolder.GetFiles();
            if (rulefiles.Length == 0)
                return;
            foreach (FileInfo fi in rulefiles)
            {
                try
                {
                    using (FileStream fs = File.OpenRead(fi.FullName))
                    {
                        if (this.formatter.Deserialize(fs) is AbsRuleModel rl)
                        {
                            this.uiListBox_ConfedRulesDisplay.Items.Add(new ListBoxItemCheck(rl));
                            this.confedRuleModels.Add(rl);
                        }
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }
            return;
        }

        /// <summary>
        /// 将已配置的匹配器和执行器合并为规则。
        /// </summary>
        /// <param name="remark">规则备注名</param>
        private void CombineTaskModulesAsCommonRule(string remark = null)
        {
            if (this.confedMatchers.Count == 0 || this.confedExecutors.Count == 0)
                return;
            if (remark == null)
                remark = "自动保存的规则";
            if (string.IsNullOrEmpty(remark))
            {
                this.ShowMessageBox("规则备注名不能为空，请先填写规则备注名。", "提示");
                return;
            }
            CommonRule rl = new CommonRule
            {
                CustomName = remark,
                Executors = this.confedExecutors.ToArray(),
                Matchers = this.confedMatchers.ToArray(),
                TaskModules = this.confedTaskModules.ToArray(),
            };
            this.confedRuleModels.Add(rl);
            this.uiListBox_ConfedRulesDisplay.Items.Add(new ListBoxItemCheck(rl));
            this.confedTaskModules.Clear();
            this.confedMatchers.Clear();
            this.confedExecutors.Clear();
            this.uiListBox_ConfedMatchers.Items.Clear();
            this.uiListBox_ConfedExecutors.Items.Clear();
            this.uiListBox_ConfedTaskModules.Items.Clear();
        }

        /// <summary>
        /// 在主界面添加了已配置的“匹配器”和“执行器”后，点击“合成规则”按钮触发的动作。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnButtonCombineTaskModulesAsRuleClick(object sender, RoutedEventArgs e)
        {
            if (this.confedMatchers.Count < 1 || this.confedExecutors.Count < 1)
            {
                this.ShowMessageBox("已配置的匹配器或执行器为空，请添加任务模块。", "提示");
                return;
            }
            new LineInputBox()
            {
                Owner = this,
                TextOut = this.CombineTaskModulesAsCommonRule,
                Title = "合并任务模块",
                Description = "请输入规则备注名：",
            }.ShowDialog();
        }

        /// <summary>
        /// 主界面“规则添加及删除”标签页的任务模块添加按钮的点击事件触发的动作。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnButtonAddTaskModuleClick(object sender, RoutedEventArgs e)
        {
            AddTaskModuleUI addTaskModuleUI = new AddTaskModuleUI
            {
                CustomControls = this.ifaceTaskModules,
                AddTaskModuleToMainUI = this.AddTaskModuleFromOther,
                Owner = this
            };
            addTaskModuleUI.ShowDialog();
        }

        /// <summary>
        /// 点击“规则添加及删除”标签页下的“删除选中规则”按钮触发的动作。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnListBoxRulesDisplayContextMenuDeleteClick(object sender, RoutedEventArgs e)
        {
            int index = this.uiListBox_ConfedRulesDisplay.SelectedIndex;
            if (index == -1)
                return;
            int res = USER.MessageBox(
                this.windowHelper.Handle,
                "确定删除此规则吗？",
                "提示",
                MB.MB_OKCANCEL | MB.MB_TOPMOST
            );
            if (res != 1)
                return;
            this.confedRuleModels.RemoveAt(index);
            this.uiListBox_ConfedRulesDisplay.Items.RemoveAt(index);
            if (index == this.confedRuleModels.Count)
                --index;
            this.uiListBox_ConfedRulesDisplay.SelectedIndex = index;
            this.uiListBox_ConfedMatchersDisplay.Items.Clear();
            this.uiListBox_ConfedExecutorsDisplay.Items.Clear();
            this.uiTextBlock_DisplayRuleDetExeDescription.Text = "";
        }

        private void UpdateUiListBoxMatchersAndExecutors()
        {
            this.uiListBox_ConfedMatchers.Items.Clear();
            this.uiListBox_ConfedExecutors.Items.Clear();
            foreach (int i in this.confedMatchers)
                this.uiListBox_ConfedMatchers.Items.Add(this.confedTaskModules[i].CustomName);
            foreach (int i in this.confedExecutors)
                this.uiListBox_ConfedExecutors.Items.Add(this.confedTaskModules[i].CustomName);
        }

        /// <summary>
        /// 主界面“规则添加及删除”标签页的“以任务模块分类”下的“删除”按钮按下事件触发的动作。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnButtonDelConfedTaskModuleClick(object sender, RoutedEventArgs e)
        {
            int index = this.uiListBox_ConfedTaskModules.SelectedIndex;
            if (index == -1)
                return;
            this.uiListBox_ConfedTaskModules.Items.RemoveAt(index);
            this.confedTaskModules.RemoveAt(index);
            this.confedMatchers.RemoveAll(x => x == index);
            for (int i = 0; i < this.confedMatchers.Count; ++i)
                if (this.confedMatchers[i] > index)
                    --this.confedMatchers[i];
            this.confedExecutors.RemoveAll(x => x == index);
            for (int i = 0; i < this.confedExecutors.Count; ++i)
                if (this.confedExecutors[i] > index)
                    --this.confedExecutors[i];
            this.UpdateUiListBoxMatchersAndExecutors();
            if (index >= this.uiListBox_ConfedTaskModules.Items.Count)
                index = this.uiListBox_ConfedTaskModules.Items.Count - 1;
            this.uiListBox_ConfedTaskModules.SelectedIndex = index;
        }

        /// <summary>
        /// 主界面“规则添加及删除”标签页的“匹配器”或“执行器”子页下的“下移”按钮按下事件触发的动作。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnButtonDownMatchersOrExecutorsClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int index,
                swapTempInt;
            object swapTempClt;
            if (button == this.uiButton_ConfedMatcherDown)
            {
                index = this.uiListBox_ConfedMatchers.SelectedIndex;
                if (index == -1 || index == this.uiListBox_ConfedMatchers.Items.Count - 1)
                    return;
                swapTempInt = this.confedMatchers[index];
                this.confedMatchers[index] = this.confedMatchers[index + 1];
                this.confedMatchers[index + 1] = swapTempInt;
                swapTempClt = this.uiListBox_ConfedMatchers.Items[index];
                this.uiListBox_ConfedMatchers.Items[index] = this.uiListBox_ConfedMatchers.Items[
                    index + 1
                ];
                this.uiListBox_ConfedMatchers.Items[index + 1] = swapTempClt;
                this.uiListBox_ConfedMatchers.SelectedIndex = index + 1;
            }
            else if (button == this.uiButton_ConfedExecutorDown)
            {
                index = this.uiListBox_ConfedExecutors.SelectedIndex;
                if (index == -1 || index == this.uiListBox_ConfedExecutors.Items.Count - 1)
                    return;
                swapTempInt = this.confedExecutors[index];
                this.confedExecutors[index] = this.confedExecutors[index + 1];
                this.confedExecutors[index + 1] = swapTempInt;
                swapTempClt = this.uiListBox_ConfedExecutors.Items[index];
                this.uiListBox_ConfedExecutors.Items[index] = this.uiListBox_ConfedExecutors.Items[
                    index + 1
                ];
                this.uiListBox_ConfedExecutors.Items[index + 1] = swapTempClt;
                this.uiListBox_ConfedExecutors.SelectedIndex = index + 1;
            }
        }

        /// <summary>
        /// 主界面“规则添加及删除”标签页的“匹配器配置”或“执行器配置”子页下的“上移”按钮按下事件触发的动作。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnButtonUpMatchersOrExecutorsClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int index,
                swapTempInt;
            object swapTempClt;
            if (button == this.uiButton_ConfedMatcherUp)
            {
                index = this.uiListBox_ConfedMatchers.SelectedIndex;
                if (index == -1 || index == 0)
                    return;
                swapTempInt = this.confedMatchers[index - 1];
                this.confedMatchers[index - 1] = this.confedMatchers[index];
                this.confedMatchers[index] = swapTempInt;
                swapTempClt = this.uiListBox_ConfedMatchers.Items[index - 1];
                this.uiListBox_ConfedMatchers.Items[index - 1] =
                    this.uiListBox_ConfedMatchers.Items[index];
                this.uiListBox_ConfedMatchers.Items[index] = swapTempClt;
                this.uiListBox_ConfedMatchers.SelectedIndex = index - 1;
            }
            else if (button == this.uiButton_ConfedExecutorUp)
            {
                index = this.uiListBox_ConfedExecutors.SelectedIndex;
                if (index == -1 || index == 0)
                    return;
                swapTempInt = this.confedExecutors[index - 1];
                this.confedExecutors[index - 1] = this.confedExecutors[index];
                this.confedExecutors[index] = swapTempInt;
                swapTempClt = this.uiListBox_ConfedExecutors.Items[index - 1];
                this.uiListBox_ConfedExecutors.Items[index - 1] =
                    this.uiListBox_ConfedExecutors.Items[index];
                this.uiListBox_ConfedExecutors.Items[index] = swapTempClt;
                this.uiListBox_ConfedExecutors.SelectedIndex = index - 1;
            }
        }

        /// <summary>
        /// 主界面左下角的“赞赏及源代码”后的两个超链接的点击触发的动作。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHyperlinkGoToClick(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            try
            {
                Process.Start(new ProcessStartInfo(link.NavigateUri.AbsoluteUri));
            }
            catch (Exception)
            {
                this.ShowMessageBox("打不开链接，伤心 o(╥﹏╥)o ~", "伤心");
                return;
            }
        }

        /// <summary>
        /// 主界面“帮助”菜单中的“关于”菜单点击事件触发的动作。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMenuAboutClick(object sender, RoutedEventArgs e)
        {
            Window about = new AboutUI() { Owner = this, Version = Information.VER };
            about.ShowDialog();
        }

        /// <summary>
        /// 主界面“设置”菜单的点击事件触发的动作。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMenuSettingsClick(object sender, RoutedEventArgs e)
        {
            Window SettingsWindow = new SettingsUI { Owner = this };
            SettingsWindow.ShowDialog();
        }

        /// <summary>
        /// 主界面“规则编辑及描述”标签页三个列表的鼠标键抬起时触发的动作，用于更新该标签页下的“规则、任务模块详情描述”文本框。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUIListBoxDisplayMouseUp(object sender, MouseButtonEventArgs e)
        {
            string description;
            ListBox listbox = (ListBox)sender;
            int index = listbox.SelectedIndex;
            if (index == -1)
                return;
            int ruleIndex = this.uiListBox_ConfedRulesDisplay.SelectedIndex;
            if (ruleIndex == -1)
                return;
            this.uiTextBlock_DisplayRuleDetExeDescription.Text = "";
            if (listbox == this.uiListBox_ConfedRulesDisplay)
            {
                try
                {
                    description = this.confedRuleModels[index].Description();
                }
                catch (Exception)
                {
                    this.ShowMessageBox("任务模块异常：无法获取规则实例描述。", "错误");
                    return;
                }
                this.uiListBox_ConfedMatchersDisplay.Items.Clear();
                foreach (int i in this.confedRuleModels[index].Matchers)
                    this.uiListBox_ConfedMatchersDisplay.Items.Add(
                        this.confedRuleModels[index].TaskModules[i].CustomName
                    );
                this.uiListBox_ConfedExecutorsDisplay.Items.Clear();
                foreach (int i in this.confedRuleModels[index].Executors)
                    this.uiListBox_ConfedExecutorsDisplay.Items.Add(
                        this.confedRuleModels[index].TaskModules[i].CustomName
                    );
                this.uiTextBlock_DisplayRuleDetExeDescription.Text = description;
            }
            else if (listbox == this.uiListBox_ConfedMatchersDisplay)
            {
                try
                {
                    description = this.confedRuleModels[ruleIndex].TaskModules[
                        this.confedRuleModels[ruleIndex].Matchers[index]
                    ].MatcherDetails();
                }
                catch (Exception)
                {
                    this.ShowMessageBox("任务模块异常：无法获取匹配器实例描述。", "错误");
                    return;
                }
                this.uiTextBlock_DisplayRuleDetExeDescription.Text = description;
            }
            else if (listbox == this.uiListBox_ConfedExecutorsDisplay)
            {
                try
                {
                    description = this.confedRuleModels[ruleIndex].TaskModules[
                        this.confedRuleModels[ruleIndex].Executors[index]
                    ].ExecutorDetails();
                }
                catch (Exception)
                {
                    this.ShowMessageBox("任务模块异常：无法获取执行器实例描述。", "错误");
                    return;
                }
                this.uiTextBlock_DisplayRuleDetExeDescription.Text = description;
            }
        }

        /// <summary>
        /// 将所有的规则保存到本地指定目录。
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        private void SaveAllRulesAsLocalFile()
        {
            if (!this.rulesFolder.Exists)
            {
                try
                {
                    this.rulesFolder.Create();
                }
                catch (Exception e)
                {
                    USER.MessageBox(
                        IntPtr.Zero,
                        $"无法创建目录<{this.rulesFolder.FullName}>\n原因：{e.Message}",
                        "错误",
                        MB.MB_TOPMOST | MB.MB_ICONERROR
                    );
                    return;
                }
            }
            SHA1 sha1 = SHA1.Create();
            List<string> saved = this.rulesFolder.GetFiles().Select(x => x.Name).ToList();
            foreach (AbsRuleModel rule in this.confedRuleModels)
            {
                using (Stream ms = new MemoryStream())
                {
                    this.formatter.Serialize(ms, rule);
                    ms.Seek(0, SeekOrigin.Begin);
                    byte[] sha1bys = sha1.ComputeHash(ms);
                    string newName = $"{BitConverter.ToString(sha1bys).Replace("-", "")}.rl";
                    if (saved.Contains(newName))
                    {
                        saved.Remove(newName);
                        continue;
                    }
                    string fullPath = Path.Combine(this.rulesFolder.FullName, newName);
                    // 已配置的规则中完全一样的规则不重复保存
                    if (File.Exists(fullPath))
                        continue;
                    try
                    {
                        using (FileStream fs = File.Create(fullPath))
                        {
                            this.formatter.Serialize(fs, rule);
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
            sha1.Dispose();
            foreach (string fName in saved)
            {
                try
                {
                    File.Delete(Path.Combine(this.rulesFolder.FullName, fName));
                }
                catch (Exception) { }
            }
        }

        /// <summary>
        /// 将指定已保存的规则从本地目录中删除。
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        private bool DelRuleFromLocalFile(AbsRuleModel rule)
        {
            if (!this.rulesFolder.Exists)
            {
                USER.MessageBox(
                    IntPtr.Zero,
                    $"不存在规则目录<{this.rulesFolder.FullName}>",
                    "错误",
                    MB.MB_TOPMOST | MB.MB_ICONERROR
                );
                return false;
            }
            using (Stream ms = new MemoryStream())
            {
                this.formatter.Serialize(ms, rule);
                ms.Seek(0, SeekOrigin.Begin);
                SHA1 sha1 = SHA1.Create();
                byte[] sha1bys = sha1.ComputeHash(ms);
                sha1.Dispose();
                string newName = $"{BitConverter.ToString(sha1bys).Replace("-", "")}.rl";
                string fullPath = Path.Combine(this.rulesFolder.FullName, newName);
                if (!File.Exists(fullPath))
                    return true;
                try
                {
                    File.Delete(fullPath);
                }
                catch (Exception e)
                {
                    USER.MessageBox(
                        IntPtr.Zero,
                        $"无法删除规则<{fullPath}>\n原因：{e.Message}",
                        "错误",
                        MB.MB_TOPMOST | MB.MB_ICONERROR
                    );
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 调用 Win32 API 来显示总在顶层的提示窗口。
        /// </summary>
        /// <param name="lpText">窗口内容</param>
        /// <param name="lpCaption">窗口标题</param>
        private void ShowMessageBox(string lpText, string lpCaption)
        {
            USER.MessageBox(this.windowHelper.Handle, lpText, lpCaption, MB.MB_TOPMOST);
        }

        /// <summary>
        /// 点击主界面“开始运行”按钮时触发的动作，开始服务循环。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartService(object sender, RoutedEventArgs e)
        {
            this.StartService3();
        }

        /// <summary>
        /// 开始服务循环, 给‘请求处理器’调用准备的方法。
        /// </summary>
        internal void StartService2()
        {
            AbsRuleModel[] rules = this.confedRuleModels.Where(x => x.IsEnable).ToArray();
            if (rules.Length == 0)
                return;
            if (Cycle != null && Cycle.State() != STATE.Stopped)
                _ = Cycle.Stop();
            Cycle.Start(rules);
            this.ServiceStartStopEvent?.Invoke(Cmd.Start);
        }

        /// <summary>
        /// 开始服务循环，给当前线程主动调用准备的方法。
        /// </summary>
        private void StartService3()
        {
            if (this.confedRuleModels.Count == 0)
            {
                this.ShowMessageBox("没有可用的自定义规则。", "提示");
                return;
            }
            AbsRuleModel[] rules = this.confedRuleModels.Where(x => x.IsEnable).ToArray();
            if (rules.Length == 0)
            {
                this.ShowMessageBox("没有已启用的规则。", "提示");
                return;
            }
            if (PubSettings.CurSettings.CheckIfClientReady)
            {
                if (!PubDTools.DingTalkAllReady && PubDTools.InitalizeWait())
                {
                    if (!PubDTools.DingTalkAllReady)
                    {
                        this.ShowMessageBox("钉钉未准备就绪，请检查是否已运行并打开消息框。", "提示");
                        return;
                    }
                }
            }
            if (Cycle != null && Cycle.State() != STATE.Stopped)
                _ = Cycle.Stop();
            Cycle.Start(rules);
            this.ServiceStartStopEvent?.Invoke(Cmd.Start);
        }

        /// <summary>
        /// 点击主界面“停止运行”按钮时触发的动作，停止服务循环。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopService(object sender, RoutedEventArgs e)
        {
            this.StopService2();
        }

        /// <summary>
        /// 停止服务循环，给当前线程主动调用和‘请求处理器’调用写的方法。
        /// </summary>
        internal void StopService2()
        {
            if (Cycle == null)
                return;
            _ = Cycle.Stop();
            this.ServiceStartStopEvent?.Invoke(Cmd.Stop);
        }

        private void OnListBoxConfedTaskModulesSelectionChanged(
            object sender,
            SelectionChangedEventArgs e
        )
        {
            int index = this.uiListBox_ConfedTaskModules.SelectedIndex;
            if (index == -1)
            {
                this.uiCheckBox_SigEnable.IsEnabled = false;
                this.uiCheckBox_SigEnable.IsChecked = false;
                this.uiCheckBox_ExcEnable.IsEnabled = false;
                this.uiCheckBox_ExcEnable.IsChecked = false;
                return;
            }
            switch (this.confedTaskModules[index].TaskType)
            {
                case 2:
                    this.uiCheckBox_SigEnable.IsEnabled = true;
                    this.uiCheckBox_ExcEnable.IsEnabled = true;
                    break;
                case 0:
                    this.uiCheckBox_SigEnable.IsEnabled = true;
                    this.uiCheckBox_ExcEnable.IsEnabled = false;
                    break;
                case 1:
                    this.uiCheckBox_SigEnable.IsEnabled = false;
                    this.uiCheckBox_ExcEnable.IsEnabled = true;
                    break;
            }
            if (this.confedTaskModules[index].IsMatchEnable)
            {
                this.uiCheckBox_SigEnable.IsChecked = true;
                for (int i = 0; i < this.confedMatchers.Count; ++i)
                {
                    if (this.confedMatchers[i] == index)
                    {
                        this.uiListBox_ConfedMatchers.SelectedIndex = i;
                        break;
                    }
                }
            }
            else
            {
                this.uiCheckBox_SigEnable.IsChecked = false;
                this.uiListBox_ConfedMatchers.SelectedIndex = -1;
            }
            if (this.confedTaskModules[index].IsExecuteEnable)
            {
                this.uiCheckBox_ExcEnable.IsChecked = true;
                for (int i = 0; i < this.confedExecutors.Count; ++i)
                {
                    if (this.confedExecutors[i] == index)
                    {
                        this.uiListBox_ConfedExecutors.SelectedIndex = i;
                        break;
                    }
                }
            }
            else
            {
                this.uiCheckBox_ExcEnable.IsChecked = false;
                this.uiListBox_ConfedExecutors.SelectedIndex = -1;
            }
        }

        private void OnUiCheckBoxSigEnableOrExcEnableClick(object sender, RoutedEventArgs e)
        {
            int index = this.uiListBox_ConfedTaskModules.SelectedIndex;
            if (index == -1)
                return;
            CheckBox checkbox = sender as CheckBox;
            if (this.confedTaskModules[index].TaskType != 2)
            {
                checkbox.IsChecked = true;
                this.ShowMessageBox("当前任务模块仅支持单功能，无法取消。", "提示");
                return;
            }
            if (checkbox == this.uiCheckBox_SigEnable)
            {
                if (checkbox.IsChecked == true)
                {
                    this.confedTaskModules[index].IsMatchEnable = true;
                    this.confedMatchers.Add(index);
                    this.uiListBox_ConfedMatchers.Items.Add(
                        this.confedTaskModules[index].CustomName
                    );
                }
                else if (checkbox.IsChecked == false)
                {
                    if (this.confedTaskModules[index].IsExecuteEnable == false)
                    {
                        checkbox.IsChecked = true;
                        this.ShowMessageBox("不能同时取消匹配器功能和执行器功能。", "提示");
                        return;
                    }
                    this.confedMatchers.RemoveAll(x => x == index);
                    this.UpdateUiListBoxMatchersAndExecutors();
                    this.confedTaskModules[index].IsMatchEnable = false;
                }
            }
            else if (checkbox == this.uiCheckBox_ExcEnable)
            {
                if (checkbox.IsChecked == true)
                {
                    this.confedTaskModules[index].IsExecuteEnable = true;
                    this.confedExecutors.Add(index);
                    this.uiListBox_ConfedExecutors.Items.Add(
                        this.confedTaskModules[index].CustomName
                    );
                }
                else if (checkbox.IsChecked == false)
                {
                    if (this.confedTaskModules[index].IsMatchEnable == false)
                    {
                        checkbox.IsChecked = true;
                        this.ShowMessageBox("不能同时取消匹配器功能和执行器功能。", "提示");
                        return;
                    }
                    this.confedExecutors.RemoveAll(x => x == index);
                    this.UpdateUiListBoxMatchersAndExecutors();
                    this.confedTaskModules[index].IsExecuteEnable = false;
                }
            }
        }

        private void OnUiListBoxMatchersAndExecutorsMouseUp(object sender, MouseButtonEventArgs e)
        {
            ListBox listbox = sender as ListBox;
            if (listbox.SelectedIndex == -1)
                return;
            int index = listbox.SelectedIndex;
            if (listbox == this.uiListBox_ConfedMatchers)
                this.uiListBox_ConfedTaskModules.SelectedIndex = this.confedMatchers[index];
            else if (listbox == this.uiListBox_ConfedExecutors)
                this.uiListBox_ConfedTaskModules.SelectedIndex = this.confedExecutors[index];
        }

        private void RenameSelectedRule(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                this.ShowMessageBox("规则名称不能为空！", "提示");
                return;
            }
            int index = this.uiListBox_ConfedRulesDisplay.SelectedIndex;
            if (index == -1)
                return;
            this.confedRuleModels[index].CustomName = name;
            this.uiListBox_ConfedRulesDisplay.Items[index] = new ListBoxItemCheck(
                this.confedRuleModels[index]
            );
        }

        private void OnListBoxRulesDisplayContextMenuRenameClick(object sender, RoutedEventArgs e)
        {
            if (this.uiListBox_ConfedRulesDisplay.SelectedItem is ListBoxItemCheck checkableItem)
            {
                new LineInputBox()
                {
                    Owner = this,
                    Description = "请输入备注名：",
                    Title = "重命名",
                    TextOut = RenameSelectedRule,
                    PresetText = checkableItem.Rule.CustomName,
                }.ShowDialog();
            }
        }

        private void OnListBoxRulesDisplayContextMenuEnableClick(object sender, RoutedEventArgs e)
        {
            if (this.uiListBox_ConfedRulesDisplay.SelectedItem is ListBoxItemCheck checkableItem)
                checkableItem.ChangeCheckedState(true);
        }

        private void OnListBoxRulesDisplayContextMenuDisableClick(object sender, RoutedEventArgs e)
        {
            if (this.uiListBox_ConfedRulesDisplay.SelectedItem is ListBoxItemCheck checkableItem)
                checkableItem.ChangeCheckedState(false);
        }

        private void OnCheckBoxPreventGoingToSleep(object sender, RoutedEventArgs e)
        {
            if (this.uiCheckBox_PreventSleeping.IsChecked ?? false)
                this.PreventSystemGoingToSleep(true);
            else
                this.PreventSystemGoingToSleep(false);
        }

        private void PreventSystemGoingToSleep(bool state)
        {
            uint previous;
            if (state)
            {
                previous = KERNEL.SetThreadExecutionState(ES.ES_CONTINUOUS);
                KERNEL.SetThreadExecutionState(previous | ES.ES_SYSTEM_REQUIRED | ES.ES_CONTINUOUS);
            }
            else
            {
                previous = KERNEL.SetThreadExecutionState(ES.ES_CONTINUOUS);
                KERNEL.SetThreadExecutionState(previous & ~ES.ES_SYSTEM_REQUIRED);
            }
        }
    }
}
