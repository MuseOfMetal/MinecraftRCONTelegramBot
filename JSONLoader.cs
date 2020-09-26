using System;
using System.IO;
using Newtonsoft.Json;
namespace MinecraftRCONTelegramBot
{
    class FileManager<T>
    {
        public static T Load(string path)
        {
            if (File.Exists(path))
                using (StreamReader r = new StreamReader(path))
                {
                    return JsonConvert.DeserializeObject<T>(r.ReadToEnd());
                }
            else
                return default;
                
        }
    }

    class FileManager 
    {
        public static void Save(string Path, object obj)
        {
            if (!File.Exists(Path))
                using (FileStream f = File.Create(Path)) { }



            using (StreamWriter w = new StreamWriter(Path))
            {
                w.WriteLine(JsonConvert.SerializeObject(obj));
            }
        }
    }
}