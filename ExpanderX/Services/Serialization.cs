using CommonUtils;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace ExpanderX
{
    internal static class Serialization
    {
        private static readonly IFormatter fmt = new BinaryFormatter();

        /// <summary>
        /// 将类的实例保存为文件。
        /// </summary>
        /// <typeparam name="_T">类型</typeparam>
        /// <param name="instance">类的实例</param>
        /// <param name="savePath">完整保存路径</param>
        /// <returns>成功返回 true，失败返回 false</returns>
        public static bool SaveInstFile<_T>(_T instance, string savePath)
        {
            try
            {
                FileInfo sp = new FileInfo(savePath);
                if (sp.Exists)
                    return false;
                using (Stream fs = sp.Create())
                {
                    fmt.Serialize(fs, instance);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 复制类的实例。
        /// </summary>
        /// <typeparam name="_T">类型</typeparam>
        /// <param name="instance">类的实例</param>
        /// <returns>复制的新实例</returns>
        public static _T CpInstance<_T>(_T instance)
        {
            try
            {
                using (Stream ms = new MemoryStream())
                {
                    fmt.Serialize(ms, instance);
                    ms.Seek(0, SeekOrigin.Begin);
                    return (_T)fmt.Deserialize(ms);
                }
            }
            catch (Exception e)
            {
                USER.MessageBox(IntPtr.Zero, e.Message, "序列化失败", MB.MB_TOPMOST);
                return default;
            }
        }
    }
}
