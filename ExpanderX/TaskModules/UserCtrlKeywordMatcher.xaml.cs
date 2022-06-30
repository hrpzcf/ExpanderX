using ExpanderXSDK;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ExpanderX
{
    public partial class UserCtrlKeywordDetector : UserControl, IGetTaskModule
    {
        private readonly char[] seps = new char[] { '\r', '\n' };
        private readonly KeywordMatcherAndTopTips tm = new KeywordMatcherAndTopTips();

        public UserCtrlKeywordDetector()
        {
            this.InitializeComponent();
        }

        public bool Commit()
        {
            this.tm.Chatting = this.uiTextBox_NameChatting.Text;
            this.uiTextBox_NameChatting.Clear();
            this.tm.SendersInc = this.uiTextBox_SendersInclude.Text.Split(
                this.seps,
                StringSplitOptions.RemoveEmptyEntries
            );
            this.uiTextBox_SendersInclude.Clear();
            this.tm.KeywordsInc = this.uiTextBox_KeywordsInclude.Text.Split(
                this.seps,
                StringSplitOptions.RemoveEmptyEntries
            );
            this.uiTextBox_KeywordsInclude.Clear();
            this.tm.SendersExc = this.uiTextBox_SendersExclude.Text.Split(
                this.seps,
                StringSplitOptions.RemoveEmptyEntries
            );
            this.uiTextBox_SendersExclude.Clear();
            this.tm.KeywordsExc = this.uiTextBox_KeywordsExclude.Text.Split(
                this.seps,
                StringSplitOptions.RemoveEmptyEntries
            );
            this.uiTextBox_KeywordsExclude.Clear();
            this.tm.CustomTextToTips = this.uiTextBox_TopTipsText.Text;
            this.uiTextBox_TopTipsText.Clear();
            this.tm.ShowCustomOrLastMsg = this.uiComboBox_CustomOrLastMsgs.SelectedIndex;
            this.tm.ExcludeContent = this.uiTextBox_ExcludeContent.Text;
            return true;
        }

        public AbsTaskModule GetTaskModule()
        {
            return this.tm;
        }
    }

    /// <summary>
    /// 对消息内容使用关键词进行匹配的匹配器。
    /// </summary>
    [Serializable]
    public class KeywordMatcherAndTopTips : AbsTaskModule
    {
        private string lastcontent = null;
        private string lastsender = null;
        private string chattingGot = null;

        public string[] KeywordsExc { get; set; }
        public string[] KeywordsInc { get; set; }

        /// <summary>
        /// 用户设置（限制）的消息框标题。
        /// </summary>
        public string Chatting { get; set; }
        public override string Name
        {
            get { return "关键词匹配和弹窗任务模块"; }
        }
        public string[] SendersExc { get; set; }
        public string[] SendersInc { get; set; }
        public string ExcludeContent { get; set; }
        public override int ModuleType
        {
            get { return 2; }
        }

        /// <summary>
        /// 窗口提示类型。
        /// 0 表示提示自定义内容，1 表示提示最后一次检测到的钉钉消息内容。
        /// </summary>
        public int ShowCustomOrLastMsg { get; set; }
        public string CustomTextToTips { get; set; }

        public override string MatcherDetails()
        {
            string chattingdesc =
                this.Chatting == null || this.Chatting == "" ? "无限制" : this.Chatting;
            string sendersIncdesc =
                this.SendersInc == null || this.SendersInc.Length == 0
                    ? "任何人"
                    : string.Join("\n", this.SendersInc);
            string keywordsIncdesc =
                this.KeywordsInc == null || this.KeywordsInc.Length == 0
                    ? "任意关键词"
                    : string.Join("\n", this.KeywordsInc);
            string sendersExcdesc =
                this.SendersExc == null || this.SendersExc.Length == 0
                    ? "无限制"
                    : string.Join("\n", this.SendersExc);
            string keywordsExcdesc =
                this.KeywordsExc == null || this.KeywordsExc.Length == 0
                    ? "无限制"
                    : string.Join("\n", this.KeywordsExc);
            return $"匹配消息框：\n{chattingdesc}\n\n匹配发送者：\n{sendersIncdesc}\n\n"
                + $"匹配关键词：\n{keywordsIncdesc}\n\n不匹配发送者：\n{sendersExcdesc}\n\n"
                + $"不匹配关键词：\n{keywordsExcdesc}\n";
        }

        public override string ExecutorDetails()
        {
            if (this.ShowCustomOrLastMsg == 0)
            {
                string desc =
                    this.CustomTextToTips == null || this.CustomTextToTips == ""
                        ? "内容为空"
                        : this.CustomTextToTips;
                return $"显示弹窗并显示自定义内容：\n{desc}。";
            }
            else if (this.ShowCustomOrLastMsg == 1)
            {
                return "显示弹窗，并显示与匹配器条件相匹配的钉钉消息。";
            }
            else
            {
                return "未实现的弹窗选项。";
            }
        }

        public override bool IsMatch()
        {
            string current = PubDTools.GetNameChatting();
            if (current == "")
                return false;
            // 防止运行过程中切换消息框马上发生匹配
            if (this.chattingGot != null && current != this.chattingGot)
            {
                this.lastsender = null;
                this.lastcontent = null;
                this.chattingGot = current;
                return false;
            }
            this.chattingGot = current;
            if (this.Chatting != null && this.Chatting != "")
                if (current != this.Chatting)
                    return false;
            List<string[]> msgs = PubDTools.GetRecentMessages(1);
            if (msgs.Count < 1)
                return false;
            string lastSnd = this.lastsender;
            this.lastsender = msgs[0][0];
            string lastCnt = this.lastcontent;
            this.lastcontent = msgs[0][1];
            if (lastSnd == null || lastCnt == null)
                return false;
            // TODO 突然切换消息框
            if (!string.IsNullOrEmpty(this.ExcludeContent) && msgs[0][1] == this.ExcludeContent)
                return false;
            if (msgs[0][0] == lastSnd && msgs[0][1] == lastCnt)
                return false;
            // 作出判定的前提是能获取到消息
            // 获取不到消息的时候，就算 Senders 和 Keywords 为空也不应该简单地判定为真
            foreach (string s in this.SendersExc)
            {
                if (msgs[0][0] == s)
                    return false;
            }
            foreach (string k in this.KeywordsExc)
            {
                if (msgs[0][1].Contains(k))
                    return false;
            }
            bool flagSenderInc = this.SendersInc.Length == 0;
            foreach (string s in this.SendersInc)
            {
                if (flagSenderInc = msgs[0][0] == s)
                    break;
            }
            bool flagKeywordInc = this.KeywordsInc.Length == 0;
            foreach (string k in this.KeywordsInc)
            {
                if (flagKeywordInc = msgs[0][1].Contains(k))
                    break;
            }
            return flagSenderInc && flagKeywordInc;
        }

        // 此处重写 Init 方法也可
        public override bool Stop()
        {
            this.lastsender = null;
            this.lastcontent = null;
            return true;
        }

        public override bool Execute()
        {
            if (this.ShowCustomOrLastMsg == 0)
            {
                Application.Current.Dispatcher.Invoke(this.ShowCustomTipsText);
                return true;
            }
            else if (this.ShowCustomOrLastMsg == 1)
            {
                Application.Current.Dispatcher.Invoke(this.ShowLastMsgsTipsText);
            }
            return false;
        }

        private void ShowCustomTipsText()
        {
            if (this.CustomTextToTips == null)
                this.CustomTextToTips = "没有任何内容。";
            TopTipsWin tips = new TopTipsWin() { Message = this.CustomTextToTips };
            tips.OnlyContent();
            tips.Show();
        }

        private void ShowLastMsgsTipsText()
        {
            string[][] lastMessages;
            string sender;
            string content;
            if (this.lastsender == null || this.lastcontent == null)
            {
                lastMessages = PubDTools.LatestAcquired();
                if (lastMessages.Length == 0)
                    return;
                sender = lastMessages[0][0];
                content = lastMessages[0][1];
            }
            else
            {
                sender = this.lastsender;
                content = this.lastcontent;
            }
            TopTipsWin tips = new TopTipsWin()
            {
                Sender = sender,
                Message = content,
                NameChatting = this.chattingGot ?? string.Empty
            };
            tips.Show();
        }
    }
}
