using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Team6.Engine.Components;
using Team6.Engine.Content;

namespace Team6.Engine.Graphics2d
{
    public class AnimatedSpriteComponent : SpriteComponent, IUpdateableComponent, ILoadContent
    {
        private AnimationDefintion config;
        private readonly Dictionary<string, AnimationDefintion.Animation> animByName;
        private readonly int[] neutralFrames;
        private float totalSecondsSinceLastSwitch = 0;

        public AnimationDefintion.Animation CurrentAnimation { get; private set; }

        public AnimationDefintion.Animation NextAnimation { get; private set; } = null;


        public AnimatedSpriteComponent(AnimationDefintion config, Vector2 size, Vector2 pivot = default(Vector2),
            float layerDepth = 0, string name = null) : base(config.AssetName, size, pivot, layerDepth: layerDepth)
        {
            this.config = config;
            this.Name = name;

            neutralFrames = config.Neutral;

            animByName = config.Animations.ToDictionary(a => a.Name);
            CurrentAnimation = animByName.Values.First();

            if (!animByName.Values.All(a => neutralFrames.Intersect(a.FrameNumbers).Any()))
                throw new NotSupportedException("All animations must have a neutral frame in them!");
        }

        public new void LoadContent()
        {
            base.LoadContent();
            SetSourceRectangle(0);
        }

        public void SwitchTo(string animationName)
        {
            if (animationName != CurrentAnimation.Name)
                NextAnimation = animByName[animationName];
        }

        public void Update(float elapsedSeconds, float totalSeconds)
        {
            if (CurrentAnimation.IsScaledWithEntity)
            {
                float speed = Entity.Body.LinearVelocity.Length();
                float factor = CurrentAnimation.EntitySpeedOffset + speed * CurrentAnimation.EntitySpeedScale;
                totalSecondsSinceLastSwitch += elapsedSeconds * factor;
            }
            else
                totalSecondsSinceLastSwitch += elapsedSeconds;

            float currentDuration = CurrentAnimation.Duration;
            int inAnimationNum = (int)((totalSecondsSinceLastSwitch % currentDuration) / currentDuration * CurrentAnimation.FrameNumbers.Length);
            inAnimationNum = Math.Min(CurrentAnimation.FrameNumbers.Length - 1, Math.Max(0, inAnimationNum));
            int inImageFrameNum = CurrentAnimation.FrameNumbers[inAnimationNum];

            SetSourceRectangle(inImageFrameNum);

            if (NextAnimation != null && neutralFrames.Contains(inImageFrameNum))
            {
                CurrentAnimation = NextAnimation;
                NextAnimation = null;
                totalSecondsSinceLastSwitch = 0f;
            }
        }

        private void SetSourceRectangle(int inImageFrameNum)
        {
            var tileWidth = this.texture.Width / config.TilingX;
            var tileHeight = this.texture.Height / config.TilingY;

            var x = inImageFrameNum % config.TilingX;
            var y = (inImageFrameNum / config.TilingX) % config.TilingY;

            this.sourceRectangle = new Rectangle(x * tileWidth, y * tileHeight, tileWidth, tileHeight);
        }
    }
}