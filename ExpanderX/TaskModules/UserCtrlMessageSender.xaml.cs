using ExpanderXSDK;
using System;
using System.Windows.Controls;

namespace ExpanderX
{
    public partial class UserCtrlMessageSender : UserControl, IGetTaskModule
    {
        private readonly DTalkMessageSender tm = new DTalkMessageSender();

        public UserCtrlMessageSender()
        {
            this.InitializeComponent();
        }

        public bool Commit()
        {
            this.tm.TextToSend = this.uiTextBox_MsgToSend.Text;
            if (this.tm.TextToSend == "")
                return false;
            else
            {
                this.uiTextBox_MsgToSend.Clear();
                return true;
            }
        }

        public AbsTaskModule GetTaskModule() { return this.tm; }
    }

    /// <summary>
    /// 发送钉钉消息执行器。
    /// </summary>
    [Serializable]
    public class DTalkMessageSender : AbsTaskModule
    {
        public DTalkMessageSender() { this.TextToSend = ""; }

        public override int ModuleType { get { return 1; } }
        public string TextToSend { get; set; }
        public override string Name { get { return "自动发送钉钉消息任务模块"; } }

        public override string ExecutorDetails()
        {
            return this.TextToSend == ""
                ? "没有需要发送的消息。" : $"使用钉钉发送此消息：\n{this.TextToSend}";
        }

        public override bool IsMatch() { return false; }

        public override bool Execute()
        {
            return this.TextToSend == "" || PubDTools.SendMessage(this.TextToSend);
        }
    }
}
