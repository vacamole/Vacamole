using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Team6.Engine.Misc;
using Team6.Engine.Components;
using Team6.Engine.Content;
using Team6.Engine.Graphics2d;

namespace Team6.Engine.UI
{
    public class HUDComponent : Component, IDrawableComponent, ILoadContent
    {
        protected Texture2D texture;
        protected Rectangle? sourceRectangle;
        protected string textureName;
        protected Vector2 origin;
        protected Vector2 size;
        protected float layerDepth;
        protected Vector2 offset;
        private Renderer2d renderer2d;

        /// <summary>
        /// Renders a sprite on the HUD
        /// </summary>
        /// <param name="textureName">the texture resource name</param>
        /// <param name="size">the size of the object, from 0 to 1</param>
        /// <param name="origin">the center of the object. Top left is (0, 0), bottom right is (1, 1)</param>
        /// <param name="offset">Position offset from the entity position</param>
        /// <param name="sourceRectangle">Which part of the texture to show, in pixel</param>
        /// <param name="layerDepth">from -1 to 1, default 0</param>
        public HUDComponent(string textureName, Vector2 size, Vector2 origin = default(Vector2),
            Rectangle? sourceRectangle = null, float layerDepth = 0, Vector2 offset = default(Vector2))
        {
            this.textureName = textureName;
            this.sourceRectangle = sourceRectangle;
            this.size = size;
            this.origin = origin;
            this.offset = offset;
            // transform from [-1,1] to [1,0]
            this.layerDepth = 0.5f - layerDepth * 0.5f;
        }

        /// <summary>
        /// Renders a sprite on the HUD
        /// </summary>
        /// <param name="texture">the texture</param>
        /// <param name="size">the size of the object, from 0 to 1</param>
        /// <param name="origin">the center of the object. Top left is (0, 0), bottom right is (1, 1)</param>
        /// <param name="offset">Position offset from the entity position</param>
        /// <param name="sourceRectangle">Which part of the texture to show, in pixel</param>
        /// <param name="layerDepth">from -1 to 1, default 0</param>
        public HUDComponent(Texture2D texture, Vector2 size, Vector2 origin = default(Vector2),
            Rectangle? sourceRectangle = null, float layerDepth = 0, Vector2 offset = default(Vector2))
        {
            this.texture = texture;
            this.sourceRectangle = sourceRectangle;
            this.size = size;
            this.origin = origin;
            this.offset = offset;
            // transform from [-1,1] to [1,0]
            this.layerDepth = 0.5f - layerDepth * 0.5f;
        }

        public Vector2 LocalPointToWorldPoint(Vector2 localPoint)
        {
            return offset - origin * size + localPoint * size;
        }

        public override void Initialize()
        {
            this.renderer2d = this.Entity.Scene.Game.EngineComponents.Get<Renderer2d>();
        }


        public virtual void Draw(SpriteBatch spriteBatch, float elapsedSeconds, float totalSeconds)
        {
            Vector2 textureSize = new Vector2(texture.Width, texture.Height);
            if (sourceRectangle != null)
            {
                textureSize = new Vector2(sourceRectangle.Value.Width, sourceRectangle.Value.Height);
            }

            Vector2 position = Entity.Body.Position + offset;
            Vector2 originInTexCoords = textureSize * origin;
            Vector2 screenSize = OnVirtualUIScreen ? renderer2d.VirtualScreenSize : renderer2d.ScreenSize;

            float smallerSide = Math.Min(screenSize.X, screenSize.Y);

            Vector2 scaledSize;

            if (MaintainAspectRation)
                scaledSize = size / textureSize * smallerSide;
            else
                scaledSize = screenSize  / textureSize * size;

            Vector2 actualPosition = position * screenSize;

            if (!OnVirtualUIScreen)
                actualPosition -= renderer2d.VirtualScreenOffset;

            spriteBatch.Draw(texture, actualPosition, sourceRectangle, Color * Opacity,
            Entity.Body.Rotation, originInTexCoords, scaledSize, SpriteEffects.None, layerDepth);
        }

        public float Opacity { get; set; } = 1;

        public Color Color { get; set; } = Color.White;

        public bool MaintainAspectRation { get; set; } = true;

        public bool OnVirtualUIScreen { get; set; } = true;



        public void LoadContent()
        {
            if (texture == null || texture.IsDisposed)
                this.texture = this.Entity.Game.Content.Load<Texture2D>(this.textureName);
        }
    }
}
