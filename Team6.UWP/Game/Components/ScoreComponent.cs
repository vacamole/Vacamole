using Microsoft.Xna.Framework;
using System;
using Team6.Engine.Animations;
using Team6.Engine.Components;

namespace Team6.Game.Components
{
    public class ScoreComponent : ParticleComponent<TextParticle>
    {
        private int nextPoints;

        public Color Color { get; private set; }

        public ScoreComponent(Color color, string name) : base(8, name)
        {
            this.Color = color;
        }

        public void ShowScore(int points)
        {
            // hack to create no capture with garbage for initializer
            nextPoints = points;
            this.Spawn(Initializer, OnTick);
        }

        private void OnTick(float time, TextParticle particle)
        {
            const float totalTime = 0.9f;
            const float inTime = 0.2f;
            const float outTime = 0.5f;
            particle.Offset = new Vector2(-0.2f, -0.4f + (-0.6f) * time / totalTime);
            particle.Position = this.Entity.Body.Position;
            if (time < inTime)
                particle.Opacity = EasingFunctions.QuadIn(time / inTime);
            else if (time > totalTime - outTime)
            {
                particle.Opacity = 1 - EasingFunctions.QuadIn((time - totalTime + outTime) / outTime);
            }


            if (time >= totalTime)
                particle.Dispose(); // kill at 2 seconds
        }

        private void Initializer(TextParticle particle)
        {
            particle.Text = "+" + nextPoints.ToString();
            particle.Font = this.Entity.Scene.MainFont;
            particle.Color = Color;
            particle.Offset = new Vector2(-0.2f, -0.4f);
            particle.Opacity = 0;
            particle.TextHeight = 0.5f;
        }
    }
}