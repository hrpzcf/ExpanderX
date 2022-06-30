using ExpanderXSDK;
using System;
using System.Diagnostics;
using System.Threading;

namespace ExpanderX
{
    /// <summary>
    /// 任务包循环器。
    /// </summary>
    internal sealed class Circulator
    {
        private AbsTaskPack[] taskPacks;
        private Thread srvThread;
        private readonly object locker = new object();
        private STATE state = STATE.Stopped;
        private bool keepState = true;

        /// <summary>
        /// 循环服务开始方法。
        /// </summary>
        /// <returns></returns>
        public bool Start(AbsTaskPack[] packs)
        {
            if (this.state == STATE.Running)
                this.Stop();
            this.taskPacks = packs;
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
            foreach (AbsTaskPack r in this.taskPacks)
                foreach (AbsTaskModule t in r.TaskModules)
                    try
                    {
                        _ = t.Stop();
                    }
                    catch { }
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

            foreach (AbsTaskPack r in this.taskPacks)
                foreach (AbsTaskModule t in r.TaskModules)
                    try
                    {
                        _ = t.Init();
                    }
                    catch { }
            try
            {
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
            long roundIntervalLimit = 100;
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
                    roundIntervalLimit = random.Next(interval[0], interval[1]);
                }
                catch { }
                if (milliseconds - lastMilliseconds < roundIntervalLimit)
                    try
                    {
                        Thread.Sleep((int)(roundIntervalLimit - (milliseconds - lastMilliseconds)));
                    }
                    catch (ThreadInterruptedException)
                    {
                        goto StopWatchAndEndLoop;
                    }
                lastMilliseconds = stopwatch.ElapsedMilliseconds;
                foreach (AbsTaskPack rl in this.taskPacks)
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
