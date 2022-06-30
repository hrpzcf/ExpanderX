using ExpanderXSDK;
using System;

namespace ExpanderX
{
    /// <summary>
    /// 常用任务包模型，目前只有这一种任务包模型。
    /// </summary>
    [Serializable]
    public sealed class ComTaskPack : AbsTaskPack
    {
        /// <summary>
        /// 此通用任务包框架的名称。
        /// </summary>
        public override string Name { get { return "通用任务包模型"; } }

        /// <summary>
        /// 返回任务包的详情描述，一般是任务包内包含哪些匹配器及执行器的描述。
        /// </summary>
        /// <returns></returns>
        public override string Description() { return "一个通用的任务包模型的实例。"; }
    }
}
