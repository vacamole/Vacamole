using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Team6.Engine.Content
{
    public static class ContentJsonExtensions
    {
        public static T LoadFromJson<T>(this ContentManager content, string assetName, string extension = ".json")
        {
            string text;
            using (Stream stream = TitleContainer.OpenStream("Content/" + assetName + extension))
            {
                using (StreamReader reader = new StreamReader(stream))
                    text = reader.ReadToEnd();
            }

            return JsonConvert.DeserializeObject<T>(text);
        }

    }
}
