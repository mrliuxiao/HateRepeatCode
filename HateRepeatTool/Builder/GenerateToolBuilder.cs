using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace HateRepeatTool
{
    public class GenerateToolBuilder
    {
        /// <summary>
        /// 命令参数
        /// </summary>
        private List<string> _commands;

        public GenerateToolBuilder()
        {
            _commands = new List<string>();
        }

        /// <summary>
        /// 使用命令行工具
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public GenerateToolBuilder UseCommand(string[] args)
        {
            _commands.AddRange(args);
            return this;
        }

        /// <summary>
        /// 设置数据库
        /// </summary>
        /// <param name="Connection"></param>
        /// <param name="DatabaseType"></param>
        /// <returns></returns>
        public GenerateToolBuilder SetDatabase([NotNull] string Connection, [NotNull] DatabaseType DatabaseType)
        {
            var command = new string[] { "-db", Connection, "-dt", DatabaseType.ToInt64().ToString() };
            _commands.AddRange(command);
            return this;
        }

        /// <summary>
        /// 添加模版
        /// </summary>
        /// <param name="setTemplate"></param>
        /// <returns></returns>
        public GenerateToolBuilder AddTemplate(Action<Template> setTemplate)
        {
            Template template = new Template();
            setTemplate(template);
            var command = template.ToCommand();
            _commands.AddRange(command);
            return this;
        }

        /// <summary>
        /// 添加模版
        /// </summary>
        /// <param name="templates"></param>
        /// <returns></returns>
        public GenerateToolBuilder AddTemplate([NotNull] List<Template> templates)
        {
            templates.ForEach(template =>
            {
                _commands.AddRange(template.ToCommand());
            });
            return this;
        }

        /// <summary>
        /// 过滤表
        /// </summary>
        /// <param name="tableNames">需要过滤的表名</param>
        /// <returns></returns>
        public GenerateToolBuilder Filter(string[] tableNames)
        {
            List<string> command = new List<string>();
            tableNames.ToList().ForEach(table =>
            {
                command.Add("-f");
                command.Add(table);
            });
            _commands.AddRange(command);
            return this;
        }

        /// <summary>
        /// 下划线转驼峰
        /// </summary>
        /// <param name="IsHump"></param>
        /// <returns></returns>
        public GenerateToolBuilder UseUnderscoreToHump(bool isHump = true)
        {
            if (isHump) _commands.Add("-hu");
            else _commands.Remove("-hu");
            return this;
        }

        /// <summary>
        /// 表名与文件名转换规则
        /// </summary>
        /// <param name="regex">下划线转驼峰</param>
        /// <returns></returns>
        public GenerateToolBuilder UseTableNameToFileNameRegex(string nameRegex)
        {
            this._commands.AddRange(new string[] { "-nr", nameRegex });
            return this;
        }

        /// <summary>
        /// 下划线转驼峰
        /// </summary>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        public GenerateToolBuilder UseOverwrite(bool overwrite = true)
        {
            if (overwrite) _commands.Add("-o");
            else _commands.Remove("-o");
            return this;
        }

        ///// <summary>
        ///// 根据表名新建文件夹
        ///// </summary>
        ///// <param name="overwrite"></param>
        ///// <returns></returns>
        //public GenerateToolBuilder UseTableNameFolder(bool newTableNameFolder = true)
        //{
        //    if (newTableNameFolder) _commands.Add("-nf");
        //    else _commands.Remove("-nf");
        //    return this;
        //}

        /// <summary>
        /// 文件类型
        /// </summary>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        public GenerateToolBuilder UseFileType(string fileType = "cs")
        {
            this._commands.AddRange(new string[] { "-ft", fileType });
            return this;
        }

        /// <summary>
        /// 命名空间
        /// </summary>
        /// <param name="nameSpace"></param>
        /// <returns></returns>
        public GenerateToolBuilder UseNameSpace(string nameSpace)
        {
            if (string.IsNullOrWhiteSpace(nameSpace))
                throw new ArgumentException($"{nameof(nameSpace)}命名空间不能为空");
            this._commands.AddRange(new string[] { "-ns", nameSpace });
            return this;
        }

        /// <summary>
        /// 作用域
        /// </summary>
        /// <param name="scopeNames">作用域名称</param>
        /// <param name="regular">分组规则 正则表达式 用于筛选表名</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public GenerateToolBuilder AddScope(string scopeNames, string regular)
        {
            this._commands.AddRange(new string[] { "-sc", $"{scopeNames}|{regular}" });
            return this;
        }

        /// <summary>
        /// 作用域
        /// </summary>
        /// <param name="scpoes">作用域 key=作用域名称 val=分组规则 正则表达式 用于筛选表名</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public GenerateToolBuilder AddScope([NotNull] Dictionary<string, string> scpoes)
        {
            foreach (var scp in scpoes)
            {
                this._commands.AddRange(new string[] { "-sc", $"{scp.Key}|{scp.Value}" });
            }
            return this;
        }

        /// <summary>
        /// 运行命令行工具
        /// </summary>
        /// <returns></returns>
        public void Run()
        {
            CommandLineApplication.Execute<Command>(_commands.ToArray());
        }

        /// <summary>
        /// 运行命令行工具
        /// </summary>
        /// <returns></returns>
        public async Task RunAsync()
        {
            await CommandLineApplication.ExecuteAsync<Command>(_commands.ToArray());
        }

    }
}