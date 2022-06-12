using ExpanderXSDK;
using System.Threading;
using System.Windows;

namespace ExpanderX
{
    internal class RequestHandler
    {
        private int interval = 300;
        private bool KeepRunning = false;
        private Thread thread = null;

        public void Start()
        {
            if (this.KeepRunning)
                return;
            this.KeepRunning = true;
            this.thread = new Thread(this.CommandProcessor);
            this.thread.IsBackground = true;
            this.thread.Start();
        }

        public void Stop()
        {
            this.KeepRunning = false;
            if (this.thread != null)
            { this.thread = null; return; }
            // 不能强行退出，否则 CommandProcessor 命中
            // case Cmd.Exit 的时候主程序调用此方法，
            // 会造成 case Cmd.Exit 分支无法完成全部流程导致异常。
        }

        private void CommandProcessor()
        {
            while (this.KeepRunning)
            {
                switch (PubEX.GetCmd())
                {
                    case Cmd.Start:
                        Application.Current.Dispatcher.Invoke(ExpanderXMain.Instance.StartService2);
                        break;
                    case Cmd.Stop:
                        Application.Current.Dispatcher.Invoke(ExpanderXMain.Instance.StopService2);
                        break;
                    case Cmd.Exit:
                        Application.Current.Dispatcher.Invoke(ExpanderXMain.Instance.CloseByReqHandler);
                        break;
                    case Cmd.UpdateState:
                        PubEX.State = ExpanderXMain.Cycle.State();
                        break;
                    case Cmd.UpdateInterval:
                        this.interval = PubEX.Interval < 100 ? 100 : PubEX.Interval > 1000 ? 1000 : PubEX.Interval;
                        break;
                    case Cmd.Nothing:
                    default:
                        break;
                }
                try
                { Thread.Sleep(this.interval); }
                catch { return; }
            }
        }
    }
}
