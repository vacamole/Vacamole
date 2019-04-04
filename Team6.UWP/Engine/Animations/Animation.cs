using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team6.Engine.MemoryManagement;

namespace Team6.Engine.Animations
{
    public class Animation : DispatcherObject, IDisposable
    {
        private static ObjectPool<Animation> animationPool = new ObjectPool<Animation>(() => new Animation());

        private Animation()
        {
        }

        /// <summary>
        /// Animates a value from <paramref name="start"/> to <paramref name="end"/> in <paramref name="duration"/> seconds.
        /// A easing function can be used to smooth the animation
        /// </summary>
        /// <param name="start">The start value of the animation</param>
        /// <param name="end">The end value of the animation</param>
        /// <param name="duration">The duration of the animation, in seconds</param>
        /// <param name="onTick">The function that is called with the value on each update</param>
        /// <param name="easingFunction">The easing function used to ease the value</param>
        /// <param name="isLooping">If true, the animation will keep playing</param>
        /// <param name="delay">The delay before the first execution</param>
        public static Animation Get(float start, float end, float duration, bool isLooping, Action<float> onTick, Func<float, float> easingFunction, float delay = 0f)
        {
            PooledObject<Animation> pooledAnimation = animationPool.GetFree();
            Animation anim = pooledAnimation.Value;
            anim.start = start;
            anim.end = end;
            anim.duration = duration;
            anim.onTick = onTick;
            anim.easingFunction = easingFunction;
            anim.IsLooping = isLooping;
            anim.delay = delay;
            anim.poolItem = pooledAnimation;
            anim.IsFinished = false;
            anim.IsRunning = true;
            anim.OnFinished = null;
            anim.Reset();

            return anim;
        }

        private float elapsedSecondsSinceStart = 0f;
        private float start = 0f;
        private float end = 0f;
        private float duration = 0f;
        private float delay;
        private Func<float, float> easingFunction;
        private Action<float> onTick;

        private PooledObject<Animation> poolItem;

        public override void Update(float elapsedSeconds, float totalSeconds)
        {
            if (!IsRunning)
                return;

            elapsedSecondsSinceStart += elapsedSeconds;

            if (elapsedSecondsSinceStart >= delay)
            {
                float withoutDelay = elapsedSecondsSinceStart - delay;
                float inAnimation = withoutDelay / duration;
                inAnimation = easingFunction?.Invoke(inAnimation) ?? inAnimation;

                onTick(MathHelper.Lerp(start, end, inAnimation));

                if (withoutDelay >= duration)
                {
                    if (!IsLooping)
                    {
                        IsFinished = true;
                        OnFinished?.Invoke();
                        // we allow to reset it
                        if (IsFinished)
                            poolItem.Dispose();
                    }
                    delay = 0;
                    elapsedSecondsSinceStart -= duration;
                }
            }
        }

        public bool IsLooping { get; private set; }
        public Action OnFinished { get; private set; }

        /// <summary>
        /// Fluent API for setting <see cref="OnFinished"/>
        /// </summary>
        /// <param name="onFinished">The action that is called upon finishing of the animation</param>
        /// <returns>The animation itself</returns>
        public Animation Then(Action onFinished)
        {
            this.OnFinished = onFinished;
            return this;
        }

        public Animation Set(Action<Animation> propertySetter)
        {
            propertySetter(this);
            return this;
        }

        /// <summary>
        /// Resets the elapsed seconds
        /// </summary>
        public void Reset()
        {
            if (poolItem.Value == null)
                throw new NotSupportedException("Animation is a pooled object, you cannot directly recycle animations if they are not looped. Use Get to obtain a \"new\" one.");

            elapsedSecondsSinceStart = 0;
        }

        public void Dispose()
        {
            poolItem.Dispose();
        }
    }
}
