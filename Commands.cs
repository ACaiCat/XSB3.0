using Sora.EventArgs.SoraEvent;
using System.Collections.ObjectModel;
using System.Text;
using YukariToolBox.LightLog;

namespace XSB
{
    public delegate void CommandDelegate(CommandArgs args);

    public class CommandArgs : EventArgs
    {
        public string Message { get; private set; }
        public ServerUser ServerUser { get; private set; }

        public GroupMessageEventArgs GroupMessageArgs { get; private set; }

        /// <summary>
        /// Parameters passed to the argument. Does not include the command name.
        /// IE '/kick "jerk face"' will only have 1 argument
        /// </summary>
        public List<string> Parameters { get; private set; }


        public CommandArgs(string message, ServerUser user, List<string> args,GroupMessageEventArgs eventArgs)
        {
            Message = message;
            ServerUser = user;
            Parameters = args;
            GroupMessageArgs = eventArgs;
        }

    }

    public class Command
    {
        /// <summary>
        /// Gets or sets whether to allow non-players to use this command.
        /// </summary>
        public bool AllowServer { get; set; }
        /// <summary>
        /// Gets or sets whether to do logging of this command.
        /// </summary>
        public bool DoLog { get; set; }
        /// <summary>
        /// Gets or sets the help text of this command.
        /// </summary>
        public string HelpText { get; set; }
        /// <summary>
        /// Gets or sets an extended description of this command.
        /// </summary>

        public string Show { get; set; }
        public string[] HelpDesc { get; set; }
        /// <summary>
        /// Gets the name of the command.
        /// </summary>
        public string Name { get { return Names[0]; } }
        /// <summary>
        /// Gets the names of the command.
        /// </summary>
        public List<string> Names { get; protected set; }
        /// <summary>
        /// Gets the permissions of the command.
        /// </summary>
        public Permision Permissions { get; protected set; }

        private CommandDelegate commandDelegate;
        public CommandDelegate CommandDelegate
        {
            get { return commandDelegate; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                commandDelegate = value;
            }
        }

        public Command(Permision permissions, CommandDelegate cmd, params string[] names)
            : this(cmd, names)
        {
            Permissions = permissions;
        }


        public Command(CommandDelegate cmd, params string[] names)
        {
            if (cmd == null)
                throw new ArgumentNullException("cmd");
            if (names == null || names.Length < 1)
                throw new ArgumentException("names");

            AllowServer = true;
            CommandDelegate = cmd;
            DoLog = true;
            HelpText = "没有可用的帮助.";
            HelpDesc = null;
            Names = new List<string>(names);
            Permissions = Permision.Normal;
        }

        public bool Run(string msg, ServerUser user, List<string> parms, GroupMessageEventArgs e)
        {
            if (!CanRun(user))
                return false;
            try
            {
                CommandDelegate(new CommandArgs(msg, user, parms,e));
            }
            catch (Exception ex)
            {
                e.Reply("指令执行失败,请查找日志获得更多信息.");
                Log.Error("Commands:",ex.ToString());
            }

            return true;
        }

        public bool Run(string msg, ServerUser user, Permision parms)
        {
            return Run(msg,user, parms);
        }

        public bool HasAlias(string name)
        {
            return Names.Contains(name);
        }

        public bool CanRun(ServerUser user)
        {
            return user.permission>=Permissions;
        }
    }

    public static class Commands
    {
        public static List<Command> ChatCommands = new List<Command>();
        public static ReadOnlyCollection<Command> TShockCommands = new ReadOnlyCollection<Command>(new List<Command>());

        /// <summary>
        /// The command specifier, defaults to "/"
        /// </summary>
        public static string Specifier
        {
            get { return Main.config.command.Specifier; }
        }

        private delegate void AddChatCommand(string permission, CommandDelegate command, params string[] names);


        public static bool HandleCommand(GroupMessageEventArgs e, string text)
        {
            ServerUser user = null!;
            try
            {
                user = ServerUser.Load(e.SenderInfo.UserId);
            }
            catch
            {
                e.Reply("没有添加白名单!");
                return true;
            }
            string cmdText = text.Remove(0, 1);
            string cmdPrefix = text[0].ToString();
            if (string.IsNullOrEmpty(Main.config.command.Specifier))
            {
                cmdPrefix = "";
            }
            int index = -1;
            for (int i = 0; i < cmdText.Length; i++)
            {
                if (IsWhiteSpace(cmdText[i]))
                {
                    index = i;
                    break;
                }
            }
            string cmdName;
            if (index == 0) // Space after the command specifier should not be supported
            {
                e.Reply("无效命令");
                return true;
            }
            else if (index < 0)
                cmdName = cmdText.ToLower();
            else
                cmdName = cmdText.Substring(0, index).ToLower();

            List<string> args;
            if (index < 0)
                args = new List<string>();
            else
                args = ParseParameters(cmdText.Substring(index));

            IEnumerable<Command> cmds = ChatCommands.FindAll(c => c.HasAlias(cmdName));


            if (cmds.Count() == 0)
            {
                e.Reply("无效命令");
                return true;
            }
            foreach (Command cmd in cmds)
            {
                if (!cmd.CanRun(user))
                {
                    Log.Debug("Commands:",string.Format("{0}({3}) 试图执行 {1}{2}.", user.name, Specifier, cmdText,user.QQ));
                    e.Reply("你没有权限使用这个命令哦.");
                }
                else
                {
                    if (cmd.DoLog)
                        Log.Debug("Commands:", string.Format("{0}({3}) 执行 {1}{2}.", user.name, Specifier, cmdText, user.QQ));
                    cmd.Run(cmdText, user, args,e);
                }
            }
            return true;
        }

        /// <summary>
        /// Parses a string of parameters into a list. Handles quotes.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static List<String> ParseParameters(string str)
        {
            var ret = new List<string>();
            var sb = new StringBuilder();
            bool instr = false;
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];

                if (c == '\\' && ++i < str.Length)
                {
                    if (str[i] != '"' && str[i] != ' ' && str[i] != '\\')
                        sb.Append('\\');
                    sb.Append(str[i]);
                }
                else if (c == '"')
                {
                    instr = !instr;
                    if (!instr)
                    {
                        ret.Add(sb.ToString());
                        sb.Clear();
                    }
                    else if (sb.Length > 0)
                    {
                        ret.Add(sb.ToString());
                        sb.Clear();
                    }
                }
                else if (IsWhiteSpace(c) && !instr)
                {
                    if (sb.Length > 0)
                    {
                        ret.Add(sb.ToString());
                        sb.Clear();
                    }
                }
                else
                    sb.Append(c);
            }
            if (sb.Length > 0)
                ret.Add(sb.ToString());

            return ret;
        }

        private static bool IsWhiteSpace(char c)
        {
            return c == ' ' || c == '\t' || c == '\n';
        }

    }
}