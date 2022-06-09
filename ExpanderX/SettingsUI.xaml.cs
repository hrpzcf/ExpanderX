using CommonUtils;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ExpanderX
{
    /// <summary>
    /// SettingsUI.xaml 的交互逻辑。
    /// </summary>
    public partial class SettingsUI : Window
    {
        private readonly Regex inputLimitRe = new Regex("^[0-9]?[.]?[0-9]*$");

        public SettingsUI()
        {
            this.InitializeComponent();
            this.LoadAndSetSavedSettingsToUI();
        }

        private void LoadAndSetSavedSettingsToUI()
        {
            Settings s = PubSets.CurSettings;
            try
            {
                this.uiTextBox_IntervalLower.Text = s.IntervalLower;
                this.uiTextBox_IntervalUpper.Text = s.IntervalUpper;
                this.uiCheckBox_RemMainWinSize.IsChecked = s.RemMainWinSize;
                this.uiCheckBox_RemAddWorkWinSize.IsChecked = s.RemAddTaskWinSize;
                this.uiCheckBox_RemTipsWinSize.IsChecked = s.RemTipsWinSize;
                this.uiCheckBox_RemTipsWinPos.IsChecked = s.RemTipsWinPos;
                if (s.HideToTrayWhenClose)
                    this.uiRadioButton_ToTray.IsChecked = true;
                else
                    this.uiRadioButton_ExitApp.IsChecked = true;
                this.uiCheckBox_CheckIfClientReady.IsChecked = s.CheckIfClientReady;
                this.uiComBox_UnsavedTaskModules.SelectedIndex = s.ExitWithUnsavedTaskModule;
                this.uiComBox_UnsavedTaskModules.SelectionChanged += ComboBoxSelectionChanged;
            }
            catch
            {
                USER.MessageBox(IntPtr.Zero, "加载已保存的设置失败。", "警告", MB.MB_TOPMOST);
            }
        }

        private void OnButtonCancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnButtonApplyClick(object sender, RoutedEventArgs e)
        {
            Settings s = PubSets.CurSettings;
            try
            {
                s.IntervalLower = this.uiTextBox_IntervalLower.Text;
                s.IntervalUpper = this.uiTextBox_IntervalUpper.Text;
                s.RemMainWinSize = this.uiCheckBox_RemMainWinSize.IsChecked ?? false;
                s.RemAddTaskWinSize = this.uiCheckBox_RemAddWorkWinSize.IsChecked ?? false;
                s.RemTipsWinPos = this.uiCheckBox_RemTipsWinPos.IsChecked ?? false;
                s.RemTipsWinSize = this.uiCheckBox_RemTipsWinSize.IsChecked ?? false;
                s.HideToTrayWhenClose = this.uiRadioButton_ToTray.IsChecked ?? true;
                s.CheckIfClientReady = this.uiCheckBox_CheckIfClientReady.IsChecked ?? true;
                s.ExitWithUnsavedTaskModule = this.uiComBox_UnsavedTaskModules.SelectedIndex;
            }
            catch
            {
                s = new Settings
                {
                    IntervalLower = this.uiTextBox_IntervalLower.Text,
                    IntervalUpper = this.uiTextBox_IntervalUpper.Text,
                    RemMainWinSize = this.uiCheckBox_RemMainWinSize.IsChecked ?? false,
                    RemAddTaskWinSize = this.uiCheckBox_RemAddWorkWinSize.IsChecked ?? false,
                    RemTipsWinPos = this.uiCheckBox_RemTipsWinPos.IsChecked ?? false,
                    RemTipsWinSize = this.uiCheckBox_RemTipsWinSize.IsChecked ?? false,
                    HideToTrayWhenClose = this.uiRadioButton_ToTray.IsChecked ?? true,
                    CheckIfClientReady = this.uiCheckBox_CheckIfClientReady.IsChecked ?? true,
                    ExitWithUnsavedTaskModule = this.uiComBox_UnsavedTaskModules.SelectedIndex,
                };
            }

            PubSets.CurSettings = s;
            this.uiButton_ApplyConf.IsEnabled = false;
        }

        private void OnButtonLoadDefaultClick(object sender, RoutedEventArgs e)
        {
            this.uiButton_ApplyConf.IsEnabled = true;
            this.uiButton_LoadDefault.IsEnabled = false;
            Settings s = new Settings();
            this.uiTextBox_IntervalLower.Text = s.IntervalLower;
            this.uiTextBox_IntervalUpper.Text = s.IntervalUpper;
            this.uiCheckBox_RemMainWinSize.IsChecked = s.RemMainWinSize;
            this.uiCheckBox_RemAddWorkWinSize.IsChecked = s.RemAddTaskWinSize;
            this.uiCheckBox_RemTipsWinPos.IsChecked = s.RemTipsWinPos;
            this.uiCheckBox_RemTipsWinSize.IsChecked = s.RemTipsWinSize;
            if (s.HideToTrayWhenClose)
                this.uiRadioButton_ToTray.IsChecked = true;
            else
                this.uiRadioButton_ExitApp.IsChecked = true;
            this.uiCheckBox_CheckIfClientReady.IsChecked = s.CheckIfClientReady;
            this.uiComBox_UnsavedTaskModules.SelectedIndex = s.ExitWithUnsavedTaskModule;
        }

        private void ChangeButtonEnable()
        {
            this.uiButton_ApplyConf.IsEnabled = true;
            this.uiButton_LoadDefault.IsEnabled = true;
        }

        private void OnTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            this.ChangeButtonEnable();
        }

        private void OnUiCheckBoxClick(object sender, RoutedEventArgs e)
        {
            this.ChangeButtonEnable();
        }

        private void OnRadioButtonHideOrExitClick(object sender, RoutedEventArgs e)
        {
            this.ChangeButtonEnable();
        }

        private void OnTextBoxTimeLimitPreviewInput(object sender, TextCompositionEventArgs e)
        {
            string text = ((TextBox)sender).Text + e.Text;
            if (!this.inputLimitRe.IsMatch(text))
                e.Handled = true;
            if (text.IndexOf('.') == -1 && text.Length > 2)
                e.Handled = true;
            else if (text.Length > 4)
                e.Handled = true;
        }

        private void ComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.ChangeButtonEnable();
        }
    }
}
