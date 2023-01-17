using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using TShockAPI.DB;

namespace XSB
{
    public class SqlConfig
    {
        [JsonProperty("数据库类型")]
        public string SqlType { get; set; } = "Sqlite";

        [JsonProperty("SqLite路径")]
        public string SqlitePath { get; set; } = "XSB.sqlite";

        [JsonProperty("MySQL地址")]
        public string Host { get; set; } = "localhost";

        [JsonProperty("MySQL端口")]
        public int Port { get; set; } = 3306;

        [JsonProperty("MySQL数据库名")]
        public string DB { get; set; } = "XSB";

        [JsonProperty("MySQL用户")]
        public string User { get; set; } = "Cai";

        [JsonProperty("MySQL用户密码")]
        public string Password { get; set; } = "123456";
    }

    public class CommandConfig
    {
        [JsonProperty("命令标识符")]
        public string Specifier { get; set; } = "";
    }

    public class Config
    {
        public const string ConfigPath = "XSBConfig.json";

        [JsonProperty("数据库设置")]
        public SqlConfig sqlConfig { get; set; } = new();

        [JsonProperty("命令设置")]
        public CommandConfig command { get; set; } = new();

        public void Write(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static Config Read(string path)
        {
            if (!File.Exists(path))
            {
                Config config = new Config();
                config.Write(path);
                return config;
            }
            else
            {
                return JsonConvert.DeserializeObject<Config>(File.ReadAllText(path))!;
            }
        }
    }
}