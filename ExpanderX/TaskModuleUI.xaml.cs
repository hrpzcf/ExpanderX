using CommonUtils;
using ExpanderXSDK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace ExpanderX
{
    /// <summary>
    /// AddTaskModuleUI.xaml 的交互逻辑。
    /// </summary>
    public partial class AddTaskModuleUI : Window
    {
        private readonly WindowInteropHelper helper;
        public AddTaskModuleUI()
        {
            this.InitializeComponent();
            this.helper = new WindowInteropHelper(this);
            this.Closing += this.OnWindowClosing;
            this.LoadUISettings();
        }

        public Action<AbsTaskModule> AddTaskModuleToMainUI { get; set; }

        public List<UserControl> CustomControls { get; set; }

        private void LoadUISettings()
        {
            Settings s = PubSets.CurSettings;
            try
            {
                if (s.RemAddTaskWinSize)
                {
                    this.Width = s.AddTaskWinSize[0];
                    this.Height = s.AddTaskWinSize[1];
                }
            }
            catch { }
        }

        public new bool? ShowDialog()
        {
            this.AddTaskModuleNameToListBox();
            return base.ShowDialog();
        }

        private void AddTaskModuleNameToListBox()
        {
            if (this.CustomControls == null)
                return;
            foreach (IGetTaskModule igt in this.CustomControls)
            {
                try
                {
                    AbsTaskModule tm = igt.GetTaskModule();
                    if (tm == null) continue;
                    this.uiListBox_TaskModules.Items.Add(new ListBoxItemImage(tm));
                }
                catch (Exception) { }
            }
            if (this.CustomControls.Count > 0)
                this.uiListBox_TaskModules.SelectedIndex = 0;
        }

        private void OnButtonCancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnButtonConfirmClick(object sender, RoutedEventArgs e)
        {
            AbsTaskModule tm;
            int index = this.uiListBox_TaskModules.SelectedIndex;
            if (index == -1)
            {
                this.ShowMessageBox("还没有选中任何任务模块。", "警告", MB.MB_ICONWARNING);
                return;
            }
            string remark = this.uiTextBox_TaskModuleRemark.Text;
            if (string.IsNullOrEmpty(remark))
            {
                this.ShowMessageBox("没有填写配置备注名，请先填写备注名。", "提示");
                return;
            }
            if (!this.uiCheckBox_AsMatcher.IsChecked.GetValueOrDefault()
                && !this.uiCheckBox_AsExecutor.IsChecked.GetValueOrDefault())
            {
                this.ShowMessageBox("任务模块匹配功能和执行功能至少选择其一。", "提示");
                return;
            }
            if (this.AddTaskModuleToMainUI == null)
            {
                this.ShowMessageBox("没有设置添加配置的委托函数。", "错误", MB.MB_ICONERROR);
                return;
            }
            if (!(this.CustomControls[index] is IGetTaskModule igt))
            {
                this.ShowMessageBox("任务模块异常：无法获取通用接口。", "错误", MB.MB_ICONERROR);
                return;
            }
            try
            {
                tm = igt.GetTaskModule();
                if (tm == null)
                {
                    this.ShowMessageBox($"任务模块异常：无法获取任务模块实例。", "警告", MB.MB_ICONWARNING);
                    return;
                }
                if (!igt.Commit())
                {
                    this.ShowMessageBox($"配置提交操作被<{tm.Name}>拒绝，请检查配置。", "警告", MB.MB_ICONWARNING);
                    return;
                }
            }
            catch (Exception)
            {
                this.ShowMessageBox("任务模块异常：无法获取或提交任务模块实例。", "错误", MB.MB_ICONERROR);
                return;
            }
            tm = Serialization.CpInstance(tm);
            if (tm == null)
                return;
            tm.CustomName = tm.Name + "：" + remark;
            tm.IsMatchEnable = this.uiCheckBox_AsMatcher.IsChecked ?? false;
            tm.IsExecuteEnable = this.uiCheckBox_AsExecutor.IsChecked ?? false;
            this.AddTaskModuleToMainUI.Invoke(tm);
            this.Close();
        }

        private void OnListBoxTaskModulesSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = this.uiListBox_TaskModules.SelectedIndex;
            if (index == -1)
                index = 0;
            this.uiGrid_TaskModuleInterface.Children.Clear();
            this.uiCheckBox_AsMatcher.IsChecked = false;
            this.uiCheckBox_AsExecutor.IsChecked = false;
            if (!(this.CustomControls[index] is IGetTaskModule igt))
            {
                this.ShowMessageBox("任务模块错误：无法获取通用接口。", "错误", MB.MB_ICONERROR);
                return;
            }
            try
            {
                if (!(igt.GetTaskModule() is AbsTaskModule tm))
                {
                    this.ShowMessageBox("获取任务模块实例结果为空。", "错误", MB.MB_ICONERROR);
                    return;
                }
                switch (tm.TaskType)
                {
                case 0:
                    this.uiCheckBox_AsMatcher.IsEnabled = true;
                    this.uiCheckBox_AsExecutor.IsEnabled = false;
                    this.uiCheckBox_AsMatcher.IsChecked = true;
                    this.uiTextBox_TaskModuleSkills.Text = "仅支持匹配功能";
                    break;
                case 1:
                    this.uiCheckBox_AsMatcher.IsEnabled = false;
                    this.uiCheckBox_AsExecutor.IsEnabled = true;
                    this.uiCheckBox_AsExecutor.IsChecked = true;
                    this.uiTextBox_TaskModuleSkills.Text = "仅支持执行功能";
                    break;
                case 2:
                    this.uiCheckBox_AsMatcher.IsEnabled = true;
                    this.uiCheckBox_AsExecutor.IsEnabled = true;
                    this.uiCheckBox_AsMatcher.IsChecked = false;
                    this.uiCheckBox_AsExecutor.IsChecked = false;
                    this.uiTextBox_TaskModuleSkills.Text = "匹配和执行功能";
                    break;
                default:
                    break;
                }
            }
            catch
            {
                this.ShowMessageBox("获取任务模块出错。", "错误", MB.MB_ICONERROR);
                return;
            }
            this.uiGrid_TaskModuleInterface.Children.Add(this.CustomControls[index]);
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            Settings s = PubSets.CurSettings;
            if (s.RemAddTaskWinSize)
                s.AddTaskWinSize = new double[] { this.Width, this.Height };
            PubSets.CurSettings = s;
            this.uiGrid_TaskModuleInterface.Children.Clear();
        }

        private void ShowMessageBox(string text, string cap, uint mb = 0)
        {
            USER.MessageBox(this.helper.Handle, text, cap, mb | MB.MB_TOPMOST);
        }
    }
}
