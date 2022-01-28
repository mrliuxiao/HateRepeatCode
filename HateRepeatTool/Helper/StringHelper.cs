using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace HateRepeatTool
{
    public static class StringHelper
    {
        /// <summary>
        /// 下划线转驼峰
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string LineToHump(this string name)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var s in name.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries))
            {
                builder.Append(Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(s));
            }

            return builder.ToString();
        }

        /// <summary>
        /// 连接路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string JoinPath(this string path, string joinPath)
        {
            return Path.Combine(path, joinPath);
        }

        /// <summary>
        /// 首字母小写
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToStartUpper(this string str)
        {
            return str.Substring(0, 1).ToLower() + str.Substring(1);
        }
    }
}