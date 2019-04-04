using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team6.Engine.Misc;

namespace Team6.Engine.UI
{
    /// <summary>
    /// Loads multiple font sizes of the same sprite font. Format of asset name is asset name + font size i.e DebugFont12.
    /// It also features a indirection between semantic font size (see <see cref="FontSize"/>) and rendered font size.
    /// This indirection will facilitate DPI Awareness later on.
    /// </summary>
    public class MultiSizeSpriteFont
    {
        private Dictionary<int, SpriteFont> spriteFonts = new Dictionary<int, SpriteFont>();

        public MultiSizeSpriteFont(MainGame game, MultiSizeSpriteFontDefinition def)
        {
            FontDefintion = def;

            // Load all fonts
            // [FOREACH PERFORMANCE] Should not allocate garbage
            foreach (var fontSize in def.AllAvailableSizes)
                spriteFonts.Add(fontSize, game.Content.Load<SpriteFont>(def.FontName + fontSize.ToString()));
        }


        private SpriteFont Get(float size, bool throwOnNotFound = true)
        {
            int actualSize = FontDefintion.GetActualSize(size);
            if (spriteFonts.ContainsKey(actualSize))
                return spriteFonts[actualSize];
            else if (throwOnNotFound)
                throw new ArgumentException($"The font {FontDefintion.FontName} does not support font size {size} at the current DPI Scaling Level.");

            return spriteFonts.Values.First();
        }

        public MultiSizeSpriteFontDefinition FontDefintion { get; }


        public Vector2 MeasureString(string text, float fontSize)
        {
            var font = Get(fontSize);
            return font.MeasureString(text);
        }

        public void DrawString(SpriteBatch spriteBatch, float size, string text, Vector2 position, Color color, float rotation = 0f, Vector2 origin = default(Vector2), SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
        {
            spriteBatch.DrawString(Get(size), text, position, color, rotation, origin, 1.0f, effects, layerDepth);
        }

        /// <summary>
        /// BE AWARE! Texture stretching is applied, this might lead to pixelated text if used incorrectly!
        /// </summary>
        public void DrawStringScaled(SpriteBatch spriteBatch, float size, string text, Vector2 position, Color color, float scaling, float rotation = 0f, Vector2 origin = default(Vector2), SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
        {
            spriteBatch.DrawString(Get(size), text, position, color, rotation, origin, scaling, effects, layerDepth);
        }
    }

    /// <summary>
    /// Defintion for multiple sprite fonts from a single font in different sizes and how to adapt
    /// them to a given DPI
    /// </summary>
    public class MultiSizeSpriteFontDefinition
    {
        /// <summary>
        /// All available sizes of the sprite font
        /// </summary>
        public int[] AllAvailableSizes { get; private set; }

        /// <summary>
        /// Font specific scaling factor that is used to scale the incoming size (before offset):
        /// i.e. Actual_Size = (Requested_Size * ScalingFactor) + Offset
        /// </summary>
        public float ScalingFactor { get; private set; } = 1.0f;

        /// <summary>
        /// Font specific offset used for font size calculation:
        /// i.e. Actual_Size = (Requested_Size * ScalingFactor) + Offset
        /// </summary>
        public float Offset { get; private set; } = 0.0f;

        /// <summary>
        /// The name of the font assets except the size. The size will be appended i.e. for the asset DebugFont12
        /// FontName has to be equal to DebugFont
        /// </summary>
        public string FontName { get; private set; }


        public MainGame Game { get; }
        public DisplayUtilities Display { get; }

        private MultiSizeSpriteFontDefinition(MainGame game, string fontName)
        {
            this.FontName = fontName;
            this.Game = game;
            this.Display = Game.EngineComponents.Get<DisplayUtilities>();
        }

        public static MultiSizeSpriteFontDefinition Create(MainGame game, string fontName)
        {
            return new MultiSizeSpriteFontDefinition(game, fontName);
        }

        /// <summary>
        /// Initializes with available sizes from 12 to 72 in steps of 4
        /// </summary>
        /// <returns>The instance again (Fluent API)</returns>
        public MultiSizeSpriteFontDefinition WithDefaultAvailableSizes()
        {
            // FontSize starting from 12 in steps of 4 up to 256
            AllAvailableSizes = Enumerable.Range(0, 61).Select(i => i * 4 + 12).ToArray();
            return this;
        }

        public int GetActualSize(float requestedSize)
        {
            float exactSize = (requestedSize * ScalingFactor) + Offset;

            int lastElem = AllAvailableSizes[0];

            if (lastElem > requestedSize)
                return lastElem;

            for (int i = 1; i < AllAvailableSizes.Length; i++)
            {
                if (lastElem <= exactSize && AllAvailableSizes[i] > exactSize)
                    return lastElem;

                lastElem = AllAvailableSizes[i];
            }

            return lastElem;
        }
    }

    public static class FontSizes
    {
        public const float VerySmall = 12f;
        public const float Small = 16f;
        public const float Smaller = 20f;
        public const float Medium = 24f;
        public const float Larger = 28f;
        public const float Large = 32f;
        public const float VeryLarge = 36f;
    }
}
