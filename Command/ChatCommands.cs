using Sora.EventArgs.SoraEvent;
using System.Collections.ObjectModel;
using System.Text;
using YukariToolBox.LightLog;

namespace XSB
{
    public class RealCommands
    {
        public static void InitCommands()
        {
            Action<Command> add = (cmd) =>
            {
                Commands.ChatCommands.Add(cmd);
            };

            add(new Command(addWhiteList, "添加白名单")
            {
                HelpText = "用于在首次设置TShock时验证身份成为超级管理员.",
                Show = "验证超级管理员"
            });
        }

        private static void addWhiteList(CommandArgs args)
        {
            args.GroupMessageArgs.Reply("还没写.jpg");
        }
    }

}