using ExpanderXSDK;
using System;
using System.Windows.Controls;

namespace ExpanderX
{
    /// <summary>
    /// ExpanderXControler.xaml 的交互逻辑。
    /// </summary>
    public partial class UserCtrlExpanderXCtrl : UserControl, IGetTaskModule
    {
        private readonly ExpanderXControler tm = new ExpanderXControler();
        public UserCtrlExpanderXCtrl()
        {
            this.InitializeComponent();
        }

        public bool Commit()
        {
            if (this.uiComboBox_StartStop.SelectedIndex == -1)
                return false;
            this.tm.controlType = this.uiComboBox_StartStop.SelectedIndex;
            return true;
        }

        public AbsTaskModule GetTaskModule() { return this.tm; }
    }

    [Serializable]
    public class ExpanderXControler : AbsTaskModule
    {
        public override int ModuleType { get { return 1; } }
        public int controlType = 0;
        public override string Name { get { return "ExpanderX服务启停控制器"; } }

        public override bool Execute()
        {
            switch (this.controlType)
            {
                case 0:
                    PubEX.Stop();
                    break;
                case 1:
                    PubEX.Start();
                    break;
                case 2:
                    PubEX.Exit();
                    break;
            }
            return true;
        }

        public override string ExecutorDetails()
        {
            if (this.controlType == 0)
                return "发起\"停止ExpanderX服务\"的请求。";
            else if (this.controlType == 1)
                return "发起\"启动ExpanderX服务\"的请求。";
            else if (this.controlType == 2)
                return "发起\"退出ExpanderX程序\"的请求。";
            else
                return base.ExecutorDetails();
        }

        public override bool IsMatch() { return false; }
    }
}
