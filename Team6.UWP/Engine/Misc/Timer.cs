using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Team6.Engine.Misc
{
    /// <summary>
    /// A wrapped float that provides timing "events" at a regular interval.
    /// </summary>
    public struct Timer
    {
        public float ElapsedTimeSinceLastTick { get; private set; }
        public float BetweenTicks { get; private set; }

        public Timer(float betweenTicks)
        {
            this.ElapsedTimeSinceLastTick = 0;
            this.BetweenTicks = betweenTicks;
        }

        /// <summary>
        /// Advances <see cref="ElapsedTimeSinceLastTick"/> and returns true, if a tick happens during this update.
        /// </summary>
        /// <param name="elapsedSeconds">The elapsed seconds since the last frame</param>
        /// <returns>True if a tick happens during this update otheriwse false.</returns>
        public bool Tick(float elapsedSeconds)
        {
            ElapsedTimeSinceLastTick += elapsedSeconds;

            bool result = ElapsedTimeSinceLastTick >= BetweenTicks;
            if (result)
                ElapsedTimeSinceLastTick -= BetweenTicks;

            return result;
        }

        public void Reset()
        {
            ElapsedTimeSinceLastTick = 0;
        }
    }
}
