using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Team6.Engine.Entities;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework.Input;
using Team6.Engine.Components;
using Team6.Engine.UI;
using Team6.Engine.Animations;
using Team6.Engine.Content;
using Team6.Engine.Graphics2d;
using Team6.Engine.Misc;
using Team6.Game.Mode;
using Team6.Engine.Audio;
using Team6.Engine.MemoryManagement;

namespace Team6.Engine
{
    /// <summary>
    /// A scene is the main logic object that composes all different entities and logic classes
    /// </summary>
    public abstract class Scene
    {
        private List<Entity> entitiesToRemove = new List<Entity>();
        private List<Entity> entitiesToAdd = new List<Entity>();
        private KeyedSets<EntityType, Entity> entities = new KeyedSets<EntityType, Entity>(e => e.EntityType);

        public AudioSourceComponent NonPositionalAudio { get; } = new AudioSourceComponent() { PositionalSound = false };


        /// <summary>
        /// Creates a new scene object
        /// </summary>
        /// <param name="game">The game this scene belongs to</param>
        public Scene(MainGame game)
        {
            Game = game;
        }

        public Entity AddEntity(Entity entity)
        {
            entitiesToAdd.Add(entity);
            return entity;
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

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        public virtual void LoadContent()
        {
            MainFont = new MultiSizeSpriteFont(Game, MultiSizeSpriteFontDefinition.Create(Game, "Fonts\\Grinched\\Grinched").WithDefaultAvailableSizes());
            AddEntity(new Entity(this, EntityType.LayerIndependent, NonPositionalAudio));
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        public virtual void UnloadContent()
        {
            // [FOREACH PERFORMANCE] ALLOCATES GARBAGE
            foreach (var entity in Entities)
            {
                entity.UnloadContent();
            }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="elapsedSeconds">Elapsed Seconds since last update</param>
        /// <param name="totalSeconds">Elapsed Seconds since the game was started</param>
        public virtual void Update(float elapsedSeconds, float totalSeconds)
        {
            Dispatcher.Update(elapsedSeconds, totalSeconds);

            if (IsPaused)
            {
                // [FOREACH PERFORMANCE] ALLOCATES GARBAGE
                foreach (var entity in Entities)
                {
                    if (entity.EntityType != EntityType.Game)
                        entity.Update(elapsedSeconds, totalSeconds);
                }
            }
            else
            {
                World.Step(elapsedSeconds);

                // [FOREACH PERFORMANCE] ALLOCATES GARBAGE
                foreach (var entity in Entities)
                {
                    entity.Update(elapsedSeconds, totalSeconds);
                }
            }

            // [FOREACH PERFORMANCE] Should not allocate garbage
            foreach (var entity in entitiesToRemove)
            {
                if (entity != null) // TODO: remove this check when bug fixed
                    World.RemoveBody(entity.Body);
                entities.Remove(entity);
            }
            entitiesToRemove.Clear();

            // [FOREACH PERFORMANCE] Should not allocate garbage
            foreach (var entity in entitiesToAdd)
            {
                InsertAddedEntity(entity);
            }
            entitiesToAdd.Clear();
        }

        private void InsertAddedEntity(Entity entity)
        {
            entities.Add(entity);
            entity.Initialize();

            // [FOREACH PERFORMANCE] ALLOCATES GARBAGE
            foreach (var comp in entity.GetAllComponents<ILoadContent>())
            {
                comp.LoadContent();
            }
        }

        public virtual void OnShown()
        {

        }

        public virtual void OnUnloading()
        {

        }

        /// <summary>
        /// The associated game of the scene
        /// </summary>
        public MainGame Game { get; }

        public World World { get; } = new World(new Vector2(0f, 0f));

        public Dispatcher Dispatcher { get; } = new Dispatcher();

        /// <summary>
        /// Allows to query whether game is currently transitioning between scenes, i.e. can be used to disable input etc...
        /// </summary>
        public bool IsTransitioning { get; set; }

        public KeyedSets<EntityType, Entity> Entities
        {
            get { return entities; }
        }

        public SafeHashSetEnumerable<Entity> GetEntities(EntityType type)
        {
            return entities.GetAll(type);
        }

        public MultiSizeSpriteFont MainFont { get; private set; }

        public void RemoveEntity(Entity entity)
        {
#if DEBUG
            if (entity == null) throw new ArgumentException("Entity is null");
#endif
            entitiesToRemove.Add(entity);
        }


        public bool IsPaused { get; private set; } = false;

        public void Pause()
        {
            if (IsPaused)
                return;

            // [FOREACH PERFORMANCE] Should not allocate garbage
            foreach (var entity in GetEntities(EntityType.Game))
                entity.GetAllComponents<AudioSourceComponent>().ForEach(a => a.PauseAll());

            Dispatcher.Pause();
            IsPaused = true;
        }

        public void Unpause()
        {
            if (!IsPaused)
                return;

            // [FOREACH PERFORMANCE] ALLOCATES GARBAGE
            foreach (var entity in GetEntities(EntityType.Game))
                entity.GetAllComponents<AudioSourceComponent>().ForEach(a => a.ResumeAll());
            Dispatcher.Unpause();
            IsPaused = false;
        }
    }
}
