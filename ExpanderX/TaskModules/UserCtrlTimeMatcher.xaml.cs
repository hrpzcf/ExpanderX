using ExpanderXSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ExpanderX
{
    /// <summary>
    /// UserCtrlTimeMatcher.xaml 的交互逻辑
    /// </summary>
    public partial class UserCtrlTimeMatcher : UserControl
    {
        public UserCtrlTimeMatcher()
        {
            InitializeComponent();
        }
    }

    public class TimeMatcher : AbsTaskModule
    {
        /// <summary>
        /// 0 表示只匹配某时间点，1 表示匹配某时间范围
        /// </summary>
        public int MatchType = 0;

        public override string Name
        {
            get { return "时间匹配器"; }
        }

        public override int TaskType
        {
            get { return 0; }
        }

        public override bool Execute()
        {
            return false;
        }

        public override bool IsMatch()
        {
            throw new NotImplementedException();
        }
    }
}
