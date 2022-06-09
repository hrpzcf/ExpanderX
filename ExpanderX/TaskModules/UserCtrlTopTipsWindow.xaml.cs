using System.ComponentModel;
using System.Windows;

namespace ExpanderX
{
    /// <summary>
    /// TopTipsWin.xaml 的交互逻辑。
    /// </summary>
    public partial class TopTipsWin : Window
    {
        private string message = "";
        private string sender = "";
        private string nameChatting = "";

        public TopTipsWin()
        {
            this.InitializeComponent();
            this.Closing += this.TopTipsWin_Closing;
            this.SetupWindowSize();
        }

        private void TopTipsWin_Closing(object sender, CancelEventArgs e)
        {
            Settings s = PubSets.CurSettings;
            if (s.RemTipsWinPos)
                s.TipsWinPos = new double[] { this.Left, this.Top };
            if (s.RemTipsWinSize)
                s.TipsWinSize = new double[] { this.Width, this.Height };
            PubSets.CurSettings = s;
        }

        private void SetupWindowSize()
        {
            Settings s = PubSets.CurSettings;
            try
            {
                if (s.RemTipsWinPos)
                {
                    this.Left = s.TipsWinPos[0];
                    this.Top = s.TipsWinPos[1];
                }
                if (s.RemTipsWinSize)
                {
                    this.Width = s.TipsWinSize[0];
                    this.Height = s.TipsWinSize[1];
                }
            }
            catch { }
        }

        public string Message
        {
            get { return this.message; }
            set
            {
                this.message = value;
                this.uiTextBox_Message.Text = value;
            }
        }

        public string Sender
        {
            get { return this.sender; }
            set
            {
                this.sender = value;
                this.uiTextBox_Sender.Text = value;
            }
        }

        public string NameChatting
        {
            get { return this.nameChatting; }
            set
            {
                this.nameChatting = value;
                this.uiTextBox_NameChatting.Text = value;
            }
        }

        public void HideSender()
        {
            this.uiTextBlock_Sender.Visibility = Visibility.Collapsed;
            this.uiTextBox_Sender.Visibility = Visibility.Collapsed;
            this.uiTextBlock_Message.Visibility = Visibility.Collapsed;
            this.uiTextBox_NameChatting.Visibility = Visibility.Collapsed;
            this.uiTextBlock_NameChatting.Visibility = Visibility.Collapsed;
        }
    }
}
