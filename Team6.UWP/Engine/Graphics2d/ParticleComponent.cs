
using Microsoft.Xna.Framework;
using System;
using Team6.Engine.Graphics2d;
using Team6.Engine.MemoryManagement;
using Microsoft.Xna.Framework.Graphics;
using Team6.Engine.Entities;
using Comora;
using Team6.Engine.UI;

namespace Team6.Engine.Components
{
    public class ParticleComponent<T> : Component, IUpdateableComponent, IDrawableComponent where T : Particle<T>, new()
    {
        private T[] activeParticles;
        private volatile int activeParticleCount = 0;
        private readonly ObjectPool<T> particlePool;

        public ParticleComponent(int size, string name) : base()
        {
            this.particlePool = ObjectPools.CreateOrGet(() => new T());
            this.activeParticles = new T[size];
            Name = name;
        }


        public void Spawn(Action<T> intializer, Action<float, T> onTick)
        {
            if (activeParticleCount < activeParticles.Length)
            {
                // GET FREE SPOT
                int index = -1;
                for (int i = 0; index < 0 && i < activeParticles.Length; i++)
                    if (activeParticles[i] == null)
                        index = i;

                // OBTAIN PARTICLE and initialize
                var newParticle = particlePool.GetFree();
                newParticle.Value.Initialize(index, this, newParticle, intializer, onTick);
                activeParticles[index] = newParticle.Value;
            }
        }

        public void Update(float elapsedSeconds, float totalSeconds)
        {
            for (int i = 0; i < activeParticles.Length; i++)
                activeParticles[i]?.Update(elapsedSeconds);
        }


        public void Draw(SpriteBatch spriteBatch, float elapsedSeconds, float totalSeconds)
        {
            for (int i = 0; i < activeParticles.Length; i++)
                activeParticles[i]?.Draw(spriteBatch, elapsedSeconds, totalSeconds);
        }

        internal void DestroyParticle(T particle)
        {
            activeParticles[particle.ParticleIndex] = null;
            activeParticleCount--;
        }
    }

    public class TextParticle : Particle<TextParticle>
    {
        private Camera camera;

        internal override void Initialize(int particleIndex, ParticleComponent<TextParticle> parent, PooledObject<TextParticle> newParticle, Action<TextParticle> intializer, Action<float, TextParticle> onTick)
        {
            base.Initialize(particleIndex, parent, newParticle, intializer, onTick);
            camera = parent.Entity.Game.EngineComponents.Get<Renderer2d>().GameCamera;
            Position = parent.Entity.Body.Position;
        }

        internal override void Draw(SpriteBatch spriteBatch, float elapsedSeconds, float totalSeconds)
        {
            float scale = camera.AbsoluteScale.X;

            float fontSize = (TextHeight * scale) * 72f / 96f; //px to pt

            Font.DrawStringScaled(spriteBatch, fontSize, this.Text,
                Position + Offset, Color * Opacity, 1f / scale);
        }

        public string Text { get; set; }
        public Vector2 Offset { get; set; }
        public Vector2 Position { get; set; }
        public MultiSizeSpriteFont Font { get; set; }
        public float TextHeight { get; set; }
        public Color Color { get; set; } = Color.BlanchedAlmond;
        public float Opacity { get; set; } = 1;
    }

    public abstract class Particle<T> : IDisposable where T : Particle<T>, new()
    {
        private PooledObject<T> poolObject;
        private Action<float, T> onTick;


        public void Dispose()
        {
            Parent.DestroyParticle((T)this);
            poolObject.Free();
        }

        public virtual void Update(float elapsedSeconds)
        {
            TotalElapsedSeconds += elapsedSeconds;
            onTick(TotalElapsedSeconds, (T)this);
        }



        internal virtual void Initialize(int particleIndex, ParticleComponent<T> parent, PooledObject<T> newParticle, Action<T> intializer, Action<float, T> onTick)
        {
            this.poolObject = newParticle;
            this.Parent = parent;
            TotalElapsedSeconds = 0;
            ParticleIndex = particleIndex;

            intializer((T)this);
            this.onTick = onTick;
        }

        internal abstract void Draw(SpriteBatch spriteBatch, float elapsedSeconds, float totalSeconds);

        public int ParticleIndex { get; private set; }

        public float TotalElapsedSeconds { get; set; }
        public ParticleComponent<T> Parent { get; private set; }
    }

}