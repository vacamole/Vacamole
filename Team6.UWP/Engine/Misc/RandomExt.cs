using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Team6.Engine.Misc
{
    public static class RandomExt
    {
        public static Random r = new Random();

        /// <summary>
        /// Returns a random integer in the specified range
        /// </summary>
        public static int GetRandomInt(int min, int max)
        {
            return r.Next(min, max);
        }

        /// <summary>
        /// Returns a random integer in the specified range, but excludes certain numbers
        /// </summary>
        public static int GetRandomInt(int min, int max, params int[] exclude)
        {
            int randomValue = r.Next(min, max - exclude.Length);

            // [FOREACH PERFORMANCE] Should not allocate garbage
            foreach (int excludedValue in exclude)
            {
                if (randomValue >= excludedValue)
                    randomValue++;
                else
                    break;
            }
            
            return randomValue;
        }

        /// <summary>
        /// Returns a random float in the specified range
        /// </summary>
        public static float GetRandomFloat(float min, float max)
        {
            return r.NextFloat(min, max);
        }

        /// <summary>
        /// Returns a random float in the specified range
        /// </summary>
        public static float NextFloat(this Random random, float min, float max)
        {
            return (float)random.NextDouble() * (max - min) + min;
        }

        /// <summary>
        /// Returns a radian angle in the specified interval. Default interval is between zero and 2*PI.
        /// </summary>
        public static float GetRandomAngle(float min = 0, float max = MathHelper.TwoPi)
        {
            return r.NextFloat(min, max);
        }

        /// <summary>
        /// Returns a radian angle in the specified interval. Default interval is between zero and 2*PI.
        /// </summary>
        public static float GetRandomAngle(this Random random, float min = 0, float max = MathHelper.TwoPi)
        {
            return random.NextFloat(min, max);
        }

        public static Vector2 GetRandomVector(this Random random, float minLength, float maxLength)
        {
            Vector2 result = VectorExtensions.AngleToUnitVector(random.GetRandomAngle());
            return result * random.NextFloat(minLength, maxLength);
        }

        public static Vector2 GetRandomVector(float minLength, float maxLength)
        {
            Vector2 result = VectorExtensions.AngleToUnitVector(r.GetRandomAngle());
            return result * r.NextFloat(minLength, maxLength);
        }
    }
}
