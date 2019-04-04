using System;
using Microsoft.Xna.Framework;
using Team6.Engine;
using Team6.Game.Scenes;
using Team6.Game.Mode;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Team6.Engine.UI;
using System.Collections.Generic;
using Comora;
using Team6.Engine.Input;
using Team6.Engine.Misc;
using Team6.Engine.Audio;
using Team6.Engine.Components;
using Team6.Engine.Content;
using Microsoft.Xna.Framework.Input;
using Team6.Engine.Animations;
using Team6.Game.Misc;
using Team6.Engine.MemoryManagement;

namespace Team6
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class MainGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        Renderer2d renderer2d;
        public InputManager InputManager { get; private set; }

        readonly ConcurrentQueue<Action> dispatcherQueue = new ConcurrentQueue<Action>();
        readonly ServiceContainer<EngineComponent> engineComponents = new ServiceContainer<EngineComponent>();



        /// <summary>
        /// Should be always loaded, is swapped into current scene while the next scene is loading asynchrounously
        /// </summary>
        Scene loadingScene = null;

        public PlayerAndGameInfo CurrentGameMode;

        /// <summary>
        /// Constructor MainGame
        /// </summary>
        public MainGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content. Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            engineComponents.Add(new DisplayUtilities(this));
            EngineComponents.Add(renderer2d = new Renderer2d(this));
            EngineComponents.Add(InputManager = new InputManager(this));
            engineComponents.Add(new AudioManager(this));

            // [FOREACH PERFORMANCE] Should not allocate garbage
            EngineComponents.AllServices.ForEach(a => a.Initialize());

            loadingScene = new LoadingScene(this);
            loadingScene.Initialize();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // [FOREACH PERFORMANCE] ALLOCATES GARBAGE
            EngineComponents.AllServices.ForEach(a => a.LoadContent());
            Debug = renderer2d.Debug;
            Camera = renderer2d.GameCamera;
            loadingScene.LoadContent();

            // set desired default resolution
            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;

#if LINUX
            // No, thanks!
            graphics.IsFullScreen = false;
#else
            graphics.IsFullScreen = Settings<GameSettings>.Value.IsFullscreen; // fullscreen when not debugging
#endif
            // F11 to switch to fullscreen
            InputManager.AddGlobalHooks(new InputMapping(
                    (f) => InputFunctions.FullScreen(f),
                    (f) => ToggleFullscreen())
                );

            this.Window.AllowUserResizing = true;
            graphics.ApplyChanges();

            this.CurrentGameMode = new PlayerAndGameInfo(this);
            SwitchScene(new MainMenuScene(this));
        }

        public void ToggleFullscreen()
        {
            graphics.IsFullScreen = !graphics.IsFullScreen;
            graphics.ApplyChanges();
            Settings<GameSettings>.Value.IsFullscreen = graphics.IsFullScreen;
            Settings<GameSettings>.Save();
        }

        public Camera Camera { get; private set; }

        /// <summary>
        /// Initializes and loads the next scene asynchrounsouly
        /// </summary>
        /// <param name="nextScene">The scene to load</param>
        /// <param name="silent">True, if no loading scene is shown, otherwise false</param>
        public async void SwitchScene(Scene nextScene, bool silent = false)
        {
            if (IsLoading)
                return;

            IsLoading = true;

            var oldScene = CurrentScene;

            if (!silent)
            {
                oldScene?.OnUnloading();
                CurrentScene = loadingScene;
                CurrentScene.OnShown();
            }

            // made the method async, such that instead of freezing the game, exceptions now crash the game.
            await Task.Run(() =>
            {
                UnloadScene(oldScene);
                nextScene.Initialize();
                nextScene.LoadContent();

                GC.Collect();
                GC.WaitForPendingFinalizers();

                // swap scene
                this.Dispatch(() =>
                {
                    CurrentScene.OnUnloading();
                    IsLoading = false;
                    CurrentScene = nextScene;
                    CurrentScene.OnShown();
                });
            });
        }



        private void UnloadScene(Scene oldScene)
        {
            oldScene?.UnloadContent();
        }

        /// <summary>
        /// Dipatches the action into the main update thread. The action will executed right before the next update call to the current scene.
        /// </summary>
        /// <param name="actionToInvoke">Action to invoke on the thread</param>
        public void Dispatch(Action actionToInvoke)
        {
            dispatcherQueue.Enqueue(actionToInvoke);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            loadingScene.UnloadContent();
            CurrentScene.UnloadContent();

            // [FOREACH PERFORMANCE] ALLOCATES GARBAGE
            EngineComponents.AllServices.ForEach(a => a.UnloadContent());

        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float totalSeconds = (float)gameTime.TotalGameTime.TotalSeconds;

            // [FOREACH PERFORMANCE] ALLOCATES GARBAGE
            foreach (var component in EngineComponents.AllServices)
                component.Update(gameTime, elapsedSeconds, totalSeconds);

            // execute all dispatched actions
            Action a;
            while (dispatcherQueue.TryDequeue(out a))
                a();

            CurrentScene.Update(elapsedSeconds, totalSeconds);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            base.Draw(gameTime);


            float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float totalSeconds = (float)gameTime.TotalGameTime.TotalSeconds;

            // [FOREACH PERFORMANCE] ALLOCATES GARBAGE
            foreach (var component in EngineComponents.AllServices)
                component.Draw(gameTime, elapsedSeconds, totalSeconds);

        }

        /// <summary>
        /// The scene that is currently being displayed, can also be the loading scene
        /// </summary>
        public Scene CurrentScene { get; private set; }

        /// <summary>
        /// Debug renderer
        /// </summary>
        public DebugRenderer Debug { get; private set; }

        /// <summary>
        /// Allows to query for game engine components
        /// </summary>
        public ServiceContainer<EngineComponent> EngineComponents
        {
            get { return engineComponents; }
        }

        /// <summary>
        /// Allows to query whether the game is currently loading a new scene
        /// </summary>
        public bool IsLoading { get; private set; }
    }
}
