//using System;
//using System.Diagnostics.Tracing;
//using System.Threading.Tasks;
//using Sora;
//using Sora.Entities;
//using Sora.EventArgs.SoraEvent;
//using Sora.Interfaces;
//using Sora.Net.Config;
//using Sora.Util;
//using TShockAPI;
//using YukariToolBox.LightLog;

//public delegate void CommandDelegate(CommandArgs args);

//public class CommandArgs : EventArgs
//{
//    private static bool IsWhiteSpace(char c)
//    {
//        return c == ' ' || c == '\t' || c == '\n';
//    }
//    public static bool HandleCommand(GroupMessageEventArgs args, string text)
//    {
//        string cmdText = text.Remove(0, 1);
//        string cmdPrefix = text[0].ToString();


//        int index = -1;
//        for (int i = 0; i < cmdText.Length; i++)
//        {
//            if (IsWhiteSpace(cmdText[i]))
//            {
//                index = i;
//                break;
//            }
//        }
//        string cmdName;
//        if (index == 0) // Space after the command specifier should not be supported
//        {
//            return true;
//        }
//        else if (index < 0)
//            cmdName = cmdText.ToLower();
//        else
//            cmdName = cmdText.Substring(0, index).ToLower();

//        List<string> args;
//        if (index < 0)
//            args = new List<string>();
//        else
//            args = ParseParameters(cmdText.Substring(index));

//        IEnumerable<Command> cmds = ChatCommands.FindAll(c => c.HasAlias(cmdName));

//        if (cmds.Count() == 0)
//        {
//            return true;
//        }
//        foreach (Command cmd in cmds)
//        {
//            if (!cmd.CanRun(arg))
//            {
//                TShock.Utils.SendLogs(string.Format("{0} 试图执行 {1}{2}.", player.Name, Specifier, cmdText), Color.PaleVioletRed, player);
//                player.SendErrorMessage("你没有权限使用这个命令哦.");
//                if (player.HasPermission(Permissions.su))
//                {
//                    player.SendInfoMessage("你可以使用 '{0}sudo {0}{1}' 跳过权限检查.", Specifier, cmdText);
//                }
//            }
//            else if (!cmd.AllowServer && !player.RealPlayer)
//            {
//                player.SendErrorMessage("你必须在游戏中使用该指令.");
//            }
//            else
//            {
//                if (cmd.DoLog)
//                    TShock.Utils.SendLogs(string.Format("{0} 执行: {1}{2}.", player.Name, silent ? SilentSpecifier : Specifier, cmdText), Color.PaleVioletRed, player);
//                cmd.Run(cmdText, silent, player, args);
//            }
//        }
//        return true;
//    }
//    public string Message { get; private set; }
//    public User user { get; private set; }
//    public bool Silent { get; private set; }

//    /// <summary>
//    /// Parameters passed to the argument. Does not include the command name.
//    /// IE '/kick "jerk face"' will only have 1 argument
//    /// </summary>
//    public List<string> Parameters { get; private set; }

//    public Player TPlayer
//    {
//        get { return Player.TPlayer; }
//    }

//    public CommandArgs(string message, TSPlayer ply, List<string> args)
//    {
//        Message = message;
//        Player = ply;
//        Parameters = args;
//        Silent = false;
//    }

//    public CommandArgs(string message, bool silent, TSPlayer ply, List<string> args)
//    {
//        Message = message;
//        Player = ply;
//        Parameters = args;
//        Silent = silent;
//    }
//}

//public class Command
//{
//    /// <summary>
//    /// Gets or sets whether to allow non-players to use this command.
//    /// </summary>
//    public bool AllowServer { get; set; }
//    /// <summary>
//    /// Gets or sets whether to do logging of this command.
//    /// </summary>
//    public bool DoLog { get; set; }
//    /// <summary>
//    /// Gets or sets the help text of this command.
//    /// </summary>
//    public string HelpText { get; set; }
//    /// <summary>
//    /// Gets or sets an extended description of this command.
//    /// </summary>

//    public string Show { get; set; }
//    public string[] HelpDesc { get; set; }
//    /// <summary>
//    /// Gets the name of the command.
//    /// </summary>
//    public string Name { get { return Names[0]; } }
//    /// <summary>
//    /// Gets the names of the command.
//    /// </summary>
//    public List<string> Names { get; protected set; }
//    /// <summary>
//    /// Gets the permissions of the command.
//    /// </summary>
//    public List<string> Permissions { get; protected set; }

//    private CommandDelegate commandDelegate;
//    public CommandDelegate CommandDelegate
//    {
//        get { return commandDelegate; }
//        set
//        {
//            if (value == null)
//                throw new ArgumentNullException();

//            commandDelegate = value;
//        }
//    }

//    public Command(List<string> permissions, CommandDelegate cmd, params string[] names)
//        : this(cmd, names)
//    {
//        Permissions = permissions;
//    }

//    public Command(string permissions, CommandDelegate cmd, params string[] names)
//        : this(cmd, names)
//    {
//        Permissions = new List<string> { permissions };
//    }

//    public Command(CommandDelegate cmd, params string[] names)
//    {
//        if (cmd == null)
//            throw new ArgumentNullException("cmd");
//        if (names == null || names.Length < 1)
//            throw new ArgumentException("names");

//        AllowServer = true;
//        CommandDelegate = cmd;
//        DoLog = true;
//        HelpText = "没有可用的帮助.";
//        HelpDesc = null;
//        Names = new List<string>(names);
//        Permissions = new List<string>();
//        Show = null;
//    }

//    public bool Run(string msg, bool silent, TSPlayer ply, List<string> parms)
//    {
//        if (!CanRun(ply))
//            return false;

//        try
//        {
//            CommandDelegate(new CommandArgs(msg, silent, ply, parms));
//        }
//        catch (Exception e)
//        {
//            ply.SendErrorMessage("指令执行失败,请查找日志获得更多信息.");
//            TShock.Log.Error(e.ToString());
//        }

//        return true;
//    }

//    public bool Run(string msg, TSPlayer ply, List<string> parms)
//    {
//        return Run(msg, false, ply, parms);
//    }

//    public bool HasAlias(string name)
//    {
//        return Names.Contains(name);
//    }

//    public bool CanRun(TSPlayer ply)
//    {
//        if (Permissions == null || Permissions.Count < 1)
//            return true;
//        foreach (var Permission in Permissions)
//        {
//            if (ply.HasPermission(Permission))
//                return true;
//        }
//        return false;
//    }
//}

//public static class Commands
//{
//    public static List<Command> ChatCommands = new List<Command>();
//    public static ReadOnlyCollection<Command> TShockCommands = new ReadOnlyCollection<Command>(new List<Command>());

//    /// <summary>
//    /// The command specifier, defaults to "/"
//    /// </summary>
//    public static string Specifier
//    {
//        get { return string.IsNullOrWhiteSpace(TShock.Config.Settings.CommandSpecifier) ? "/" : TShock.Config.Settings.CommandSpecifier; }
//    }

//    /// <summary>
//    /// The silent command specifier, defaults to "."
//    /// </summary>
//    public static string SilentSpecifier
//    {
//        get { return string.IsNullOrWhiteSpace(TShock.Config.Settings.CommandSilentSpecifier) ? "." : TShock.Config.Settings.CommandSilentSpecifier; }
//    }

//    private delegate void AddChatCommand(string permission, CommandDelegate command, params string[] names);