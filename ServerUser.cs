using NuGet.Protocol;
using Sora.Entities;
using TShockAPI.DB;

namespace XSB
{
    public enum Permision
    {
        Normal,
        Vip,
        SVip,
        Admin,
        Superadmin
    }
    public class ServerUser
    {
        public long QQ { get; set; }
        public string name { get; set; }
        public Permision permission { get; set; }

        public int money { get; set; }
        public string signTime { get; set; }

        public int banCount { get; set; }

        public bool frozen { get; set; }
        public bool sign 
        { 
            get
            {
                return (DateTime.Now.ToString("yyyy-MM-dd") == signTime);
            } 
        }

        public bool signContinuously
        {
            get
            {
                return ((DateTime.Now-TimeSpan.FromDays(1.0)).ToString("yyyy-MM-dd") == signTime);
            }
        }

        public int signCount { get; set; }
        public int signSort { get; set; }
        public int signGetMonet { get; set; }

        public ServerUser()
        {
        }

        public ServerUser(long QQ)
        {
            using (QueryResult result = DB.db.QueryReader("SELECT * FROM 'users' WHERE QQ=@0;", QQ))
            {
                if (result.Read())
                {
                    QQ=result.Get<long>("QQ");
                    name=result.Get<string>("name");
                    permission = (Permision)result.Get<int>("permission");
                    money = result.Get<int>("money");
                    signCount = result.Get<int>("sign_count");
                    banCount = result.Get<int>("ban_count");
                    frozen = result.Get<int>("frozen")==1;
                    signTime = result.Get<string>("sign_time");
                    using (QueryResult sign_result = DB.db.QueryReader("SELECT * FROM 'users' WHERE QQ=@0;", QQ))
                    {
                        if (result.Read())
                        {
                            signSort = sign_result.Get<int>("ID");
                            signGetMonet = sign_result.Get<int>("ger_money");
                        }
                        else
                        {
                            signSort = -1 ;
                            signGetMonet = -1;
                        }
                    }
                }
                else
                {
                    throw new Exception("用户不存在");
                }
            }
        }
        public static ServerUser Load(long QQ)
        {
            return new(QQ);
        }
    }
}