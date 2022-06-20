using System;
using System.ComponentModel;
using System.Windows;

namespace ExpanderX
{
    /// <summary>
    /// License.xaml 的交互逻辑。
    /// </summary>
    public partial class License : Window
    {
        public License()
        {
            this.InitializeComponent();
            this.Closing += this.License_Closing;
        }

        private void OnButtonAcceptClick(object sender, RoutedEventArgs e)
        {
            Settings s = PubSettings.CurSettings;
            s.LicenseAccepted = true;
            PubSettings.CurSettings = s;
            this.Close();
        }

        private void License_Closing(object sender, CancelEventArgs e)
        {
            if (!PubSettings.CurSettings.LicenseAccepted)
                Environment.Exit(0);
        }

        private void OnButtonRejectClick(object sender, RoutedEventArgs e)
        {
            Settings s = PubSettings.CurSettings;
            s.LicenseAccepted = false;
            PubSettings.CurSettings = s;
            this.Close();
        }
    }
}
