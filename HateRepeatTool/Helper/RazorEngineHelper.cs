using RazorEngine;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HateRepeatTool
{
    public class RazorEngineHelper
    {
        /// <summary>
        /// 根据模板生成代码文件
        /// </summary>
        /// <param name="razorInfo">生成信息</param>
        public static void Generate(RazorInfo razorInfo, bool overwrite = false)
        {
            if (string.IsNullOrWhiteSpace(razorInfo.TemplateFilePath) || string.IsNullOrWhiteSpace(razorInfo.ToPath))
                return;
            var templateFile = File.ReadAllText(razorInfo.TemplateFilePath);

            StringBuilder plus = new StringBuilder();
            string templateFileResult = Engine.Razor.RunCompile(templateFile, Guid.NewGuid().ToString(), null, razorInfo);
            templateFileResult = templateFileResult.Replace("&quot;", "\"");
            plus.AppendLine("//此代码由工具自动生成");
            plus.Append(templateFileResult);
            if (File.Exists(razorInfo.ToPath.JoinPath(razorInfo.FileName!)) && !overwrite)
            {
                Console.WriteLine($"禁止覆盖文件，已跳过{razorInfo.FileName}");
                return;
            }
            if (!File.Exists(razorInfo.ToPath))
            {
                System.IO.Directory.CreateDirectory(razorInfo.ToPath);
            }
            File.WriteAllText(razorInfo.ToPath.JoinPath(razorInfo.FileName!), templateFileResult);
            Console.WriteLine("已保存文件:" + razorInfo.ToPath.JoinPath(razorInfo.FileName!));
        }

        /// <summary>
        /// 根据模板批量生成代码文件
        /// </summary>
        /// <param name="generateConfig">生成配置列表</param>
        public static void GenerateRange(List<RazorInfo>  razorInfos, bool overwrite = false)
        {
            foreach (var razor in razorInfos)
            {
                Generate(razor, overwrite);
            }
        }
    }
}
