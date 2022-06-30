using ExpanderXSDK;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ExpanderX
{
    internal struct PreSetCmd
    {
        public string DefFile;
        public string[] DefArgs;
        public string FileName;
        public string[] Arguments;
    }

    /// <summary>
    /// UserCtrlCmdExecutor.xaml 的交互逻辑。
    /// </summary>
    public partial class UserCtrlCmdExecutor : UserControl, IGetTaskModule
    {
        private PreSetCmd CmdShutdown;
        private PreSetCmd CmdKillDTalk;

        public UserCtrlCmdExecutor()
        {
            this.InitializeComponent();
            this.InitPreSetCmds();
            this.MakeFinalCmdChanged();
            this.uiListBox_PresetCmd.SelectionChanged += this.PresetCmdSelectionChanged;
            this.uiComboBox_ShutdownTime.SelectionChanged += this.OnComboShutTimeSelectionChanged;
            this.uiCheckBox_ForceShutdownCmd.Click += this.OnCheckBoxForceShutdownClick;
            this.uiTextBox_ShutdownDelay.TextChanged += this.OnTextBoxShutDelayTextChanged;
            this.uiTextBox_ImageName.TextChanged += this.OnTextBoxImageNameTextChanged;
            this.uiCheckBox_ForceKill.Click += this.OnCheckBoxForceKillClick;
            this.uiCheckBox_KillChildProcess.Click += this.OnCheckBoxKillChildProcessClick;
        }

        private void InitPreSetCmds()
        {
            this.CmdShutdown = new PreSetCmd()
            {
                DefFile = "shutdown",
                DefArgs = new string[] { "/s", "/f", "/t", "30", },
                FileName = "shutdown",
                Arguments = new string[] { "/s", "/f", "/t", "30", },
            };
            this.CmdKillDTalk = new PreSetCmd()
            {
                DefFile = "taskkill",
                DefArgs = new string[] { "/f", "/t", "/im", "DingTalk.exe" },
                FileName = "taskkill",
                Arguments = new string[] { "/f", "/t", "/im", "DingTalk.exe" },
            };
        }

        public bool Commit()
        {
            return (
                    (this.uiRadioButton_UseShell.IsChecked ?? false)
                    || (this.uiRadioButton_NotShell.IsChecked ?? false)
                ) && !string.IsNullOrEmpty(this.uiTextBox_FinalArgs.Text);
        }

        public AbsTaskModule GetTaskModule()
        {
            return new CommandExecutor(
                this.uiTextBox_FinalTarget.Text,
                this.uiTextBox_FinalArgs.Text,
                this.uiTextBox_WorkingDir.Text,
                this.uiRadioButton_UseShell.IsChecked ?? false
            );
        }

        private void MakeFinalCmdChanged()
        {
            int index = this.uiListBox_PresetCmd.SelectedIndex;
            if (index == -1)
                return;
            switch (index)
            {
                case 0:
                    this.uiGrid_Shutdown.Visibility = Visibility.Visible;
                    this.uiGrid_KillDingTalk.Visibility = Visibility.Hidden;
                    this.CmdShutdown.Arguments[3] =
                        this.uiComboBox_ShutdownTime.SelectedIndex == 0
                        || this.uiTextBox_ShutdownDelay.Text == ""
                            ? "0"
                            : this.uiTextBox_ShutdownDelay.Text;
                    this.CmdShutdown.Arguments[1] =
                        this.uiCheckBox_ForceShutdownCmd.IsChecked == true ? "/f" : "";
                    this.uiTextBox_FinalTarget.Text = this.CmdShutdown.FileName;
                    this.uiTextBox_FinalArgs.Text = string.Join(
                        " ",
                        this.CmdShutdown.Arguments.Where(x => x != "")
                    );
                    this.uiRadioButton_UseShell.IsChecked = true;
                    break;
                case 1:
                    this.uiGrid_Shutdown.Visibility = Visibility.Hidden;
                    this.uiGrid_KillDingTalk.Visibility = Visibility.Visible;
                    this.CmdKillDTalk.Arguments[3] =
                        this.uiTextBox_ImageName.Text == ""
                            ? "DingTalk.exe"
                            : this.uiTextBox_ImageName.Text;
                    this.CmdKillDTalk.Arguments[0] =
                        this.uiCheckBox_ForceKill.IsChecked == true ? "/f" : "";
                    this.CmdKillDTalk.Arguments[1] =
                        this.uiCheckBox_KillChildProcess.IsChecked == true ? "/t" : "";
                    this.uiTextBox_FinalTarget.Text = this.CmdKillDTalk.FileName;
                    this.uiTextBox_FinalArgs.Text = string.Join(
                        " ",
                        this.CmdKillDTalk.Arguments.Where(x => x != "")
                    );
                    this.uiRadioButton_UseShell.IsChecked = true;
                    break;
                default:
                    this.uiRadioButton_NotShell.IsChecked = true;
                    this.uiTextBox_FinalTarget.Text = string.Empty;
                    this.uiTextBox_FinalArgs.Text = string.Empty;
                    this.uiGrid_Shutdown.Visibility = Visibility.Hidden;
                    this.uiGrid_KillDingTalk.Visibility = Visibility.Hidden;
                    break;
            }
        }

        private void PresetCmdSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.MakeFinalCmdChanged();
        }

        private void OnTextBoxShutdownDelayInput(object sender, TextCompositionEventArgs e)
        {
            if (uint.TryParse(this.uiTextBox_ShutdownDelay.Text + e.Text, out _) == false)
                e.Handled = true;
        }

        private void OnComboShutTimeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.MakeFinalCmdChanged();
            this.uiTextBox_ShutdownDelay.IsEnabled =
                this.uiComboBox_ShutdownTime.SelectedIndex == 1;
        }

        private void OnTextBoxShutDelayTextChanged(object sender, TextChangedEventArgs e)
        {
            this.MakeFinalCmdChanged();
        }

        private void OnCheckBoxForceShutdownClick(object sender, RoutedEventArgs e)
        {
            this.MakeFinalCmdChanged();
        }

        private void OnTextBoxImageNameTextChanged(object sender, TextChangedEventArgs e)
        {
            this.MakeFinalCmdChanged();
        }

        private void OnCheckBoxForceKillClick(object sender, RoutedEventArgs e)
        {
            this.MakeFinalCmdChanged();
        }

        private void OnCheckBoxKillChildProcessClick(object sender, RoutedEventArgs e)
        {
            this.MakeFinalCmdChanged();
        }
    }

    [Serializable]
    public class CommandExecutor : AbsTaskModule
    {
        private readonly bool useShell = false;
        private readonly string fileName = null;
        private readonly string args = "";
        private readonly string workingDir = null;

        public CommandExecutor(string fileName, string args, string workingDir, bool useShell)
        {
            this.fileName = fileName;
            this.args = args;
            this.useShell = useShell;
            this.workingDir = workingDir;
        }

        public override string Name
        {
            get { return "系统命令执行器任务模块"; }
        }

        public override int ModuleType
        {
            get { return 1; }
        }

        public override bool Execute()
        {
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = this.fileName;
                    process.StartInfo.Arguments = this.args;
                    process.StartInfo.WorkingDirectory = this.workingDir;
                    process.StartInfo.LoadUserProfile = true;
                    process.StartInfo.UseShellExecute = this.useShell;
                    if (this.useShell)
                        // 和文档相反，UseShellExecute 为 true 时用此才有效果
                        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    else
                        // 和文档相反，UseShellExecute 为 false 时用此才有效果
                        process.StartInfo.CreateNoWindow = true;
                    process.Start();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override bool IsMatch()
        {
            return false;
        }

        public override string ExecutorDetails()
        {
            return $"执行命令：\n{this.fileName}\n\n命令参数：\n{this.args}" +
                $"\n\n命令执行方式：\n{(this.useShell ? "终端执行" : "创建进程")}";
        }
    }
}
