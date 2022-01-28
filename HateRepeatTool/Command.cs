using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HateRepeatTool
{
    public class Command
    {

        [Option(Description = "生成代码", ShortName = "g", LongName = "GenerateCode")]
        public bool? GenerateCode { get; }

        [Option(Description = "数据库连接字符串", ShortName = "db", LongName = "DbConnection")]
        public string? DbConnection { get; }

        [Option(LongName = "DbType", ShortName = "dt", Description = "数据库类型 MySql = 0, SqlServer = 1, Oracle = 3")]
        public int? DbType { get; }

        [Option(Description = "模版路径与输出路径 格式 模版路径|生成文件路径|文件名开始位置字符|文件名结束位置字符|新建文件夹名称|根据表名新建文件夹(yes/no)", ShortName = "t", LongName = "Template")]
        public string[] Template { get; } = new string[0];

        [Option(Description = "过滤表", ShortName = "f", LongName = "FilterTable")]
        public string[] FilterTable { get; } = new string[0];

        [Option(Description = "下划线转驼峰", ShortName = "hu", LongName = "Hump")]
        public bool Hump { get; } = false;

        [Option(Description = "表名与文件名转换规则", ShortName = "nr", LongName = "NameRegex")]
        public string? NameRegex { get; } = "";

        [Option(Description = "覆盖源文件", ShortName = "o", LongName = "Overwrite")]
        public bool Overwrite { get; } = false;

        //[Option(Description = "根据表名新建文件夹", ShortName = "nf", LongName = "NewTableNameFoleder")]
        //public bool NewTableNameFoleder { get; } = false;

        [Option(Description = "生成文件类型 默认cs", ShortName = "ft", LongName = "FileType")]
        public string FileType { get; } = "cs";

        [Option(Description = "命名空间", ShortName = "ns", LongName = "NameSpace")]
        public string? NameSpace { get; }

        [Option(Description = "作用域 分组规则", ShortName = "sc", LongName = "Scope")]
        public string[] Scope { get; } = new string[0];

        [Option(Description = "控制台打印Log", ShortName = "l", LongName = "Log")]
        public bool Log { get; } = false;
        private void OnExecute()
        {
            if (Log == true)
            {
                Console.WriteLine("执行命令");
                Console.WriteLine("生成代码" + GenerateCode);
                Console.WriteLine("当前数据库" + DbConnection);
                Console.WriteLine("数据库类型" + DbType);
                Template.ToList().ForEach(x => Console.WriteLine("模版路径" + x));
            }

            if (GenerateCode == false) return;

            if (string.IsNullOrWhiteSpace(DbConnection))
                throw new Exception("数据库连接字符串为空，使用“-db”参数。");
            if (DbType == null)
                throw new Exception("数据库类型为空，使用“-dt”参数。");
            if (Template.Length == 0)
                throw new Exception("没有可用的模版文件，使用“-t”参数。");
            if (string.IsNullOrWhiteSpace(NameSpace))
                throw new Exception("命名空间为空，使用“-ns”参数。");

            //模版
            List<Template> templates = new List<Template>();
            Template.ToList().ForEach((x) =>
            {
                if (!x.Contains("|"))
                    throw new Exception($"模版格式错误！错误值：{x}，正确格式：模版路径|生成文件路径， 请检查参数TemplatePath");
                var pt = x.Split("|");
                //var temp = new Template { TemplatePath = pt[0], ToPath = pt[1], StartChar = pt[2], EndChar = pt[3], NewFolder = pt[4] };
                var temp = new Template();
                for (int i = 0; i < pt.Length; i++)
                {
                    temp[i] = pt[i];
                }
                templates.Add(temp);
            });

            //作用域
            Dictionary<string, string> scopes = new Dictionary<string, string>();
            Scope.ToList().ForEach(f =>
            {
                if (!f.Contains("|"))
                    throw new Exception($"作用域格式错误！错误值：{f}，正确格式：作用域名称|正则表达式， 请检查参数Scope");
                var sc = f.Split("|");
                scopes.Add(sc[0], sc[1]);
            });

            Console.WriteLine("正在生成代码");

            IGenerateCode generateCode = new GenerateCodeBuilder()
                .SetDatabase(DbConnection, (DatabaseType)DbType)//设置数据库
                .AddTemplate(templates)//设置模版路径
                .Filter(FilterTable)//过滤表格
                .UseUnderscoreToHump(Convert.ToBoolean(Hump))//下划线转驼峰
                .UseTableNameToFileNameRegex(NameRegex!)//下划线转驼峰
                .UseOverwrite(Convert.ToBoolean(Overwrite))//覆盖源文件
                //.UseNewTableNameFolder(Convert.ToBoolean(NewTableNameFoleder))//根据表名新建文件夹
                .UseFileType(FileType)//生成文件类型
                .UseNameSpace(NameSpace)//命名空间
                .AddScope(scopes)//作用域分组
                .Build();

            generateCode.Run();
            Console.WriteLine("完成");
        }
    }


}