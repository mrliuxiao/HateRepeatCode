using HateRepeatTool;

namespace TestGenerateCode
{
    class Program
    {
        public static async Task Main(string[] args)
        {

            Console.WriteLine("最讨厌重复劳动");

            if (args.Contains("-g") || true)
            {
                await new GenerateToolBuilder()
                    .SetDatabase("Server=localhost; Port=3306; Database=admindb; Uid=lucio; Pwd=lucio; Charset=utf8mb4;", DatabaseType.MySql)
                    //设置模版路径和输出路径
                    .AddTemplate(temp =>
                    {
                        temp.UsePath("./RazorTemplates/Repository/IRepositoryTemplate.cshtml", "Repositorys")
                        .UseNewTableNameFolder()
                        .UseFileNameRules("I", "Repository");
                    })
                    .AddTemplate(temp =>
                    {
                        temp.UsePath("./RazorTemplates/Service/IServiceTemplate.cshtml", "Services")
                        .UseNewTableNameFolder()
                        .UseFileNameRules("I", "Service")
                        .UseNewFolder("新建文件夹");
                    })
                    .AddTemplate(temp =>
                    {
                        temp.UsePath("./RazorTemplates/Controller/ControllerTemplate.cshtml", "Controllers")
                        .UseFileNameRules("", "Controller")
                        .UseNewTableNameFolder(false);
                    })
                    .AddTemplate(temp =>
                    {
                        temp.UsePath("./RazorTemplates/FreeSqlEntity/EntityTemplate.cshtml", "Entitys")
                        .UseFileNameRules("", "Entity")
                        .UseNewTableNameFolder(false);
                    })
                    //过滤不需要生成的表
                    .Filter(new string[] { "ad_api", "ad_dictionary", "ad_employee_organization", "ad_login_log" })
                    //文件名下划线转驼峰
                    .UseUnderscoreToHump(true)
                    //命名空间
                    .UseNameSpace(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Namespace!)
                    //生成文件类型
                    .UseFileType("cs")
                    //作用域分组 按照作用域新建文件夹
                    .AddScope("Admin", "ad")
                    .AddScope("PeGroup", "pe")
                    //表名与文件名转换规则
                    .UseTableNameToFileNameRegex("(?<=_).*")
                    //使用命令工具
                    .UseCommand(args)
                    .RunAsync();

            }

        }
    }
}