using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GenerateMonoSpriteFonts
{
    public static class Program
    {
        public static string GetSpriteFontFile(string fontName, int size, float spacing = 0, bool kerning = true, string style = "Regular",
            char defaultCharacter = '*', int startCharacter = 32, int endCharacter = 126)
        {
            return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<XnaContent xmlns:Graphics= ""Microsoft.Xna.Framework.Content.Pipeline.Graphics"" >
    <Asset Type= ""Graphics:FontDescription"" >
        <FontName>{fontName}</FontName>
        <Size>{size}</Size>
        <Spacing>{spacing}</Spacing>
        <UseKerning>{kerning.ToString().ToLower()}</UseKerning>
        <Style>{style}</Style>
        <DefaultCharacter>*</DefaultCharacter>
        <CharacterRegions>
            <CharacterRegion>
                <Start>&#{startCharacter};</Start>
                <End>&#{endCharacter};</End>
            </CharacterRegion>
            <CharacterRegion>
                <Start>&#233;</Start>
                <End>&#255;</End>
            </CharacterRegion>
            <CharacterRegion>
                <Start>&#225;</Start>
                <End>&#225;</End>
            </CharacterRegion>
            <CharacterRegion>
                <Start>&#193;</Start>
                <End>&#193;</End>
            </CharacterRegion>
        </CharacterRegions>
    </Asset>
</XnaContent>";
        }

        public static string GetContentReference(string fileName)
        {
            fileName = fileName.Replace('\\', '/');
            return $@"

#begin {fileName}
/importer:FontDescriptionImporter
/processor:FontDescriptionProcessor
/processorParam:PremultiplyAlpha=True
/processorParam:TextureFormat=Compressed
/build:{fileName}";
        }


        public static void Main(string[] args)
        {
            string fontFileOrName = args[0];
            (int startSize, int endSize, int stepSize) = args[1].Split(':').Select(val => int.Parse(val)).ToTuple3();
            string outputFile = args[2];
            string contentProjectFileUWP = args[3];
            string contentProjectFileCXDesktop = args[4];

            if (File.Exists(fontFileOrName))
                fontFileOrName = GetRelativePath(fontFileOrName, Path.GetDirectoryName(outputFile));


            using (FileStream contentFileStream = new FileStream(contentProjectFileUWP, FileMode.Append))
            using (FileStream contentFileStreamUWP = new FileStream(contentProjectFileCXDesktop, FileMode.Append))
            {
                using (StreamWriter writerUWP = new StreamWriter(contentFileStream))
                using (StreamWriter writerCXDesktop = new StreamWriter(contentFileStreamUWP))
                    for (int i = startSize; i <= endSize; i += stepSize)
                    {
                        string currentFile = outputFile.Replace("{size}", i.ToString());

                        File.WriteAllText(currentFile, GetSpriteFontFile(fontFileOrName, i));

                        writerUWP.Write(GetContentReference(GetRelativePath(currentFile, Path.GetDirectoryName(contentProjectFileUWP))));
                        writerCXDesktop.Write(GetContentReference(GetRelativePath(currentFile, Path.GetDirectoryName(contentProjectFileCXDesktop))));
                    }
            }
        }

        public static (T a, T b, T c) ToTuple3<T>(this IEnumerable<T> enumerable)
        {
            return (enumerable.First(), enumerable.ElementAt(1), enumerable.ElementAt(2));
        }

        public static string GetRelativePath(string filespec, string folder)
        {
            Uri pathUri = new Uri(filespec);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }
    }
}