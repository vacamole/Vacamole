using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Team6.Engine.Components;
using Team6.Engine.Content;

namespace Team6.Engine.Graphics2d
{
    public class SpriteComponent : Component, IDrawableComponent, ILoadContent
    {
        protected Texture2D texture;
        protected Rectangle? sourceRectangle;
        protected string textureName;
        protected Vector2 pivot;
        protected Vector2 size;
        protected float layerDepth;

        public float Opacity { get; set; } = 1f;


        /// <summary>
        /// Renders a sprite
        /// </summary>
        /// <param name="textureName">the texture resource name</param>
        /// <param name="size">the size of the object, in meters</param>
        /// <param name="pivot">the center of the object. Top left is (0, 0), bottom right is (1, 1)</param>
        /// <param name="sourceRectangle">Which part of the texture to show, in pixel</param>
        /// <param name="layerDepth">The z value. From background (-1) to foreground (1) </param>
        public SpriteComponent(string textureName, Vector2 size, Vector2 pivot = default(Vector2)
            , Rectangle? sourceRectangle = null, float layerDepth = 0)
        {
            this.textureName = textureName;
            this.sourceRectangle = sourceRectangle;
            this.size = size;
            this.pivot = pivot;
            // transform layerDepth from [-1, 1] to [1.0, 0]
            this.layerDepth = 0.5f - layerDepth * 0.5f;
        }


        public virtual void Draw(SpriteBatch spriteBatch, float elapsedSeconds, float totalSeconds)
        {
            var b = Entity.Body;
            var ox = texture.Width * pivot.X;
            var oy = texture.Height * pivot.Y;

            var sx = size.X / texture.Width;
            var sy = size.Y / texture.Height;
            if (sourceRectangle != null)
            {
                sx = size.X / sourceRectangle.Value.Width;
                sy = size.Y / sourceRectangle.Value.Height;
                ox = sourceRectangle.Value.Width * pivot.X;
                oy = sourceRectangle.Value.Height * pivot.Y;
            }


            spriteBatch.Draw(texture, b.Position, sourceRectangle, Color.White * Opacity,
                b.Rotation, new Vector2(ox, oy), new Vector2(sx, sy), SpriteEffects.None, layerDepth);
        }

        public void LoadContent()
        {
            this.texture = this.Entity.Game.Content.Load<Texture2D>(this.textureName);
        }
    }
}
