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
            // To quickly work around that, lets just not save any settings
            settings = new T();
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
            // do nothing
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
