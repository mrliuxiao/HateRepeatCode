using FreeSql;
using FreeSql.DataAnnotations;
using FreeSql.DatabaseModel;
using FreeSql.Internal.CommonProvider;
using McMaster.Extensions.CommandLineUtils;
using MySql.Data.MySqlClient;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HateRepeatTool
{
    /// <summary>
    /// 生成代码
    /// </summary>
    public class GenerateCode : IGenerateCode
    {
        /// <summary>
        /// 数据库
        /// </summary>
        private readonly Database _database;
        /// <summary>
        /// db
        /// </summary>
        private readonly IFreeSql _freeSql;
        /// <summary>
        /// 模版
        /// </summary>
        private List<Template> _templates;
        /// <summary>
        /// 过滤表
        /// </summary>
        private List<string> _filterTable;
        /// <summary>
        /// 表名与文件名转换规则
        /// </summary>
        public string NameRegex { get; set; }
        /// <summary>
        /// 下划线转驼峰 默认false
        /// </summary>
        public bool IsHump { get; set; } = false;
        /// <summary>
        /// 覆盖源文件 默认false
        /// </summary>
        public bool Overwrite { get; set; } = false;
        ///// <summary>
        ///// 根据表名创建文件夹
        ///// </summary>
        //public bool NewTableNameFolder { get; set; } = false;
        /// <summary>
        /// 文件类型 默认cs
        /// </summary>
        public string FileType { get; set; } = "cs";
        /// <summary>
        /// 命名空间
        /// </summary>
        public string NameSpace { get; set; }
        /// <summary>
        /// 作用域
        /// </summary>
        public Dictionary<string, string> Scope { get; set; }

        public GenerateCode(
            [NotNull] Database database,
            [NotNull] List<Template> templates,
            [NotNull] List<string> filterTable,
            [NotNull] string nameSpace,
            [NotNull] string nameRegex = "")
        {
            this._database = database;
            this._templates = templates;
            this._filterTable = filterTable;
            this.NameSpace = nameSpace;
            this.NameRegex = nameRegex;
            this.Scope = new Dictionary<string, string>();
            this._freeSql = _database.CreateDb();
        }

        public void Run()
        {
            List<RazorInfo> razorInfos = GetRazorInfos();
            if (razorInfos.Count == 0)
            {
                Console.WriteLine("暂无匹配的数据表");
                return;
            }
            RazorEngineHelper.GenerateRange(razorInfos, this.Overwrite);
        }

        /// <summary>
        /// 获取生成数据
        /// </summary>
        /// <returns></returns>
        private List<RazorInfo> GetRazorInfos()
        {
            //获取表结构
            var tables = _freeSql.DbFirst.GetTablesByDatabase().FindAll(table => !_filterTable.Contains(table.Name));
            var razorInfos = new List<RazorInfo>();

            _templates.ForEach(temp =>
            {
                tables.ForEach(table =>
                {
                    var razor = new RazorInfo
                    {
                        NameSpace = NameSpace,
                        DbTableInfo = table,
                        Description = table.Comment,
                        Name = GetClassName(table.Name),
                        FileName = $"{temp.StartChar}{GetClassName(table.Name)}{temp.EndChar}.{this.FileType}",
                        TemplateFilePath = temp.TemplatePath ?? string.Empty,
                        ToPath = temp.ToPath ?? string.Empty,
                        Template = temp,
                        Scope = Scope.FirstOrDefault(fun => Regex.IsMatch(table.Name, fun.Value)).Key,
                        Attribute = GetTableAttribute(table.Name),
                        MySqlEnumSet = GetMySqlEnumSetDefine(table),
                        Columns = table.Columns.Select(c => new ColumnInfo
                        {
                            Name = GetColumnName(c.Name),
                            Description = c.Comment,
                            Type = _freeSql.DbFirst.GetCsType(c),
                            EntityType = GetCsType(table.Name, c),
                            Attribute = GetColumnAttribute(c)
                        }).ToList(),
                    };
                    // 根据作用域新建文件夹
                    if (!string.IsNullOrWhiteSpace(razor.Scope))
                        razor.ToPath = razor.ToPath?.JoinPath(razor.Scope);
                    // 根据表名新建文件夹
                    if (temp.NewTableNameFolder)
                        razor.ToPath = razor.ToPath?.JoinPath(GetClassName(table.Name));
                    // 根据模版新建文件夹
                    if (!string.IsNullOrWhiteSpace(temp.NewFolder))
                        razor.ToPath = razor.ToPath?.JoinPath(temp.NewFolder);
                    razorInfos.Add(razor);
                });
            });
            return razorInfos;
        }

        /// <summary>
        /// 获取文件名称
        /// </summary>
        /// <returns></returns>
        private string GetClassName(string name)
        {
            if (!string.IsNullOrWhiteSpace(NameRegex))
                name = Regex.Match(name, NameRegex).Value;
            if (this.IsHump && name.Contains("_"))
                return StringHelper.LineToHump(name);
            return name;
        }
        /// <summary>
        /// 获取名称
        /// </summary>
        /// <returns></returns>
        private string GetColumnName(string name)
        {
            if (this.IsHump && name.Contains("_"))
                return StringHelper.LineToHump(name);
            return name;
        }

        #region 特性
        /// <summary>
        /// 获取表实体特性
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private string GetTableAttribute(string tableName)
        {
            var sb = new List<string>();
            if (GetClassName(tableName) != tableName)
            {
                sb.Add("Name = \"" + tableName + "\"");
            }
            //禁用迁移
            sb.Add("DisableSyncStructure = true");
            return "[Table(" + string.Join(", ", sb) + ")]";
        }
        /// <summary>
        /// 获取列实体特性
        /// </summary>
        /// <param name="col"></param>
        /// <param name="isInsertValueSql"></param>
        /// <returns></returns>
        public string GetColumnAttribute(DbColumnInfo col, bool isInsertValueSql = false)
        {
            var sb = new List<string>();

            if (GetColumnName(col.Name) != col.Name)
                sb.Add("Name = \"" + col.Name + "\"");

            if (col.CsType != null)
            {
                var dbinfo = _freeSql.CodeFirst.GetDbInfo(col.CsType);
                if (dbinfo != null && string.Compare(dbinfo.dbtypeFull.Replace("NOT NULL", "").Trim(), col.DbTypeTextFull, true) != 0)
                {
                    #region StringLength 反向
                    switch (_freeSql.Ado.DataType)
                    {
                        case DataType.MySql:
                        case DataType.OdbcMySql:
                            switch (col.DbTypeTextFull.ToLower())
                            {
                                case "longtext": sb.Add("StringLength = -2"); break;
                                case "text": sb.Add("StringLength = -1"); break;
                                default:
                                    var m_stringLength = Regex.Match(col.DbTypeTextFull, @"^varchar\s*\((\w+)\)$", RegexOptions.IgnoreCase);
                                    if (m_stringLength.Success) sb.Add($"StringLength = {m_stringLength.Groups[1].Value}");
                                    else sb.Add("DbType = \"" + col.DbTypeTextFull + "\"");
                                    break;
                            }
                            break;
                        case DataType.SqlServer:
                        case DataType.OdbcSqlServer:
                            switch (col.DbTypeTextFull.ToLower())
                            {
                                case "nvarchar(max)": sb.Add("StringLength = -2"); break;
                                default:
                                    var m_stringLength = Regex.Match(col.DbTypeTextFull, @"^nvarchar\s*\((\w+)\)$", RegexOptions.IgnoreCase);
                                    if (m_stringLength.Success) sb.Add($"StringLength = {m_stringLength.Groups[1].Value}");
                                    else sb.Add("DbType = \"" + col.DbTypeTextFull + "\"");
                                    break;
                            }
                            break;
                        case DataType.PostgreSQL:
                        case DataType.OdbcPostgreSQL:
                        case DataType.KingbaseES:
                        case DataType.OdbcKingbaseES:
                        case DataType.ShenTong:
                            switch (col.DbTypeTextFull.ToLower())
                            {
                                case "text": sb.Add("StringLength = -2"); break;
                                default:
                                    var m_stringLength = Regex.Match(col.DbTypeTextFull, @"^varchar\s*\((\w+)\)$", RegexOptions.IgnoreCase);
                                    if (m_stringLength.Success) sb.Add($"StringLength = {m_stringLength.Groups[1].Value}");
                                    else sb.Add("DbType = \"" + col.DbTypeTextFull + "\"");
                                    break;
                            }
                            break;
                        case DataType.Oracle:
                        case DataType.OdbcOracle:
                            switch (col.DbTypeTextFull.ToLower())
                            {
                                case "nclob": sb.Add("StringLength = -2"); break;
                                default:
                                    var m_stringLength = Regex.Match(col.DbTypeTextFull, @"^nvarchar2\s*\((\w+)\)$", RegexOptions.IgnoreCase);
                                    if (m_stringLength.Success) sb.Add($"StringLength = {m_stringLength.Groups[1].Value}");
                                    else sb.Add("DbType = \"" + col.DbTypeTextFull + "\"");
                                    break;
                            }
                            break;
                        case DataType.Dameng:
                        case DataType.OdbcDameng:
                            switch (col.DbTypeTextFull.ToLower())
                            {
                                case "text": sb.Add("StringLength = -2"); break;
                                default:
                                    var m_stringLength = Regex.Match(col.DbTypeTextFull, @"^nvarchar2\s*\((\w+)\)$", RegexOptions.IgnoreCase);
                                    if (m_stringLength.Success) sb.Add($"StringLength = {m_stringLength.Groups[1].Value}");
                                    else sb.Add("DbType = \"" + col.DbTypeTextFull + "\"");
                                    break;
                            }
                            break;
                        case DataType.Sqlite:
                            switch (col.DbTypeTextFull.ToLower())
                            {
                                case "text": sb.Add("StringLength = -2"); break;
                                default:
                                    var m_stringLength = Regex.Match(col.DbTypeTextFull, @"^nvarchar\s*\((\w+)\)$", RegexOptions.IgnoreCase);
                                    if (m_stringLength.Success) sb.Add($"StringLength = {m_stringLength.Groups[1].Value}");
                                    else sb.Add("DbType = \"" + col.DbTypeTextFull + "\"");
                                    break;
                            }
                            break;
                        case DataType.MsAccess:
                            switch (col.DbTypeTextFull.ToLower())
                            {
                                case "longtext": sb.Add("StringLength = -2"); break;
                                default:
                                    var m_stringLength = Regex.Match(col.DbTypeTextFull, @"^varchar\s*\((\w+)\)$", RegexOptions.IgnoreCase);
                                    if (m_stringLength.Success) sb.Add($"StringLength = {m_stringLength.Groups[1].Value}");
                                    else sb.Add("DbType = \"" + col.DbTypeTextFull + "\"");
                                    break;
                            }
                            break;
                    }
                    #endregion
                }
                if (col.IsPrimary)
                    sb.Add("IsPrimary = true");
                if (col.IsIdentity)
                    sb.Add("IsIdentity = true");

                if (dbinfo != null && dbinfo.isnullable != col.IsNullable)
                {
                    var cstype = _freeSql.DbFirst.GetCsType(col);
                    if (col.IsNullable && cstype.Contains("?") == false && col.CsType.IsValueType)
                        sb.Add("IsNullable = true");
                    if (col.IsNullable == false && (cstype.Contains("?") == true || cstype == "string"))
                        sb.Add("IsNullable = false");
                }

                if (isInsertValueSql)
                {
                    var defval = GetColumnDefaultValue(col, false);
                    if (defval == null) //c#默认属性值，就不需要设置 InsertValueSql 了
                    {
                        defval = GetColumnDefaultValue(col, true);
                        if (defval != null)
                        {
                            sb.Add("InsertValueSql = \"" + defval.Replace("\"", "\\\"") + "\"");
                            //sb.Add("CanInsert = false");
                        }
                    }
                    //else
                    //sb.Add("CanInsert = false");
                }
            }
            if (sb.Any() == false) return null;
            return "[Column(" + string.Join(", ", sb) + ")]";
        }

        /// <summary>
        /// 获取列默认值
        /// </summary>
        /// <param name="col"></param>
        /// <param name="isInsertValueSql"></param>
        /// <returns></returns>
        public string GetColumnDefaultValue(DbColumnInfo col, bool isInsertValueSql)
        {
            var defval = col.DefaultValue?.Trim();
            if (string.IsNullOrEmpty(defval)) return null;
            var cstype = col.CsType.NullableTypeOrThis();
            if (_freeSql.Ado.DataType == DataType.SqlServer || _freeSql.Ado.DataType == DataType.OdbcSqlServer)
            {
                if (defval.StartsWith("((") && defval.EndsWith("))")) defval = defval.Substring(2, defval.Length - 4);
                else if (defval.StartsWith("('") && defval.EndsWith("')")) defval = defval.Substring(2, defval.Length - 4).Replace("''", "'");
                else if (defval.StartsWith("(") && defval.EndsWith(")")) defval = defval.Substring(1, defval.Length - 2);
                else return null;
                if (defval.StartsWith("N'") && defval.EndsWith("'")) defval = defval.Substring(1);
                if (cstype == typeof(Guid) && string.Compare(defval, "newid()", true) == 0) return $"Guid.NewGuid()";
                if (cstype == typeof(string) && string.Compare(defval, "newid()", true) == 0) return $"Guid.NewGuid().ToString().ToUpper()";
                if (defval == "NULL") return null;
            }
            if ((cstype == typeof(string) && defval.StartsWith("'") && defval.EndsWith("'::character varying") ||
                cstype == typeof(Guid) && defval.StartsWith("'") && defval.EndsWith("'::uuid")
                ) && (_freeSql.Ado.DataType == DataType.PostgreSQL || _freeSql.Ado.DataType == DataType.OdbcPostgreSQL ||
                    _freeSql.Ado.DataType == DataType.KingbaseES || _freeSql.Ado.DataType == DataType.OdbcKingbaseES ||
                    _freeSql.Ado.DataType == DataType.ShenTong))
            {
                defval = defval.Substring(1, defval.LastIndexOf("'::") - 1).Replace("''", "'");
            }
            else if (defval.StartsWith("'") && defval.EndsWith("'"))
            {
                defval = defval.Substring(1, defval.Length - 2).Replace("''", "'");
                if (_freeSql.Ado.DataType == DataType.MySql || _freeSql.Ado.DataType == DataType.OdbcMySql) defval = defval.Replace("\\\\", "\\");
            }
            if (cstype.IsNumberType() && decimal.TryParse(defval, out var trydec))
            {
                if (isInsertValueSql) return defval;
                if (cstype == typeof(float)) return defval + "f";
                if (cstype == typeof(double)) return defval + "d";
                if (cstype == typeof(decimal)) return defval + "M";
                return defval;
            }
            if (cstype == typeof(Guid) && Guid.TryParse(defval, out var tryguid)) return isInsertValueSql ? (_freeSql.Select<TestTb>() as Select0Provider)._commonUtils.FormatSql("{0}", defval) : $"Guid.Parse({LiteralString(defval)})";
            if (cstype == typeof(DateTime) && DateTime.TryParse(defval, out var trydt)) return isInsertValueSql ? (_freeSql.Select<TestTb>() as Select0Provider)._commonUtils.FormatSql("{0}", defval) : $"DateTime.Parse({LiteralString(defval)})";
            if (cstype == typeof(TimeSpan) && TimeSpan.TryParse(defval, out var tryts)) return isInsertValueSql ? (_freeSql.Select<TestTb>() as Select0Provider)._commonUtils.FormatSql("{0}", defval) : $"TimeSpan.Parse({LiteralString(defval)})";
            if (cstype == typeof(string)) return isInsertValueSql ? (_freeSql.Select<TestTb>() as Select0Provider)._commonUtils.FormatSql("{0}", defval) : LiteralString(defval);
            if (cstype == typeof(bool)) return isInsertValueSql ? defval : (defval == "1" || defval == "t" ? "true" : "false");
            if (_freeSql.Ado.DataType == DataType.MySql || _freeSql.Ado.DataType == DataType.OdbcMySql)
                if (col.DbType == (int)MySqlDbType.Enum || col.DbType == (int)MySqlDbType.Set)
                    if (isInsertValueSql) return (_freeSql.Select<TestTb>() as Select0Provider)._commonUtils.FormatSql("{0}", defval);
            return isInsertValueSql ? defval : null; //sql function or exp
        }
        #endregion
        private string LiteralString(string text)
        {
            using (var writer = new StringWriter())
            {
                using (var provider = CodeDomProvider.CreateProvider("CSharp"))
                {
                    provider.GenerateCodeFromExpression(new CodePrimitiveExpression(text), writer, null);
                    return writer.ToString();
                }
            }
        }

        /// <summary>
        /// 获取列类型
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        public string GetCsType(string table, DbColumnInfo col)
        {
            if (_freeSql.Ado.DataType == FreeSql.DataType.MySql)
                if (col.DbType == (int)MySqlDbType.Enum || col.DbType == (int)MySqlDbType.Set)
                    return $"{this.GetClassName(table)}{this.GetColumnName(col.Name).ToUpper()}{(col.IsNullable ? "?" : "")}";
            return _freeSql.DbFirst.GetCsType(col);
        }

        #region mysql enum/set
        /// <summary>
        /// mysql枚举/set
        /// </summary>
        /// <returns></returns>
        public string GetMySqlEnumSetDefine(DbTableInfo table)
        {
            if (_freeSql.Ado.DataType != FreeSql.DataType.MySql && _freeSql.Ado.DataType != FreeSql.DataType.OdbcMySql) return null;
            var sb = new StringBuilder();
            foreach (var col in table.Columns)
            {
                if (col.DbType == (int)MySqlDbType.Enum || col.DbType == (int)MySqlDbType.Set)
                {
                    if (col.DbType == (int)MySqlDbType.Set) sb.Append("\r\n\t[Flags]");
                    sb.Append($"\r\n\tpublic enum {this.GetClassName(table.Name)}{this.GetColumnName(col.Name).ToUpper()}");
                    if (col.DbType == (int)MySqlDbType.Set) sb.Append(" : long");
                    sb.Append(" {\r\n\t\t");

                    string slkdgjlksdjg = "";
                    int field_idx = 0;
                    int unknow_idx = 0;
                    string exp2 = string.Concat(col.DbTypeTextFull);
                    int quote_pos = -1;
                    while (true)
                    {
                        int first_pos = quote_pos = exp2.IndexOf('\'', quote_pos + 1);
                        if (quote_pos == -1) break;
                        while (true)
                        {
                            quote_pos = exp2.IndexOf('\'', quote_pos + 1);
                            if (quote_pos == -1) break;
                            int r_cout = 0;
                            //for (int p = 1; true; p++) {
                            //	if (exp2[quote_pos - p] == '\\') r_cout++;
                            //	else break;
                            //}
                            while (exp2[++quote_pos] == '\'') r_cout++;
                            if (r_cout % 2 == 0/* && quote_pos - first_pos > 2*/)
                            {
                                string str2 = exp2.Substring(first_pos + 1, quote_pos - first_pos - 2).Replace("''", "'");
                                if (Regex.IsMatch(str2, @"^[\u0391-\uFFE5a-zA-Z_\$][\u0391-\uFFE5a-zA-Z_\$\d]*$"))
                                    slkdgjlksdjg += ", " + str2;
                                else
                                    slkdgjlksdjg += string.Format(@", 
/// <summary>
/// {0}
/// </summary>
[Description(""{0}"")]
Unknow{1}", str2.Replace("\"", "\\\""), ++unknow_idx);
                                if (col.DbType == (int)MySqlDbType.Set)
                                    slkdgjlksdjg += " = " + Math.Pow(2, field_idx++);
                                if (col.DbType == (int)MySqlDbType.Enum && field_idx++ == 0)
                                    slkdgjlksdjg += " = 1";
                                break;
                            }
                        }
                        if (quote_pos == -1) break;
                    }
                    sb.Append(slkdgjlksdjg.Substring(2).TrimStart('\r', '\n', '\t'));
                    sb.Append("\r\n\t}");
                }
            }
            return sb.ToString();
        }
        #endregion
    }
    [Table(DisableSyncStructure = true)]
    class TestTb { public Guid id { get; set; } }
}