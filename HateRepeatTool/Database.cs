using FreeSql.DatabaseModel;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace HateRepeatTool
{
    /// <summary>
    /// 数据库信息
    /// </summary>
    public class Database
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        public string? Connection { get; set; }
        /// <summary>
        /// 数据库类型
        /// </summary>
        public DatabaseType? Type { get; set; }


        /// <summary>
        /// 创建数据库
        /// </summary>
        /// <returns></returns>
        public IFreeSql CreateDb()
        {
            if (this.Connection == null)
                throw new InvalidOperationException($"数据库连接字符串为必填项，“{nameof(Connection)}”。");
            if (this.Type == null)
                throw new InvalidOperationException($"没有指定数据库类型，“{nameof(Type)}”。");

            //Console.WriteLine($"数据库连接{Connection}");
            //Console.WriteLine($"数据库类型{Type}");
            FreeSql.DataType dataType = (FreeSql.DataType)Type;
            IFreeSql fsql = new FreeSql.FreeSqlBuilder()
           .UseConnectionString(dataType, this.Connection)
           .UseAutoSyncStructure(false)
           .Build();
            return fsql;
        }

        /// <summary>
        /// 获取表结构
        /// </summary>
        /// <returns></returns>
        public List<DbTableInfo> GetTables()
        {
            using IFreeSql freeSql = this.CreateDb();
            var t2 = freeSql.DbFirst.GetTablesByDatabase();
            return t2;
        }

        public string GetCsType(DbColumnInfo col)
        {
            using IFreeSql fsql = this.CreateDb();
            return fsql.DbFirst.GetCsType(col);
        }

    }


    /// <summary>
    /// 数据库类型
    /// </summary>
    public enum DatabaseType
    {
        MySql = 0,
        SqlServer = 1,
        PostgreSQL = 2,
        Oracle = 3
    }
}