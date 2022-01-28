using HateRepeatTool;

Console.WriteLine("最讨厌重复劳动");

if (args.Length == 0) return;
await new GenerateToolBuilder()
    //使用命令工具
    .UseCommand(args)
    .RunAsync();