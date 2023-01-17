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
                HelpText = "添加白名单",
            });
        }

        private static void addWhiteList(CommandArgs args)
        {
            args.GroupMessageArgs.Reply("还没写.jpg");
        }
    }

}