using ExpanderXSDK;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ExpanderX
{
    public partial class UserCtrlTopTipsExecutor : UserControl, IGetTaskModule
    {
        private readonly ShowTopTipsWinExecutor tm = new ShowTopTipsWinExecutor();

        public UserCtrlTopTipsExecutor()
        {
            this.InitializeComponent();
        }

        public bool Commit()
        {
            this.tm.TopTipsText = this.uiTextBox_MsgToShow.Text;
            if (this.uiComboBox_TopTipsType.SelectedIndex != 1 && this.tm.TopTipsText == "")
                return false;
            this.uiTextBox_MsgToShow.Clear();
            this.tm.TopTipsType = this.uiComboBox_TopTipsType.SelectedIndex;
            return true;
        }

        public AbsTaskModule GetTaskModule() { return this.tm; }
    }

    [Serializable]
    public class ShowTopTipsWinExecutor : AbsTaskModule
    {
        private const int TIPS_CUSTOMTEXT = 0;
        private const int TIPS_LASTDTMSGS = 1;

        public ShowTopTipsWinExecutor()
        {
            this.TopTipsType = 0;
            this.TopTipsText = "";
        }

        public string TopTipsText { get; set; }
        public override string Name { get { return "弹窗展示消息内容任务模块"; } }

        /// <summary>
        /// 提示填写的内容还是最后一条获取的钉钉消息。
        /// </summary>
        public int TopTipsType { get; set; }
        public override int ModuleType { get { return 1; } }

        public override string ExecutorDetails()
        {
            if (this.TopTipsType == TIPS_LASTDTMSGS)
                return "显示弹窗，并显示最后一次探测到的钉钉消息。";
            else if (this.TopTipsType == TIPS_CUSTOMTEXT)
                return $"显示弹窗并显示自定义内容：\n{this.TopTipsText}";
            else
                return "未实现的弹窗类型。";
        }

        public override bool IsMatch() { return false; }

        public override bool Execute()
        {
            if (this.TopTipsType == TIPS_CUSTOMTEXT)
                Application.Current.Dispatcher.Invoke(this.ShowCustomTipsText);
            else if (this.TopTipsType == TIPS_LASTDTMSGS)
                Application.Current.Dispatcher.Invoke(this.ShowLastMsgsTipsText);
            return true;
        }

        private void ShowCustomTipsText()
        {
            TopTipsWin tips = new TopTipsWin()
            { Message = TopTipsText };
            tips.OnlyContent();
            tips.Show();
        }

        private void ShowLastMsgsTipsText()
        {
            string[][] lastMsgs = PubDTools.LatestAcquired();
            if (lastMsgs.Length == 0)
                return;
            TopTipsWin tips = new TopTipsWin()
            {
                Sender = lastMsgs[0][0],
                Message = lastMsgs[0][1],
                NameChatting = PubDTools.GetNameChatting()
            };
            tips.Show();
        }
    }
}
