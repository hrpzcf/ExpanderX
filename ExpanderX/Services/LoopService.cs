using ExpanderXSDK;
using System;
using System.Diagnostics;
using System.Threading;

namespace ExpanderX
{
    /// <summary>
    /// 循环器。
    /// </summary>
    internal sealed class Circulator
    {
        private AbsRuleModel[] enableRules;
        private Thread srvThread;
        private readonly object locker = new object();
        private STATE state = STATE.Stopped;
        private bool keepState = true;

        /// <summary>
        /// 循环服务开始方法。
        /// </summary>
        /// <returns></returns>
        public bool Start(AbsRuleModel[] rules)
        {
            if (this.state == STATE.Running)
                this.Stop();
            this.enableRules = rules;
            lock (this.locker)
            {
                this.keepState = true;
                return this.CreateLoopThread();
            }
        }

        /// <summary>
        /// 循环服务停止方法。
        /// </summary>
        /// <returns></returns>
        public bool Stop()
        {
            lock (this.locker)
                this.keepState = false;
            if (this.srvThread == null || !this.srvThread.IsAlive)
            {
                this.srvThread = null;
                return true;
            }
            try
            {
                this.srvThread.Interrupt();
                this.srvThread.Join();
            }
            catch (ThreadStateException) // 不捕获SecurityException
            {
                return false;
            }
            catch (ThreadInterruptedException) { }
            finally
            {
                this.srvThread = null;
            }
            foreach (AbsRuleModel r in this.enableRules)
                foreach (AbsTaskModule t in r.TaskModules)
                    _ = t.Init();
            return true;
        }

        public STATE State()
        {
            lock (this.locker)
                return this.state;
        }

        private bool CreateLoopThread()
        {
            if (this.srvThread != null)
                return false;
            try
            {
                foreach (AbsRuleModel r in this.enableRules)
                    foreach (AbsTaskModule t in r.TaskModules)
                        _ = t.Init();
                this.srvThread = new Thread(this.LoopThreadMain);
                this.srvThread.IsBackground = true;
                this.srvThread.Start();
                return true;
            }
            catch
            {
                this.srvThread = null;
                return false;
            }
        }

        private void LoopThreadMain()
        {
            Stopwatch stopwatch = new Stopwatch();
            long timeIntervalLimit = 100;
            long lastMilliseconds = stopwatch.ElapsedMilliseconds;
            Random random = new Random();
            lock (this.locker)
                this.state = STATE.Running;
            while (this.keepState)
            {
                long milliseconds = stopwatch.ElapsedMilliseconds;
                int[] interval = PubSettings.CurSettings.Interval();
                try
                {
                    timeIntervalLimit = random.Next(interval[0], interval[1]);
                }
                catch { }
                if (milliseconds - lastMilliseconds < timeIntervalLimit)
                    try
                    {
                        Thread.Sleep((int)(timeIntervalLimit - (milliseconds - lastMilliseconds)));
                    }
                    catch (ThreadInterruptedException)
                    {
                        goto StopWatchAndEndLoop;
                    }
                lastMilliseconds = stopwatch.ElapsedMilliseconds;
                foreach (AbsRuleModel rl in this.enableRules)
                {
                    rl.ExecuteTask();
                }
            }
        StopWatchAndEndLoop:
            lock (this.locker)
                this.state = STATE.Stopped;
            stopwatch.Stop();
        }
    }
}
