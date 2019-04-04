using System;
using Comora;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Team6.Engine.Entities;
using Team6.Engine.Graphics2d;
using Team6.Engine.Misc;
using Team6.Engine.UI;
using Team6.Game.Misc;

namespace Team6.Engine
{
    /// <summary>
    /// Supports rendering 2d sprites with support for cameras
    /// </summary>
	public class Renderer2d : EngineComponent
    {
        private SpriteBatch spriteBatch;
        public Camera GameCamera { get; private set; }

        public DebugRenderer Debug { get; internal set; }

        public Renderer2d(MainGame game) : base(game)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            // The camera now sees 32 by 18 meters
            this.GameCamera = new Camera(Game.GraphicsDevice)
            {
                Width = GameConstants.ScreenWidth,
                Height = GameConstants.ScreenHeight,
                ResizeMode = ResizeMode.FillUniform,
                PositionOffset = new Vector2(-GameConstants.ScreenWidth / 2, -GameConstants.ScreenHeight / 2)
            };

            // TODO: remove debug stuff
            GameCamera.Debug.IsVisible = true;
            // White lines are 1m, red line 10m
            GameCamera.Debug.Grid.AddLines(1, Color.White, 1);
            GameCamera.Debug.Grid.AddLines(10, Color.Red, 1);
        }


        public override void LoadContent()
        {
            base.LoadContent();

            this.GameCamera.LoadContent(Game.GraphicsDevice);

            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            Debug = new DebugRenderer(spriteBatch, Game);
        }

        public override void Update(GameTime gameTime, float elapsedSeconds, float totalSeconds)
        {
            this.GameCamera.Update(gameTime);

            base.Update(gameTime, elapsedSeconds, totalSeconds);
        }

        public Vector2 VirtualScreenSize
        {
            get
            {
                Viewport viewport = Game.GraphicsDevice.Viewport;
                Vector2 screenSize = viewport.Bounds.Size.ToVector2();

                if (viewport.AspectRatio >= (16f / 9f)) // width is too big, scale with height
                    return new Vector2(screenSize.Y / 9f * 16f, screenSize.Y);
                else
                    return new Vector2(screenSize.X, screenSize.X / 16f * 9f);
            }
        }

        public Vector2 ScreenSize
        {
            get
            {
                Viewport viewport = Game.GraphicsDevice.Viewport;
                return viewport.Bounds.Size.ToVector2();
            }
        }

        public Vector2 VirtualScreenOffset { get { return (ScreenSize - VirtualScreenSize) / 2f; } }

        public override void Draw(GameTime gameTime, float elapsedSeconds, float totalSeconds)
        {
            bool drawDebug = InputFunctions.DrawDebug(Keyboard.GetState());

            DrawLayer(GameCamera, EntityType.Game, drawDebug, elapsedSeconds, totalSeconds);
            DrawLayer(null, EntityType.UI, drawDebug, elapsedSeconds, totalSeconds);
            DrawLayer(GameCamera, EntityType.OverlayLayer, drawDebug, elapsedSeconds, totalSeconds);

#if DEBUG
            if (drawDebug)
                this.GameCamera.Debug.Draw(gameTime, spriteBatch, new Vector2(0, 0));
#endif
        }

        private void DrawLayer(ICamera camera, EntityType entityType, bool debugDraw, float elapsedSeconds, float totalSeconds)
        {
            if (camera != null)
                spriteBatch.Begin(camera, SpriteSortMode.BackToFront);
            else
            {
                spriteBatch.Begin(SpriteSortMode.BackToFront, transformMatrix: Matrix.CreateTranslation(new Vector3(VirtualScreenOffset, 0)));
            }

            // [FOREACH PERFORMANCE] Should not allocate garbage
            foreach (var entity in Game.CurrentScene.GetEntities(entityType))
            {
                foreach (var drawable in entity.GetAllComponents<IDrawableComponent>())
                    drawable.Draw(spriteBatch, elapsedSeconds, totalSeconds);
            }

            if (debugDraw)
                DrawDebug(spriteBatch, entityType, elapsedSeconds, totalSeconds);

            spriteBatch.End();
        }

        private void DrawDebug(SpriteBatch spriteBatch, EntityType type, float elapsedSeconds, float totalSeconds)
        {
            // [FOREACH PERFORMANCE] ALLOCATES GARBAGE
            foreach (var entity in Game.CurrentScene.GetEntities(type))
            {
                entity.DebugDraw(Debug, elapsedSeconds, totalSeconds);

                // [FOREACH PERFORMANCE] ALLOCATES GARBAGE
                foreach (var c in entity.GetAllComponents<IDebugDrawable>())
                    c.DebugDraw(Debug, elapsedSeconds, totalSeconds);
            }
        }
    }
}