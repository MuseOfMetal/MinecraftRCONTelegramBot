using System;
using System.Collections.Generic;
using System.Text;

namespace MinecraftRCONTelegramBot
{
    class Config
    {
        public string IP;
        public ushort Port;
        public string Password;
        public string BotPassword;
        private static Config _config;
        public static Config Get()
        {
            if (_config != null)
                return _config;

            _config = FileManager<Config>.Load("config.json") ?? new Config();
            return _config;
        }
    }
}
