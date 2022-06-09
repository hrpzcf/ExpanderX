using ExpanderXSDK;
using System.Windows;
using System.Windows.Controls;

namespace ExpanderX
{
    /// <summary>
    /// ListBoxItemCheck.xaml 的交互逻辑。
    /// </summary>
    public partial class ListBoxItemCheck : UserControl
    {
        private readonly AbsRuleModel rule = null;

        public ListBoxItemCheck(AbsRuleModel rule)
        {
            this.InitializeComponent();
            this.uiTextBlock_RuleName.Text = rule.CustomName;
            this.uiCheckBox_RuleEnable.IsChecked = rule.IsEnable;
            this.rule = rule;
        }

        /// <summary>
        /// 包含的通用规则实例。
        /// </summary>
        public AbsRuleModel Rule { get { return this.rule; } }

        private void OnCheckBoxRuleEnableClick(object sender, RoutedEventArgs e)
        {
            if (this.rule == null)
                return;
            this.rule.IsEnable = this.uiCheckBox_RuleEnable.IsChecked ?? false;
        }

        internal void ChangeCheckedState(bool state)
        {

            this.rule.IsEnable = state;
            this.uiCheckBox_RuleEnable.IsChecked = state;
        }
    }
}
