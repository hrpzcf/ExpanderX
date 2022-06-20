using CommonUtils;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace ExpanderX
{
    [Serializable]
    internal class Settings
    {
        public bool LicenseAccepted = false;
        public bool RemMainWinSize = false;
        public double[] MainWinSize = { 800, 600 };
        public bool RemAddTaskWinSize = false;
        public double[] AddTaskWinSize = { 800, 600 };
        public bool RemTipsWinPos = false;
        public double[] TipsWinPos = { 500, 100 };
        public bool RemTipsWinSize = false;
        public double[] TipsWinSize = { 500, 300 };
        public bool CheckIfClientReady = true;
        public bool HideToTrayWhenClose = true;
        public int ExitWithUnsavedTaskModule = 0;

        private double intervalLower = 1.0;
        private double intervalUpper = 2.0;

        public int[] Interval()
        {
            double temp;
            if (this.intervalLower > this.intervalUpper)
            {
                temp = this.intervalLower;
                this.intervalLower = this.intervalUpper;
                this.intervalUpper = temp;
            }
            return new int[] { (int)(this.intervalLower * 1000), (int)(this.intervalUpper * 1000) };
        }

        public string IntervalLower
        {
            get { return this.intervalLower.ToString(); }
            set
            {
                bool res = double.TryParse(value, out double il);
                if (res)
                    this.intervalLower = il < 0.1 ? 0.1 : il;
            }
        }

        public string IntervalUpper
        {
            get { return this.intervalUpper.ToString(); }
            set
            {
                bool res = double.TryParse(value, out double iu);
                if (res)
                    this.intervalUpper = iu < 0.1 ? 0.1 : iu;
            }
        }
    }

    /// <summary>
    /// 主设置工具集。
    /// </summary>
    internal static class PubSettings
    {
        private static readonly IFormatter fmt = new BinaryFormatter();
        private static Settings curSettings = null;
        private static readonly DirectoryInfo confDir = new DirectoryInfo(
            Path.Combine(ExpanderXMain.basicFolder, "Config")
        );
        private static readonly string confFile = Path.Combine(confDir.FullName, "config.bin");

        public static Settings CurSettings
        {
            get
            {
                if (curSettings == null)
                    curSettings = LoadConfig();
                return curSettings;
            }
            set
            {
                curSettings = value;
                SaveConfig(value);
            }
        }

        private static bool SaveConfig(Settings s)
        {
            try
            {
                if (!confDir.Exists)
                {
                    confDir.Create();
                }
                using (FileStream fs = File.Create(confFile))
                {
                    fmt.Serialize(fs, s);
                }
                return true;
            }
            catch
            {
                USER.MessageBox(IntPtr.Zero, "设置保存失败！", "错误", MB.MB_TOPMOST | MB.MB_ICONERROR);
                return false;
            }
        }

        private static Settings LoadConfig()
        {
            if (!File.Exists(confFile))
                return new Settings();
            try
            {
                using (FileStream fs = File.OpenRead(confFile))
                {
                    return fmt.Deserialize(fs) is Settings s ? s : new Settings();
                }
            }
            catch (Exception)
            {
                return new Settings();
            }
        }
    }
}
