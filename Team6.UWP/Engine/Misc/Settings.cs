using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Team6.Engine.Misc
{
    public class Settings<T> where T : class, new()
    {
        private static T settings = null;


        public static T Value
        {
            get
            {
                if (settings == null)
                    Load();

                return settings;
            }
        }

        private static void Load()
        {
#if LINUX
            // IsolatedStorageFile throws an exception on mono.
            // Instead, let's just store the settings in the application folder
            String path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName);
            if (File.Exists(path))
            {
                using (System.IO.StreamReader file = new System.IO.StreamReader(path))
                {
                    settings = (T)JsonSerializer.Create().Deserialize(file, typeof(T));
                }

            }
            else
            {
                settings = new T();
            }
#else
            using (var userStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (userStorage.FileExists(FileName))
                {
                    using (IsolatedStorageFileStream stream = userStorage.OpenFile(FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            settings = (T)JsonSerializer.Create().Deserialize(reader, typeof(T));
                        }
                    }
                }
                else
                {
                    settings = new T();
                }
            }
#endif
        }

        public static readonly string FileName = $"settings.{typeof(T).Name}.json";

        public static void Save()
        {
#if LINUX
            String path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName);
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(path))
            {
                JsonSerializer.Create().Serialize(file, settings);
            }
#else
            using (var userStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (Stream stream = userStorage.OpenFile(FileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (var writer = new StreamWriter(stream))
                        JsonSerializer.Create().Serialize(writer, settings);
                }
            }
#endif
        }

    }
}
