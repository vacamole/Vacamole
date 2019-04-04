using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Team6.Engine.Graphics2d;

namespace Team6.Engine.UI
{
    class HUDAnimatedComponent : HUDComponent
    {
        private AnimationDefintion config;
        private readonly Dictionary<string, AnimationDefintion.Animation> animByName;
        private readonly int[] neutralFrames;

        public AnimationDefintion.Animation CurrentAnimation { get; private set; }

        public AnimationDefintion.Animation NextAnimation { get; private set; } = null;

        public HUDAnimatedComponent(AnimationDefintion config, Vector2 size, 
            Vector2 origin = default(Vector2), Rectangle? sourceRectangle = default(Rectangle?),  float layerDepth = 0, 
            Vector2 offset = default(Vector2)) : base(config.AssetName, size, origin, sourceRectangle, layerDepth, offset)
        {
            this.config = config;


            neutralFrames = config.Neutral;

            animByName = config.Animations.ToDictionary(a => a.Name);
            CurrentAnimation = animByName.Values.First();

            if (!animByName.Values.All(a => neutralFrames.Intersect(a.FrameNumbers).Any()))
                throw new NotSupportedException("All animations must have a netural frame in them!");
        }

        public override void Draw(SpriteBatch spriteBatch, float elapsedSeconds, float totalSeconds)
        {
            var tileWidth = this.texture.Width / config.TilingX;
            var tileHeight = this.texture.Height / config.TilingY;

            int inAnimationNum = (int)((totalSeconds % CurrentAnimation.Duration) * CurrentAnimation.FrameNumbers.Length);
            int inImageFrameNum = CurrentAnimation.FrameNumbers[inAnimationNum];

            var x = inImageFrameNum % config.TilingX;
            var y = (inImageFrameNum / config.TilingX) % config.TilingY;

            this.sourceRectangle = new Rectangle(x * tileWidth, y * tileHeight, tileWidth, tileHeight);

            if (NextAnimation != null && neutralFrames.Contains(inImageFrameNum))
            {
                CurrentAnimation = NextAnimation;
                NextAnimation = null;
            }

            base.Draw(spriteBatch, elapsedSeconds, totalSeconds);
        }

        public void SwitchTo(string animationName)
        {
            NextAnimation = animByName[animationName];
        }
    }
}
