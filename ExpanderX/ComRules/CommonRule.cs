using ExpanderXSDK;
using System;

namespace ExpanderX
{
    /// <summary>
    /// 常用规则模型，目前只有这一种规则模型。
    /// </summary>
    [Serializable]
    public sealed class CommonRule : AbsRuleModel
    {
        /// <summary>
        /// 返回规则的详情描述，一般是规则内包含哪些匹配器及执行器的描述。
        /// </summary>
        /// <returns></returns>
        public override string Description() { return "通用规则模型的实例。"; }

        /// <summary>
        /// 规则名称。
        /// </summary>
        public override string Name { get { return "通用规则模型"; } }
    }
}
