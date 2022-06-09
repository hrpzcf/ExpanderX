using System;
using System.Windows;

namespace ExpanderX
{
    /// <summary>
    /// InputBox.xaml 的交互逻辑。
    /// </summary>
    public partial class LineInputBox : Window
    {
        private string presetText = string.Empty;
        private string description = string.Empty;

        public LineInputBox()
        {
            this.InitializeComponent();
            this.uiTextBox_InputContent.Focus();
        }

        /// <summary>
        /// 点击确定后执行的动作，以输入的文本执行此动作。
        /// </summary>
        public Action<string> TextOut { get; set; }

        /// <summary>
        /// 文本输入框上方的提示文字。
        /// </summary>
        public string Description
        {
            get { return this.description; }
            set
            {
                this.description = value;
                this.uiTextBlock_Description.Text = value;
            }
        }

        /// <summary>
        /// 输入框预置的文本。
        /// </summary>
        public string PresetText
        {
            get { return this.presetText; }
            set
            {
                this.uiTextBox_InputContent.Text = value;
                this.presetText = value;
            }
        }

        private void OnButtonCancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnButtonConfirmClick(object sender, RoutedEventArgs e)
        {
            this.Close();
            this.TextOut?.Invoke(this.uiTextBox_InputContent.Text);
        }
    }
}
