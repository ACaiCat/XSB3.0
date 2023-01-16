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
        int QQ { get; set; }
        string name { get; set; }
        Permision permission { get; set; }

        int money { get; set; }
        string signTime { get; set; }

        int banCount { get; set; }

        bool frozen { get; set; }
        bool sign 
        { 
            get
            {
                return (DateTime.Now.ToString("yyyy-MM-dd") == signTime);
            } 
        }

        bool signContinuously
        {
            get
            {
                return ((DateTime.Now-TimeSpan.FromDays(1.0)).ToString("yyyy-MM-dd") == signTime);
            }
        }
        
        int signCount { get; set; }
        int signSort { get; set; }
        int signGetMonet { get; set; }

        public ServerUser(int QQ)
        {
            using (QueryResult result = DB.db.QueryReader("SELECT * FROM 'users' WHERE QQ=@0;", QQ))
            {
                if (result.Read())
                {
                    QQ=result.Get<int>("QQ");
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
        static ServerUser Load(int QQ)
        {
            return new(QQ);
        }
    }
}