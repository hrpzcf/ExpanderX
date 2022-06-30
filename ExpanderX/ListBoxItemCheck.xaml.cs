using ExpanderXSDK;
using System.Windows;
using System.Windows.Controls;

namespace ExpanderX
{
    /// <summary>
    /// ListBoxItemCheck.xaml 的交互逻辑。
    /// </summary>
    public partial class ListBoxItemCheck : UserControl
    {
        private readonly AbsTaskPack pack = null;

        public ListBoxItemCheck(AbsTaskPack pack)
        {
            this.InitializeComponent();
            this.pack = pack;
            this.uiTextBlock_PackName.Text = pack.CustomName;
            this.uiCheckBox_PackEnable.IsChecked = pack.IsEnable;
        }

        /// <summary>
        /// 包含的通用任务包实例。
        /// </summary>
        public AbsTaskPack TaskPack
        {
            get { return this.pack; }
        }

        private void OnCheckBoxPackEnableClick(object sender, RoutedEventArgs e)
        {
            if (this.pack == null)
                return;
            this.pack.IsEnable = this.uiCheckBox_PackEnable.IsChecked ?? false;
        }

        internal void ChangeCheckedState(bool state)
        {
            this.pack.IsEnable = state;
            this.uiCheckBox_PackEnable.IsChecked = state;
        }
    }
}
