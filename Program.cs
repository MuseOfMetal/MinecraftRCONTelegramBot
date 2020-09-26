using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using CoreRCON;
using Telegram.Bot;
using Telegram.Bot.Args;
namespace MinecraftRCONTelegramBot
{
    class Program
    {
        public static List<User> Users;
        static TelegramBotClient Bot = new TelegramBotClient("");
        static string Passwd = Config.Get().BotPassword;
        static RCON rCON = new RCON(IPAddress.Parse(Config.Get().IP), Config.Get().Port, Config.Get().Password);
        static void Main(string[] args)
        {
            Users = FileManager<List<User>>.Load("users.json") ?? new List<User>();
            rCON.ConnectAsync().GetAwaiter().GetResult();
            Bot.OnMessage += Bot_OnMessage;
            Bot.OnCallbackQuery += Bot_OnCallbackQuery;
            Bot.StartReceiving();
            while (true)
            {
                Thread.Sleep(int.MaxValue);
            }
        }

        private static async void Bot_OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            try
            {
                var user = Users[FindUser(e.CallbackQuery.From.Id)];
                Console.WriteLine($"[{DateTime.Now}] [{user.FName}] [{e.CallbackQuery.Data}]");
                for (int i = 0; i < user.Commands.Count; i++)
                {
                    if (user.Commands[i].Name == e.CallbackQuery.Data)
                    {
                        var responce = await rCON.SendCommandAsync(user.Commands[i].CommandText);
                        await Bot.SendTextMessageAsync(e.CallbackQuery.From.Id, responce, replyMarkup: Keyboard.GetMenu(user.Commands));
                        try { await Bot.DeleteMessageAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId); } catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            try
            {
                var msg = e.Message;
                var msgFrom = e.Message.From;
                var msgText = e.Message.Text;
                int ui = FindUser(msgFrom.Id);
                if (ui == -1)
                {
                    Users.Add(new User() { FName = msgFrom.FirstName, LName = msgFrom.LastName, UName = msgFrom.Username, Id = msgFrom.Id, PermissionLvl = 0 });
                    ui = FindUser(msgFrom.Id);
                }

                var user = Users[ui];
                Console.WriteLine($"[{DateTime.Now}] [{user.FName}] [{msgText}]");
                if (msgText == Passwd)
                {
                    Users[ui].PermissionLvl = 1;
                    await Bot.SendTextMessageAsync(msgFrom.Id, "Access Granted");
                    FileManager.Save("users.json", Users);
                    return;
                }
                if (msgText == "/start")
                {
                    await Bot.SendTextMessageAsync(msgFrom.Id, "Enter password");
                    return;
                }
                if (user.PermissionLvl == 1)
                {
                    if (!string.IsNullOrEmpty(user.Status))
                    {
                        if (user.Status == "SendCommandName")
                        {
                            if (msgText.Length > 30)
                            {
                                await Bot.SendTextMessageAsync(msgFrom.Id, "Name must be 30 characters or less.");
                                return;
                            }
                            if (CheckNameCommand(user.Commands, msgText))
                            {
                                Users[ui].Status = "SendCommandText&*(" + msgText;
                                await Bot.SendTextMessageAsync(msgFrom.Id, "Enter command");
                                return;
                            }
                            else
                            {
                                await Bot.SendTextMessageAsync(msgFrom.Id, "This name already exists");
                                return;
                            }
                        }
                        else if (user.Status.StartsWith("SendCommandText"))
                        {
                            string name = user.Status.Split("&*(")[1];
                            Users[ui].Commands.Add(new Command() { Name = name, CommandText = msgText });
                            await Bot.SendTextMessageAsync(msgFrom.Id, "Successful binded");
                            Users[ui].Status = "";
                            FileManager.Save("users.json", Users);
                            return;
                        }
                        else if (user.Status == "DeleteCommand")
                        {
                            if (msgText == "/cancel")
                            {
                                Users[ui].Status = "";
                                await Bot.SendTextMessageAsync(msgFrom.Id, "Cancelled");
                                return;
                            }
                            int index = FindCommand(user.Commands, msgText);
                            if (index == -1)
                            {
                                await Bot.SendTextMessageAsync(msgFrom.Id, "Bind not found");
                                return;
                            }
                            else
                            {
                                Users[ui].Commands.RemoveAt(index);
                                Users[ui].Status = "";
                                await Bot.SendTextMessageAsync(msgFrom.Id, "Command deleted");
                                FileManager.Save("users.json", Users);
                                return;
                            }
                        }
                    }
                    if (msgText.StartsWith("/"))
                    {
                        if (msgText == "/rconreconnect")
                        {
                            rCON = new RCON(IPAddress.Parse(Config.Get().IP), Config.Get().Port, Config.Get().Password);
                            await rCON.ConnectAsync();
                            await Bot.SendTextMessageAsync(msgFrom.Id, "Reconnected");
                            return;
                        }
                        else if (msgText == "/bind")
                        {
                            Users[ui].Status = "SendCommandName";
                            await Bot.SendTextMessageAsync(msgFrom.Id, "Enter new command name");
                            return;
                        }
                        else if (msgText == "/commandsmenu")
                        {
                            try
                            {
                                await Bot.SendTextMessageAsync(msgFrom.Id, "Menu", replyMarkup: Keyboard.GetMenu(user.Commands));
                            }
                            catch
                            {
                                await Bot.SendTextMessageAsync(msgFrom.Id, "You have not any binded command");
                            }
                            return;
                        }
                        else if (msgText == "/deletecommand")
                        {
                            Users[ui].Status = "DeleteCommand";
                            await Bot.SendTextMessageAsync(msgFrom.Id, "Enter command name to delete. /cancel - for cancel ( ͡° ͜ʖ ͡°)");
                            return;
                        }
                    }
                    var responce = await rCON.SendCommandAsync(msgText);
                    await Bot.SendTextMessageAsync(msgFrom.Id, responce);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Bot.SendTextMessageAsync(e.Message.From.Id, $"Error\n\n{ex.Message}");
            }
        }
        static int FindCommand(List<Command> commands, string name)
        {
            for (int i = 0; i < commands.Count; i++)
            {
                if (commands[i].Name == name)
                    return i;
            }
            return -1;
        }
        static bool CheckNameCommand(List<Command> commands, string Name)
        {
            if (commands.Count == 0)
                return true;
            foreach (var item in commands)
            {
                if (item.Name == Name)
                    return false;
            }
            return true;
        }
        static int FindUser(int Id)
        {
            var User = Users;

            for (int i = 0; i < User.Count; i++)
            {
                if (User[i].Id == Id)
                    return i;
            }
            return -1;
        }
    }
}
