using ExpanderXSDK;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ExpanderX
{
    /// <summary>
    /// UserCtrlTimeMatcher.xaml 的交互逻辑。
    /// </summary>
    public partial class UserCtrlTimeMatcher : UserControl, IGetTaskModule
    {
        private static readonly string[] n24 = Enumerable
            .Range(0, 24)
            .Select(x => x.ToString())
            .ToArray();
        private static readonly string[] n60 = Enumerable
            .Range(0, 60)
            .Select(x => x.ToString())
            .ToArray();
        private readonly TimeMatcher tm = new TimeMatcher();

        public UserCtrlTimeMatcher()
        {
            this.InitializeComponent();
            this.SetComboBoxItems();
        }

        public bool Commit()
        {
            if (string.IsNullOrEmpty(this.uiTextBox_DurationHou.Text))
                this.uiTextBox_DurationHou.Text = "0";
            if (string.IsNullOrEmpty(this.uiTextBox_DurationMin.Text))
                this.uiTextBox_DurationMin.Text = "0";
            if (string.IsNullOrEmpty(this.uiTextBox_DurationSec.Text))
                this.uiTextBox_DurationSec.Text = "0";
            if (string.IsNullOrEmpty(this.uiTextBox_DeviationUpper.Text))
                this.uiTextBox_DeviationUpper.Text = "0";
            if (string.IsNullOrEmpty(this.uiTextBox_DeviationLower.Text))
                this.uiTextBox_DeviationLower.Text = "0";
            if (this.uiRadioButton_Duration.IsChecked ?? false)
            {
                this.tm.MatchingMethod = 0;
                this.tm.Duration = new TimeSpan(
                    int.Parse(this.uiTextBox_DurationHou.Text),
                    int.Parse(this.uiTextBox_DurationMin.Text),
                    int.Parse(this.uiTextBox_DurationSec.Text)
                );
                this.tm.ResetAfterMatched = this.uiCheckBox_ResetAfterMatched.IsChecked ?? false;
                return true;
            }
            else if (this.uiRadioButton_TimePoint.IsChecked ?? false)
            {
                this.tm.MatchingMethod = 1;
                this.tm.TimePoint = new TimeSpan(
                    int.Parse(this.uiComboBox_TimePointHou.Text),
                    int.Parse(this.uiComboBox_TimePointMin.Text),
                    int.Parse(this.uiComboBox_TimePointSec.Text)
                );
                this.tm.DeviationUpper = new TimeSpan(
                    0,
                    0,
                    int.Parse(this.uiTextBox_DeviationUpper.Text)
                );
                this.tm.DeviationLower = new TimeSpan(
                    0,
                    0,
                    int.Parse(this.uiTextBox_DeviationLower.Text)
                );
                return true;
            }
            else if (this.uiRadioButton_Period.IsChecked ?? true)
            {
                this.tm.MatchingMethod = 2;
                this.tm.PeriodBgn = new TimeSpan(
                    int.Parse(this.uiComboBox_PeriodBgnHou.Text),
                    int.Parse(this.uiComboBox_PeriodBgnMin.Text),
                    int.Parse(this.uiComboBox_PeriodBgnSec.Text)
                );
                this.tm.PeriodEnd = new TimeSpan(
                    int.Parse(this.uiComboBox_PeriodEndHou.Text),
                    int.Parse(this.uiComboBox_PeriodEndMin.Text),
                    int.Parse(this.uiComboBox_PeriodEndSec.Text)
                );
                if (this.tm.PeriodBgn >= this.tm.PeriodEnd)
                    this.tm.PeriodEnd += new TimeSpan(1, 0, 0, 0);
                return true;
            }
            else
            {
                this.tm.MatchingMethod = -1;
            }
            return false; // 不符合以上条件则拒绝添加实例
        }

        public AbsTaskModule GetTaskModule()
        {
            return this.tm;
        }

        private void SetComboBoxItems()
        {
            this.uiComboBox_TimePointHou.ItemsSource = n24;
            this.uiComboBox_PeriodBgnHou.ItemsSource = n24;
            this.uiComboBox_PeriodEndHou.ItemsSource = n24;
            this.uiComboBox_TimePointMin.ItemsSource = n60;
            this.uiComboBox_PeriodBgnMin.ItemsSource = n60;
            this.uiComboBox_PeriodEndMin.ItemsSource = n60;
            this.uiComboBox_TimePointSec.ItemsSource = n60;
            this.uiComboBox_PeriodBgnSec.ItemsSource = n60;
            this.uiComboBox_PeriodEndSec.ItemsSource = n60;
            this.uiComboBox_TimePointHou.SelectedIndex = 0;
            this.uiComboBox_PeriodBgnHou.SelectedIndex = 0;
            this.uiComboBox_PeriodEndHou.SelectedIndex = 0;
            this.uiComboBox_TimePointMin.SelectedIndex = 0;
            this.uiComboBox_PeriodBgnMin.SelectedIndex = 0;
            this.uiComboBox_PeriodEndMin.SelectedIndex = 0;
            this.uiComboBox_TimePointSec.SelectedIndex = 0;
            this.uiComboBox_PeriodBgnSec.SelectedIndex = 0;
            this.uiComboBox_PeriodEndSec.SelectedIndex = 0;
        }

        private void HoursPreviewInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (int.TryParse(textBox.Text + e.Text, out int res) == false)
            {
                e.Handled = true;
                return;
            }
            if (res < 0 || res > 23)
            {
                e.Handled = true;
            }
        }

        private void MinsSecsPreviewInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (int.TryParse(textBox.Text + e.Text, out int res) == false)
            {
                e.Handled = true;
                return;
            }
            if (res < 0 || res > 59)
            {
                e.Handled = true;
            }
        }

        private void TextBoxInputUnsignIntLimits(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (uint.TryParse(textBox.Text, out _) == false)
                e.Handled = true;
        }

        private void OnRadioButtonMatchingMethodClick(object sender, RoutedEventArgs e)
        {
            if (this.uiRadioButton_Duration.IsChecked == true)
            {
                this.uiTextBox_DurationHou.IsEnabled = true;
                this.uiTextBox_DurationMin.IsEnabled = true;
                this.uiTextBox_DurationSec.IsEnabled = true;
                this.uiCheckBox_ResetAfterMatched.IsEnabled = true;
                this.uiComboBox_TimePointHou.IsEnabled = false;
                this.uiComboBox_TimePointMin.IsEnabled = false;
                this.uiComboBox_TimePointSec.IsEnabled = false;
                this.uiTextBox_DeviationUpper.IsEnabled = false;
                this.uiTextBox_DeviationLower.IsEnabled = false;
                this.uiComboBox_PeriodBgnHou.IsEnabled = false;
                this.uiComboBox_PeriodBgnMin.IsEnabled = false;
                this.uiComboBox_PeriodBgnSec.IsEnabled = false;
                this.uiComboBox_PeriodEndHou.IsEnabled = false;
                this.uiComboBox_PeriodEndMin.IsEnabled = false;
                this.uiComboBox_PeriodEndSec.IsEnabled = false;
            }
            else if (this.uiRadioButton_TimePoint.IsChecked == true)
            {
                this.uiTextBox_DurationHou.IsEnabled = false;
                this.uiTextBox_DurationMin.IsEnabled = false;
                this.uiTextBox_DurationSec.IsEnabled = false;
                this.uiCheckBox_ResetAfterMatched.IsEnabled = false;
                this.uiComboBox_TimePointHou.IsEnabled = true;
                this.uiComboBox_TimePointMin.IsEnabled = true;
                this.uiComboBox_TimePointSec.IsEnabled = true;
                this.uiTextBox_DeviationUpper.IsEnabled = true;
                this.uiTextBox_DeviationLower.IsEnabled = true;
                this.uiComboBox_PeriodBgnHou.IsEnabled = false;
                this.uiComboBox_PeriodBgnMin.IsEnabled = false;
                this.uiComboBox_PeriodBgnSec.IsEnabled = false;
                this.uiComboBox_PeriodEndHou.IsEnabled = false;
                this.uiComboBox_PeriodEndMin.IsEnabled = false;
                this.uiComboBox_PeriodEndSec.IsEnabled = false;
            }
            else
            {
                this.uiTextBox_DurationHou.IsEnabled = false;
                this.uiTextBox_DurationMin.IsEnabled = false;
                this.uiTextBox_DurationSec.IsEnabled = false;
                this.uiCheckBox_ResetAfterMatched.IsEnabled = false;
                this.uiComboBox_TimePointHou.IsEnabled = false;
                this.uiComboBox_TimePointMin.IsEnabled = false;
                this.uiComboBox_TimePointSec.IsEnabled = false;
                this.uiTextBox_DeviationUpper.IsEnabled = false;
                this.uiTextBox_DeviationLower.IsEnabled = false;
                this.uiComboBox_PeriodBgnHou.IsEnabled = true;
                this.uiComboBox_PeriodBgnMin.IsEnabled = true;
                this.uiComboBox_PeriodBgnSec.IsEnabled = true;
                this.uiComboBox_PeriodEndHou.IsEnabled = true;
                this.uiComboBox_PeriodEndMin.IsEnabled = true;
                this.uiComboBox_PeriodEndSec.IsEnabled = true;
            }
        }
    }

    [Serializable]
    public class TimeMatcher : AbsTaskModule
    {
        private dynamic sw;
        private bool matched;

        /// <summary>
        /// 0 表示匹配时长，1 表示只匹配某时间点，2 表示匹配某时间范围。
        /// </summary>
        public int MatchingMethod = 0;
        public TimeSpan Duration;
        public bool ResetAfterMatched;
        public TimeSpan TimePoint;
        public TimeSpan DeviationUpper;
        public TimeSpan DeviationLower;
        public TimeSpan PeriodBgn;
        public TimeSpan PeriodEnd;

        public override string Name
        {
            get { return "时长/时间点/时间段匹配器"; }
        }

        public override int ModuleType
        {
            get { return 0; }
        }

        public override bool Init()
        {
            if (this.MatchingMethod == 0)
            {
                this.sw = new Stopwatch();
                this.sw.Start();
            }
            this.matched = false;
            return true;
        }

        public override bool Stop()
        {
            if (this.sw != null)
            {
                this.sw.Stop();
                this.sw = null;
            }
            this.matched = false;
            return true;
        }

        public override bool Execute()
        {
            return false;
        }

        public override bool IsMatch()
        {
            DateTime datetime = DateTime.Now;
            switch (this.MatchingMethod)
            {
                case 0:
                    if (this.sw.ElapsedMilliseconds >= this.Duration.TotalMilliseconds)
                    {
                        if (this.ResetAfterMatched)
                            this.sw.Restart();
                        return true;
                    }
                    return false;
                case 1:
                    if (this.matched)
                        return false;
                    if (
                        datetime >= datetime.Date + this.TimePoint - this.DeviationLower
                        && datetime <= datetime.Date + this.TimePoint + this.DeviationUpper
                    )
                    {
                        this.matched = true;
                        return true;
                    }
                    return false;
                case 2:
                    if (
                        datetime.Date + this.PeriodBgn <= datetime
                        && datetime <= datetime.Date + this.PeriodEnd
                    )
                        return true;
                    return false;
                case -1:
                    return false;
            }
            return false;
        }

        public override string MatcherDetails()
        {
            switch (this.MatchingMethod)
            {
                case 0:
                    return $"匹配方式：匹配累计时长\n匹配时长：{this.Duration.Hours}小时"
                        + $"{this.Duration.Minutes}分{this.Duration.Seconds}秒\n"
                        + $"匹配后是否重置累计时长：{(this.ResetAfterMatched ? "是" : "否")}";
                case 1:
                    return $"匹配方式：按时间点匹配\n时间点：{this.TimePoint}，允许误差："
                        + $"+{this.DeviationUpper.TotalSeconds}-{this.DeviationLower.TotalSeconds}秒";
                case 2:
                    TimeSpan temp = this.PeriodEnd;
                    string ends = "";
                    if (temp.Days > 0)
                    {
                        ends = "次日";
                        temp -= new TimeSpan(1, 0, 0, 0);
                    }
                    return $"匹配方式：按时间段匹配\n时间段：{this.PeriodBgn}~{ends}{temp}";
                default:
                    return $"未知的匹配方式";
            }
        }
    }
}
