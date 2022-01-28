using System;

namespace HateRepeatTool
{
    /// <summary>
    /// 模版
    /// </summary>
    public class Template
    {

        /// <summary>
        /// 模版路径
        /// </summary>
        private string? _templatePath;
        /// <summary>
        /// 模版路径
        /// </summary>
        public string TemplatePath { get { return _templatePath ?? string.Empty; } }
        /// <summary>
        /// 生成文件路径
        /// </summary>
        private string? _toPath;
        /// <summary>
        /// 生成文件路径
        /// </summary>
        public string ToPath { get { return _toPath ?? string.Empty; } }
        /// <summary>
        /// 追加到文件名开始字符
        /// </summary>
        private string? _startChar;
        /// <summary>
        /// 追加到文件名开始字符
        /// </summary>
        public string StartChar { get { return _startChar ?? string.Empty; } }
        /// <summary>
        /// 追加到文件名结束字符
        /// </summary>
        private string? _endChar;
        /// <summary>
        /// 追加到文件名结束字符
        /// </summary>
        public string EndChar { get { return _endChar ?? string.Empty; } }
        /// <summary>
        /// 新建文件夹
        /// </summary>
        private string? _newFolder;
        /// <summary>
        /// 新建文件夹
        /// </summary>
        public string NewFolder { get { return _newFolder ?? string.Empty; } }
        /// <summary>
        /// 根据表名新建文件夹 yes/no
        /// </summary>
        private string _newTableNameFolder = "no";
        /// <summary>
        /// 根据表名新建文件夹 yes/no
        /// </summary>
        public bool NewTableNameFolder
        {
            get { return _newTableNameFolder.ToLower() == "yes"; }
            //set { _newTableNameFolder = value ? "yes" : "no"; }
        }


        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="index">0 模版路径，1 生成路径，2 追加名称前字符，3 追加名称后字符，4 新建文件夹名称</param>
        /// <returns></returns>
        public string this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _templatePath ?? string.Empty;
                    case 1:
                        return _toPath ?? string.Empty;
                    case 2:
                        return _startChar ?? string.Empty;
                    case 3:
                        return _endChar ?? string.Empty;
                    case 4:
                        return _newFolder ?? string.Empty;
                    case 5:
                        return _newTableNameFolder?.ToLower() == "yes" ? "yes" : "no";
                    default:
                        return string.Empty;
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _templatePath = value ?? string.Empty;
                        break;
                    case 1:
                        _toPath = value ?? string.Empty;
                        break;
                    case 2:
                        _startChar = value ?? string.Empty;
                        break;
                    case 3:
                        _endChar = value ?? string.Empty;
                        break;
                    case 4:
                        _newFolder = value ?? string.Empty;
                        break;
                    case 5:
                        _newTableNameFolder = value.ToLower() == "yes" ? "yes" : "no";
                        break;
                    default:
                        break;
                }
            }
        }


        /// <summary>
        /// 模版路径
        /// </summary>
        /// <param name="templatePath"></param>
        /// <param name="toPath"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public Template UsePath(string templatePath, string toPath)
        {
            if (string.IsNullOrWhiteSpace(templatePath))
                throw new System.Exception($"{nameof(templatePath)}模版路径不能为空");
            if (string.IsNullOrWhiteSpace(toPath))
                throw new System.Exception($"{nameof(toPath)}生成路径不能为空");
            this._templatePath = templatePath;
            this._toPath = toPath;
            return this;
        }


        /// <summary>
        /// 生成文件名规则
        /// </summary>
        /// <param name="startChar">追加到文件名前字符</param>
        /// <param name="endChar">追加到文件名后字符</param>
        public Template UseFileNameRules(string startChar, string endChar = "")
        {
            this._startChar = startChar;
            this._endChar = endChar;
            return this;
        }

        /// <summary>
        /// 新建文件夹
        /// </summary>
        /// <param name="folderName">文件夹名称</param>
        public Template UseNewFolder(string folderName)
        {
            this._newFolder = folderName;
            return this;
        }

        /// <summary>
        /// 根据表名新建文件夹
        /// </summary>
        /// <param name="newTableNameFolder"></param>
        public Template UseNewTableNameFolder(bool newTableNameFolder = true)
        {
            this._newTableNameFolder = newTableNameFolder ? "yes" : "no";
            return this;
        }

        /// <summary>
        /// 转换为命令
        /// </summary>
        /// <returns></returns>
        public string[] ToCommand()
        {
            this.Check();
            var command = new string[] { "-t", $"{_templatePath}|{_toPath}|{_startChar}|{_endChar}|{_newFolder}|{_newTableNameFolder}" };
            return command;
        }

        /// <summary>
        /// 验证数据可用性
        /// </summary>
        /// <param name="folderName">文件夹名称</param>
        public Template Check()
        {
            if (string.IsNullOrWhiteSpace(this._templatePath))
                throw new ArgumentException($"{nameof(_templatePath)}模版路径不允许为空");
            if (string.IsNullOrWhiteSpace(this._toPath))
                throw new ArgumentException($"{nameof(_toPath)}生成路径不允许为空");
            return this;
        }
    }

}