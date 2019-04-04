using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team6.Engine.Components;
using Team6.Engine.Content;
using Team6.Engine.MemoryManagement;
using Team6.Engine.Misc;

namespace Team6.Engine.Entities
{
    /// <summary>
    /// Represents a game entity. A game entity is made from components. Every Entity has a Body.
    /// </summary>
    public class Entity : IDebugDrawable, IUnloadContent
    {
        public Body Body { get; private set; }
        private ServiceContainer<Component> components = new ServiceContainer<Component>(c => c.Name);

        public Entity(Scene scene, EntityType type, params Component[] components) : this(scene, type, Vector2.Zero, BodyType.Static, components)
        {
        }

        public Entity(Scene scene, EntityType type, Vector2 position, params Component[] components) : this(scene, type, position, BodyType.Static, components)
        {
        }

        public Entity(Scene scene, EntityType type, Vector2 position, float rotation, params Component[] components) : this(scene, type, position, rotation, BodyType.Static, 0.01f, 0.02f, components)
        {
        }

        public Entity(Scene scene, EntityType type, Vector2 position, BodyType bodyType, params Component[] components) : this(scene, type, position, 0, bodyType, 0.01f, 0.02f, components)
        {
        }

        public Entity(Scene scene, EntityType type, Vector2 position, float rotation, BodyType bodyType, float linearDamping, float angularDamping, params Component[] components)
        {
            this.Scene = scene;
            this.Game = scene.Game;
            this.EntityType = type;
            this.Body = new Body(Scene.World, position, bodyType: bodyType)
            {
                Rotation = rotation,
                AngularDamping = angularDamping,
                LinearDamping = linearDamping,
                Friction = 0.3f,
                UserData = this
            };

            // [FOREACH PERFORMANCE] Should not allocate garbage
            foreach (var component in components)
            {
                AddComponent(component);
            }
        }

        /// <summary>
        /// Returns the component of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The component if available otherwise null</returns>
        public T GetComponent<T>() where T : class
        {
            return components.Get<T>();
        }

        /// <summary>
		/// Returns the component of type T with the given name.
		/// </summary>
		/// <typeparam name="T">The type of the component</typeparam>
		/// <returns>The component if available otherwise null</returns>
        public T GetComponentByName<T>(string name) where T : Component
        {
            return components.GetByName<T>(name);
        }

        /// <summary>
        /// Returns whether the entity has a component of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>True if available otherwise null</returns>
        public bool HasComponent<T>() where T : class
        {
            return GetComponent<T>() != null;
        }

        /// <summary>
		/// Returns the component of type T
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns>The component if available otherwise null</returns>
		public SafeSelectHashSetEnumerable<T, Component> GetAllComponents<T>() where T : class
        {
            return components.GetAll<T>();
        }

        /// <summary>
        /// Adds a component to this entity
        /// </summary>
        /// <param name="component"></param>
        public T AddComponent<T>(T component) where T : Component
        {
            component.SetEntity(this);
            components.Add(component);
            return component;
        }

        public virtual void Initialize()
        {
            // [FOREACH PERFORMANCE] ALLOCATES GARBAGE
            foreach (var component in components.AllServices)
                component.Initialize();
        }

        public void RemoveAllComponents<T>() where T : class
        {
            components.RemoveAllComponents<T>();
        }

        internal void Update(float elapsedSeconds, float totalSeconds)
        {
            // [FOREACH PERFORMANCE] ALLOCATES GARBAGE
            foreach (var component in components.GetAll<IUpdateableComponent>())
                component.Update(elapsedSeconds, totalSeconds);
        }

        public virtual void DebugDraw(DebugRenderer renderer, float elapsedSeconds, float totalSeconds)
        {
            // Draw the center
            renderer.DebugDrawPoint(Body.Position);
        }

        public Scene Scene { get; private set; }

        public MainGame Game { get; private set; }

        /// <summary>
        /// User-defined entity data
        /// </summary>
        public object Tag { get; set; }

        public void Die()
        {
            Scene.RemoveEntity(this);
        }

        public EntityType EntityType { get; private set; }

        public void UnloadContent()
        {
            // [FOREACH PERFORMANCE] ALLOCATES GARBAGE
            foreach (var component in GetAllComponents<IUnloadContent>())
            {
                component.UnloadContent();
            }
        }
    }
}
