using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Team6.Engine.UI;
using Team6.Engine.Components;
using Team6.Engine.Graphics2d;

namespace Team6.Engine.UI
{
    public class HUDTextComponent : Component, IDrawableComponent
    {
        public string Text { get; set; }
        public Color Color { get; set; }
        public Color ShadowColor { get; set; } = Color.Black * 0.5f;
        public float Opacity { get; set; } = 1.0f;

        private Vector2 origin;
        private float layerDepth;
        private Vector2 offset;
        private MultiSizeSpriteFont font;
        private float textHeight;
        private Renderer2d renderer2d;

        public Vector2 ShadowDistance { get; set; } = new Vector2(2, 2);


        /// <summary>
        /// Renders a sprite on the HUD
        /// </summary>
        /// <param name="font">the font</param>
        /// <param name="text">the text to display</param>
        /// <param name="textHeight">The text height in screen size (i.e. 1 equals full screen height)</param>
        /// <param name="origin">the center of the object. Top left is (0, 0), bottom right is (1, 1)</param>
        /// <param name="offset">Position offset from the entity position</param>
        /// <param name="layerDepth">from -1 to 1, default 0</param>
        public HUDTextComponent(MultiSizeSpriteFont font, float textHeight, string text, Vector2 origin = default(Vector2),
            float layerDepth = 0, Vector2 offset = default(Vector2), Color? color = null)
        {
            this.Text = text;
            this.origin = origin;
            this.font = font;
            this.offset = offset;
            this.Color = color ?? Color.BlanchedAlmond;
            this.textHeight = textHeight;

            // transform from [-1,1] to [1,0]
            this.layerDepth = 0.5f - layerDepth * 0.5f;
        }

        public override void Initialize()
        {
            this.renderer2d = this.Entity.Scene.Game.EngineComponents.Get<Renderer2d>();
        }

        public void Draw(SpriteBatch spriteBatch, float elapsedSeconds, float totalSeconds)
        {
            Vector2 position = Entity.Body.Position + offset;
            Vector2 screenSize = renderer2d.VirtualScreenSize;

            float smallerSide = Math.Min(screenSize.X, screenSize.Y);
            float fontSize = (textHeight * smallerSide) * 72f / 96f; // px to pt

            Vector2 textSize = font.MeasureString(Text, fontSize);

            font.DrawString(spriteBatch, fontSize, this.Text, position * screenSize, Color * Opacity, 0, origin * textSize, SpriteEffects.None, layerDepth);

            font.DrawString(spriteBatch, fontSize, this.Text, position * screenSize + ShadowDistance, ShadowColor * Opacity, 0, origin * textSize, SpriteEffects.None, layerDepth + 0.01f);
        }
    }
}
