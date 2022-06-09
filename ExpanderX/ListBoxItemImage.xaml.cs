using ExpanderXSDK;
using System.Windows;
using System.Windows.Controls;

namespace ExpanderX
{
    /// <summary>
    /// ListBoxItemIcon.xaml 的交互逻辑。
    /// </summary>
    public partial class ListBoxItemImage : UserControl
    {
        public ListBoxItemImage(AbsTaskModule tm)
        {
            this.InitializeComponent();
            this.SetupUI(tm);
        }

        private void SetupUI(AbsTaskModule w)
        {
            this.uiTextBox_TaskModuleName.Text = w.Name;
            switch (w.TaskType)
            {
            case 0:
                this.uiImage_TaskModuleIcon.Style = (Style)RES.Res["ImgMatcher"];
                break;
            case 1:
                this.uiImage_TaskModuleIcon.Style = (Style)RES.Res["ImgExecutor"];
                break;
            case 2:
                this.uiImage_TaskModuleIcon.Style = (Style)RES.Res["ImgMixed"];
                break;
            default:
                this.uiImage_TaskModuleIcon.Style = (Style)RES.Res["ImgMatcher"];
                break;
            }
        }
    }
}
