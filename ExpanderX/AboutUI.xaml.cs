using CommonUtils;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Interop;

namespace ExpanderX
{
    /// <summary>
    /// AboutUI.xaml 的交互逻辑。
    /// </summary>
    public partial class AboutUI : Window
    {
        private readonly WindowInteropHelper helper;
        public AboutUI()
        {
            this.InitializeComponent();
            this.helper = new WindowInteropHelper(this);

        }

        private void OnHyperlinkGoToClick(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            try
            { Process.Start(new ProcessStartInfo(link.NavigateUri.AbsoluteUri)); }
            catch (Exception)
            {
                USER.MessageBox(this.helper.Handle, "无法打开超链接。", "提示", MB.MB_TOPMOST);
                return;
            }
        }

        public string Version
        {
            get { return this.uiTextBlock_Version.Text; }
            set { this.uiTextBlock_Version.Text = value; }
        }

        private void OnButtonCopyInfoClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText($"版本：{this.Version}\n作者：hrpzcf\n源码：\n" +
                $"{this.uiHyperlinkGitee.NavigateUri}\n{this.uiHyperlinkGithub.NavigateUri}");
            this.Close();
        }
    }
}
