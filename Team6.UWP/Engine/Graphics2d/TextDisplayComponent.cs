using Comora;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Team6.Engine.Components;
using Team6.Engine.UI;

namespace Team6.Engine.Graphics2d
{
    public class TextDisplayComponent : Component, IDrawableComponent
    {
        public string Text { get; set; }
        private MultiSizeSpriteFont font;
        private Color color;
        private float textHeight;
        private Camera camera;
        private float layerDepth;

        /// <summary>
        /// Render <paramref name="text"/> with <paramref name="textHeight"/> in game units
        /// </summary>
        /// <param name="font">font to use for the text</param>
        /// <param name="textHeight">height of text in game units (i.e meters)</param>
        /// <param name="text">text to render</param>
        /// <param name="color">color of text</param>
        public TextDisplayComponent(MultiSizeSpriteFont font, float textHeight, string text, Color? color = null, float layerDepth = 0f)
        {
            this.Text = text;
            this.color = color ?? Color.Crimson;
            this.font = font;
            this.textHeight = textHeight;
            // transform layerDepth from [-1, 1] to [1.0, 0]
            this.layerDepth = 0.5f - layerDepth * 0.5f;
        }

        public override void Initialize()
        {
            base.Initialize();
            camera = Entity.Game.EngineComponents.Get<Renderer2d>().GameCamera;
        }

        public void Draw(SpriteBatch spriteBatch, float elapsedSeconds, float totalSeconds)
        {
            float scale = camera.AbsoluteScale.X;

            float fontSize = (textHeight * scale) * 72f / 96f; //px to pt

            font.DrawStringScaled(spriteBatch, fontSize, this.Text,
                Entity.Body.Position, color, 1f / scale);
        }
    }
}
