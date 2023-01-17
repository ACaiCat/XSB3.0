using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using System.Data;
using System.Diagnostics;
using TerrariaApi.Server;
using TShockAPI.DB;

namespace XSB
{
    
    public static class DB
    {
        public static IDbConnection db;
        public static void Connect()
        {
            switch (Main.config.sqlConfig.SqlType.ToLower())
            {
                case "mysql":
                    try
                    {
                        db = new MySqlConnection()
                        {
                            ConnectionString = string.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4};",
                            Main.config.sqlConfig.Host,
                            Main.config.sqlConfig.Port,
                            Main.config.sqlConfig.DB,
                            Main.config.sqlConfig.User,
                            Main.config.sqlConfig.Password)
                        };
                    }
                    catch (MySqlException)
                    {
                        throw new Exception("MySql设置不正确");
                    }
                    break;

                case "sqlite":
                    string sql = Path.Combine(Main.config.sqlConfig.SqlitePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(sql)!);
                    db = new SqliteConnection(string.Format("Data Source={0}", sql));
                    break;
                default:
                    throw new Exception("配置文件错误: 无效数据库类型!");
            }

            SqlTableCreator sqlcreator = new SqlTableCreator(db, db.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());

            sqlcreator.EnsureTableStructure(new SqlTable("users",
                new SqlColumn("QQ", MySqlDbType.Int32) { Primary = true, Unique = true, Length = 100 },
                new SqlColumn("money", MySqlDbType.Int32) { Length = 100 },
                new SqlColumn("name", MySqlDbType.Text) { Length = 100 },
                new SqlColumn("permission", MySqlDbType.Int32) { Length = 100 },
                new SqlColumn("sign_time", MySqlDbType.Text) { Length = 100 },
                new SqlColumn("sign_count", MySqlDbType.Int32) { Length = 100 },
                new SqlColumn("frozen", MySqlDbType.Int32) { Length = 1 },
                new SqlColumn("ban_count", MySqlDbType.Int32)
                )) ;
            sqlcreator.EnsureTableStructure(new SqlTable("server_info",
                new SqlColumn("server_name", MySqlDbType.Text) { Primary = true, Unique = true, Length = 100 },
                new SqlColumn("rest_host", MySqlDbType.Text) { Length = 100 },
                new SqlColumn("rest_port", MySqlDbType.Text) { Length = 100 },
                new SqlColumn("rest_token", MySqlDbType.Text) { Length = 100 },
                new SqlColumn("game_host", MySqlDbType.Text) { Length = 100 },
                new SqlColumn("game_port", MySqlDbType.Text) { Length = 100 }
                ));

            sqlcreator.EnsureTableStructure(new SqlTable("sign_in_sort",
                new SqlColumn("ID", MySqlDbType.Int32) { Primary = true, Unique = true, Length = 100 },
                new SqlColumn("QQ", MySqlDbType.Int32) { Length = 100 },
                new SqlColumn("ger_money", MySqlDbType.Int32) { Length = 100 }
                ));
        }

       
    }
}