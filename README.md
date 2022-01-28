# 讨厌重复的代码

#### 介绍
命令行工具，代码生成器，支持大部分数据库

#### 软件架构
软件架构说明


#### 安装教程

1.  直接引用源码项目
2.  引用nuget包
3.  全局使用

#### 使用说明

1.  在项目内使用命令行工具  dotnet run -g 

```
await new GenerateToolBuilder()
    //使用命令工具
    .UseCommand(args)
    .RunAsync();
```

2.  在项目内直接载入配置 通过命令 dotnet run -g 即可生成，也可通过命令直接增加参数

```
await new GenerateToolBuilder()
	.SetDatabase("Server=localhost; Port=3306; Database=admindb; Uid=lucio; Pwd=lucio; Charset=utf8mb4;", DatabaseType.MySql)
	//设置模版路径和输出路径
	.AddTemplate(temp =>
	{
		temp.UsePath("./RazorTemplates/Repository/IRepositoryTemplate.cshtml", "Repository")
		.UseNewTableNameFolder()
		.UseFileNameRules("I", "Repository");
	})
	.AddTemplate(temp =>
	{
		temp.UsePath("./RazorTemplates/Service/IServiceTemplate.cshtml", "Service")
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
```

3.  全局命令行工具 添加环境变量 或直接使用绝对路径 查看帮助命令haterepeat -h

#### 参与贡献

1.  Fork 本仓库
2.  新建 Feat_xxx 分支
3.  提交代码
4.  新建 Pull Request
