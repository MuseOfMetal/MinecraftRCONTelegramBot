using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;

namespace MinecraftRCONTelegramBot
{
    class Keyboard
    {
        public static InlineKeyboardMarkup GetMenu(List<Command> commands)
        {
            var keyboard = new List<List<InlineKeyboardButton>>();
            if (commands.Count == 0)
                throw new ArgumentException();
            for (int i =0;i < commands.Count;)
            {
                var subKeyboard = new List<InlineKeyboardButton>();
                for (int j = 0; j < 3 && i < commands.Count; j++, i++)
                {
                    subKeyboard.Add(InlineKeyboardButton.WithCallbackData(commands[i].Name, commands[i].Name));
                }
                keyboard.Add(subKeyboard);
            }
            return keyboard.ToArray();
        }
    }
}
