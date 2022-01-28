using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace HateRepeatTool
{
    public class GenerateCodeBuilder
    {

        /// <summary>
        /// 数据库
        /// </summary>
        private Database? _database;
        /// <summary>
        /// 模版
        /// </summary>
        private List<Template> _templates;
        /// <summary>
        /// 过滤表
        /// </summary>
        private List<string> _filterTable;
        /// <summary>
        /// 下划线转驼峰
        /// </summary>
        private bool _isHump;
        /// <summary>
        /// 表名与文件名转换规则
        /// </summary>
        private string _nameRegex = "";
        /// <summary>
        /// 覆盖源文件
        /// </summary>
        private bool _overwrite;
        ///// <summary>
        ///// 根据表名新建文件夹
        ///// </summary>
        //private bool _newTableNameFolder;
        /// <summary>
        /// 文件类型
        /// </summary>
        private string _fileType = "cs";
        /// <summary>
        /// 命名空间
        /// </summary>
        private string? _nameSpace;
        /// <summary>
        /// 作用域
        /// </summary>
        public Dictionary<string, string> _scope;

        public GenerateCodeBuilder()
        {
            _templates = new List<Template>();
            _filterTable = new List<string>();
            _isHump = false;
            _overwrite = false;
            //_newTableNameFolder = false;
            _scope = new Dictionary<string, string>();
        }
        /// <summary>
        /// 设置数据库
        /// </summary>
        /// <param name="connection">数据库连接字符串</param>
        /// <param name="databaseType">数据库类型</param>
        /// <returns></returns>
        public GenerateCodeBuilder SetDatabase([NotNull] string connection, [NotNull] DatabaseType databaseType)
        {
            this._database = new Database
            {
                Connection = connection,
                Type = databaseType
            };
            return this;
        }

        /// <summary>
        /// 添加模版
        /// </summary>
        /// <param name="setTemplate"></param>
        /// <returns></returns>
        public GenerateCodeBuilder AddTemplate(Action<Template> setTemplate)
        {
            Template template = new Template();
            setTemplate(template);
            this._templates.Add(template);
            return this;
        }

        /// <summary>
        /// 添加模版
        /// </summary>
        /// <param name="templates">模版集合</param>
        /// <returns></returns>
        public GenerateCodeBuilder AddTemplate([NotNull] List<Template> templates)
        {
            this._templates.AddRange(templates);
            return this;
        }

        /// <summary>
        /// 过滤表
        /// </summary>
        /// <param name="tableNames">需要过滤的表名</param>
        /// <returns></returns>
        public GenerateCodeBuilder Filter(string[] tableNames)
        {
            this._filterTable.AddRange(tableNames);
            return this;
        }

        /// <summary>
        /// 下划线转驼峰
        /// </summary>
        /// <param name="isHump">下划线转驼峰</param>
        /// <returns></returns>
        public GenerateCodeBuilder UseUnderscoreToHump(bool isHump)
        {
            this._isHump = isHump;
            return this;
        }

        /// <summary>
        /// 表名与文件名转换规则
        /// </summary>
        /// <param name="regex">下划线转驼峰</param>
        /// <returns></returns>
        public GenerateCodeBuilder UseTableNameToFileNameRegex(string nameRegex)
        {
            this._nameRegex = nameRegex;
            return this;
        }

        /// <summary>
        /// 覆盖源文件
        /// </summary>
        /// <param name="overwrite">覆盖源文件</param>
        /// <returns></returns>
        public GenerateCodeBuilder UseOverwrite(bool overwrite = true)
        {
            this._overwrite = overwrite;
            return this;
        }

        ///// <summary>
        ///// 根据表名创建文件夹
        ///// </summary>
        ///// <param name="tableNameFolder">覆盖源文件</param>
        ///// <returns></returns>
        //public GenerateCodeBuilder UseNewTableNameFolder(bool tableNameFolder = true)
        //{
        //    this._newTableNameFolder = tableNameFolder;
        //    return this;
        //}

        /// <summary>
        /// 文件类型
        /// </summary>
        /// <param name="fileType">文件类型</param>
        /// <returns></returns>
        public GenerateCodeBuilder UseFileType(string fileType = "cs")
        {
            this._fileType = fileType;
            return this;
        }

        /// <summary>
        /// 命名空间
        /// </summary>
        /// <param name="nameSpace">命名空间</param>
        /// <returns></returns>
        public GenerateCodeBuilder UseNameSpace(string nameSpace)
        {
            if (string.IsNullOrWhiteSpace(nameSpace))
                throw new ArgumentException($"{nameof(nameSpace)}命名空间不能为空");
            this._nameSpace = nameSpace;
            return this;
        }

        /// <summary>
        /// 作用域
        /// </summary>
        /// <param name="scopeNames">作用域名称</param>
        /// <param name="regular">分组规则 此方法用于筛选表名</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public GenerateCodeBuilder AddScope(string scopeNames, string regular)
        {
            this._scope.Add(scopeNames, regular);
            return this;
        }

        /// <summary>
        /// 作用域
        /// </summary>
        /// <param name="scpoes">作用域 key=作用域名称 val=分组规则 正则表达式 用于筛选表名</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public GenerateCodeBuilder AddScope([NotNull] Dictionary<string, string> scpoes)
        {
            foreach (var scope in scpoes)
            {
                _scope.Add(scope.Key, scope.Value);
            }
            return this;
        }

        /// <summary>
        /// 构建
        /// </summary>
        /// <returns></returns>
        public IGenerateCode Build()
        {
            if (this._database == null)
                throw new InvalidOperationException($"数据库不存在“{nameof(this._database)}”");
            if (string.IsNullOrWhiteSpace(_nameSpace))
                throw new InvalidOperationException($"命名空间不能为空“{nameof(this._nameSpace)}”");
            var generateCode = new GenerateCode(this._database, this._templates, this._filterTable, this._nameSpace);
            generateCode.IsHump = this._isHump;
            generateCode.Overwrite = this._overwrite;
            generateCode.NameRegex = this._nameRegex;
            generateCode.FileType = this._fileType;
            generateCode.Scope = this._scope;
            //generateCode.NewTableNameFolder = this._newTableNameFolder;
            return generateCode;
        }
    }
}