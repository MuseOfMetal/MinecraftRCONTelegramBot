using System;
using System.Collections.Generic;
using System.Text;

namespace MinecraftRCONTelegramBot
{
    class User
    {
        public string FName;
        public string LName;
        public string UName;
        public int Id;
        public int PermissionLvl;
        public string Status;
        public List<Command> Commands = new List<Command>();
    }
}
