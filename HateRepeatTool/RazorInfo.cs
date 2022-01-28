using FreeSql.DatabaseModel;
using System.Collections.Generic;

namespace HateRepeatTool
{
    /// <summary>
    /// 模版信息
    /// </summary>
    public class RazorInfo
    {
        /// <summary>
        /// 命名空间
        /// </summary>
        public string? NameSpace { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// 作用域
        /// </summary>
        public string? Scope { get; set; }
        /// <summary>
        /// 列集合
        /// </summary>
        public List<ColumnInfo>? Columns { get; set; }
        /// <summary>
        /// 数据库表信息
        /// </summary>
        public DbTableInfo? DbTableInfo { get; set; }
        /// <summary>
        /// cshtml模板
        /// </summary>
        public string? TemplateFilePath { get; set; }
        /// <summary>
        /// 保存路径
        /// </summary>
        public string? ToPath { get; set; }
        /// <summary>
        /// 文件名称
        /// </summary>
        public string? FileName { get; set; }
        /// <summary>
        /// 模版信息
        /// </summary>
        public Template? Template { get; set; }
        /// <summary>
        /// freeSql表特性
        /// </summary>
        public string? Attribute { get; set; }
        /// <summary>
        /// freeSql_MySqlEnumSet
        /// </summary>
        public string? MySqlEnumSet { get; set; }
    }

    public class ColumnInfo
    {
        /// <summary>
        /// 列名
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// 列描述
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// 列类型
        /// </summary>
        public string? Type { get; set; }
        /// <summary>
        /// freeSql实体列类型
        /// </summary>
        public string? EntityType { get; set; }
        /// <summary>
        /// freeSql列特性
        /// </summary>
        public string? Attribute { get; set; }
    }
}