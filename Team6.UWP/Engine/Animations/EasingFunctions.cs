using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Team6.Engine.Animations
{
    public static class EasingFunctions
    {
        public static float LinearIn(float t)
        {
            return t;
        }

        public static float QuadIn(float t)
        {
            return (float)Math.Pow(t, 2);
        }

        public static float CubicIn(float t)
        {
            return (float)Math.Pow(t, 3);
        }

        public static Func<float, float> PowIn(float power)
        {
            return (t) => (float)Math.Pow(t, power);
        }

        public static float SinIn(float t)
        {
            return 1f - (float)Math.Cos(t - MathHelper.PiOver2);
        }

        public static Func<float, float> ToEaseOut(Func<float, float> easeInFunction)
        {
            return (t) => 1 - easeInFunction(1 - t);
        }

        public static Func<float, float> ToLoop(Func<float, float> easeInFunction)
        {
            return (t) => t > 0.5f ? 1 - easeInFunction(1 - t / 0.5f) : easeInFunction(t / 0.5f);
        }
    }
}
