using Microsoft.Xna.Framework;

namespace Team6.Engine
{
    /// <summary>
    /// Basis class for all engine components
    /// </summary>
    public abstract class EngineComponent
    {

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
		/// <param name="gameTime">Same as the next two, but less convenient</param>
        /// <param name="elapsedSeconds">Elapsed Seconds since last update</param>
        /// <param name="totalSeconds">Elapsed Seconds since the game was started</param>
        public virtual void Update(GameTime gameTime, float elapsedSeconds, float totalSeconds)
        {

        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
		/// <param name="gameTime">Same as the next two, but less convenient</param>
        /// <param name="elapsedSeconds">Elapsed Seconds since last draw</param>
        /// <param name="totalSeconds">Elapsed Seconds since the game was started</param>
        public virtual void Draw(GameTime gameTime, float elapsedSeconds, float totalSeconds)
        {

        }

        /// <summary>
        /// Allows the component to load any content
        /// </summary>
        public virtual void LoadContent()
        {

        }

        /// <summary>
        /// Allows the component to unload any content
        /// </summary>
        public virtual void UnloadContent()
        {

        }


        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        public virtual void Initialize()
        {

        }


        public EngineComponent(MainGame game)
        {
            this.Game = game;
        }

        public MainGame Game { get; }

    }
}