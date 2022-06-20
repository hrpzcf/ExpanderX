using ExpanderXSDK;
using System;
using System.Diagnostics;
using System.Threading;

namespace ExpanderX
{
    /// <summary>
    /// 规则的收尾工作运行结果。
    /// </summary>
    internal struct RuleResult
    {
        /// <summary>
        /// 所有匹配器的 Endings 方法运行结果。
        /// </summary>
        public bool[] MatcherResult;

        /// <summary>
        /// 所有执行器的 Endings 方法运行结果。
        /// </summary>
        public bool[] ExectuorResult;
    }

    /// <summary>
    /// 循环器。
    /// </summary>
    internal sealed class Circulator
    {
        private AbsRuleModel[] enableRules;
        private Thread threadSrv;
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
                _ = this.Stop();
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
        public RuleResult[] Stop()
        {
            lock (this.locker)
            { this.keepState = false; }
            if (this.threadSrv == null || !this.threadSrv.IsAlive)
            {
                this.threadSrv = null;
                return new RuleResult[0];
            }
            try
            {
                this.threadSrv.Interrupt();
                this.threadSrv.Join();
            }
            catch (ThreadStateException) // 不捕获SecurityException
            {
                return new RuleResult[0];
            }
            catch (ThreadInterruptedException) { }
            finally
            {
                this.threadSrv = null;
            }
            if (this.enableRules == null)
                return new RuleResult[0];
            RuleResult[] ruleRes = new RuleResult[this.enableRules.Length];
            for (int i = 0; i < this.enableRules.Length; ++i)
            {
                AbsRuleModel r = this.enableRules[i];
                bool[] sRes = new bool[r.Matchers.Length];
                for (int j = 0; j < r.Matchers.Length; ++j)
                {
                    try
                    {
                        sRes[j] = r.TaskModules[r.Matchers[j]].Endings();
                    }
                    catch (Exception) { sRes[j] = false; }
                }
                bool[] eRes = new bool[r.Executors.Length];
                for (int k = 0; k < r.Executors.Length; ++k)
                {
                    try
                    {
                        eRes[k] = r.TaskModules[r.Executors[k]].Endings();
                    }
                    catch (Exception) { eRes[k] = false; }
                }
                ruleRes[i] = new RuleResult { MatcherResult = sRes, ExectuorResult = eRes };
            }
            return ruleRes;
        }

        public STATE State() { lock (this.locker) { return this.state; } }

        private bool CreateLoopThread()
        {
            if (this.threadSrv != null)
                return false;
            try
            {
                this.threadSrv = new Thread(this.LoopThreadMain);
                this.threadSrv.IsBackground = true;
                this.threadSrv.Start();
                return true;
            }
            catch { this.threadSrv = null; return false; }
        }

        private void LoopThreadMain()
        {
            Stopwatch stopwatch = new Stopwatch();
            long timeIntervalLimit = 100;
            long lastMilliseconds = stopwatch.ElapsedMilliseconds;
            Random random = new Random();
            lock (this.locker)
            { this.state = STATE.Running; }
            while (this.keepState)
            {
                long milliseconds = stopwatch.ElapsedMilliseconds;
                int[] interval = PubSettings.CurSettings.Interval();
                try
                { timeIntervalLimit = random.Next(interval[0], interval[1]); }
                catch { }
                if (milliseconds - lastMilliseconds < timeIntervalLimit)
                    try
                    {
                        Thread.Sleep(
                            (int)(timeIntervalLimit - (milliseconds - lastMilliseconds)));
                    }
                    catch (ThreadInterruptedException) { goto StopWatchAndEndLoop; }
                lastMilliseconds = stopwatch.ElapsedMilliseconds;
                foreach (AbsRuleModel rl in this.enableRules)
                { rl.ExecuteTask(); }
            }
        StopWatchAndEndLoop:
            lock (this.locker)
            { this.state = STATE.Stopped; }
            stopwatch.Stop();
        }
    }
}
