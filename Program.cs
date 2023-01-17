﻿using System;
using System.Threading.Tasks;
using System.Xml.Schema;
using Sora;
using Sora.EventArgs.SoraEvent;
using Sora.Interfaces;
using Sora.Net.Config;
using Sora.Util;
using YukariToolBox.LightLog;
using XSB;

Main.config = Config.Read(Config.ConfigPath);
DB.Connect();

Thread readLine = new(new ThreadStart(delegate
{
    while (true)
    {
        var cmd =Console.ReadLine();
        if (!string.IsNullOrEmpty(cmd))
        {
            //处理命令方法
            Console.WriteLine("执行:" + cmd);
        }
    }

}));
readLine.Start();

//设置log等级
Log.LogConfiguration.EnableConsoleOutput().SetLogLevel(LogLevel.Debug);

//实例化Sora服务
ISoraService service = SoraServiceFactory.CreateService(new ServerConfig
{
    UniversalPath = "/onebot/v11/",
    EnableSocketMessage    = false,
    ThrowCommandException  = false,
    SendCommandErrMsg      = false,
    CommandExceptionHandle = CommandExceptionHandle,
    Port                   = 8080
});

#region 事件处理

//连接事件
service.ConnManager.OnOpenConnectionAsync += (connectionId, eventArgs) =>
{
    Log.Debug("Sora_Test|OnOpenConnectionAsync", $"connectionId = {connectionId} type = {eventArgs.Role}");
    return ValueTask.CompletedTask;
};
//连接关闭事件
service.ConnManager.OnCloseConnectionAsync += (connectionId, eventArgs) =>
{
    Log.Debug("Sora_Test|OnCloseConnectionAsync",
              $"uid = {eventArgs.SelfId} connectionId = {connectionId} type = {eventArgs.Role}");
    return ValueTask.CompletedTask;
};
//连接成功元事件
service.Event.OnClientConnect += (_, eventArgs) =>
{
    Log.Debug("Sora_Test|OnClientConnect", $"uid = {eventArgs.LoginUid}");
    return ValueTask.CompletedTask;
};

//群聊消息事件
service.Event.OnGroupMessage += async (_, eventArgs) => { await Task.Run(delegate { Commands.HandleCommand(eventArgs, eventArgs.Message.RawText); }); };
service.Event.OnSelfGroupMessage += (_, eventArgs) =>
{
    Log.Warning("test", $"self group msg {eventArgs.Message.MessageId}[{eventArgs.IsSelfMessage}]");
    return ValueTask.CompletedTask;
};
//私聊消息事件
service.Event.OnPrivateMessage += async (_, eventArgs) => { await eventArgs.Reply("好耶"); };
service.Event.OnSelfPrivateMessage += (_, eventArgs) =>
{
    Log.Warning("test", $"self private msg {eventArgs.Message.MessageId}[{eventArgs.IsSelfMessage}]");
    return ValueTask.CompletedTask;
};

//动态向管理器注册指令
service.Event.CommandManager.RegisterGroupDynamicCommand(new[] { "哇哦" },
                                                         async eventArgs =>
                                                         {
                                                             eventArgs.IsContinueEventChain = false;
                                                             await eventArgs.Reply("shit");
                                                         });

//指令错误处理
async void CommandExceptionHandle(Exception exception, BaseMessageEventArgs eventArgs, string log)
{
    string msg = $"死了啦都你害的啦\r\n{log}\r\n{exception.Message}";
    switch (eventArgs)
    {
        case GroupMessageEventArgs g:
            //await g.Reply(msg);
            break;
        case PrivateMessageEventArgs p:
            //await p.Reply(msg);
            break;
    }
}

#endregion

//启动服务并捕捉错误
await service.StartService().RunCatch(e => Log.Error("Sora Service", Log.ErrorLogBuilder(e)));

await Task.Delay(-1);

