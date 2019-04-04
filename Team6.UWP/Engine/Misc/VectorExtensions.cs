using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Team6.Engine.Misc
{
    public static class VectorExtensions
    {
        public static Vector2 AngleToUnitVector(float angle)
        {
            return new Vector2((float)Math.Sin(angle), -(float)Math.Cos(angle));
        }

        public static float UnitVectorToAngle(this Vector2 vector)
        {
            return (float) Math.Atan2(vector.X, -vector.Y);
        }

        public static float DotClamp(this Vector2 vector1, Vector2 vector2, float min = -1, float max = 1)
        {
            float value = Vector2.Dot(vector1, vector2);
            return (value > max) ? max : (value < min) ? min : value;
        }

        public static float AngleBetween(this Vector2 unitVector, Vector2 otherUnitVector)
        {
            return (float)Math.Acos(DotClamp(unitVector, otherUnitVector));
        }

        public static float DistanceTo(this Vector2 vector1, Vector2 vector2)
        {
            return Vector2.Distance(vector1, vector2);
        }

        public static float DistanceSquaredTo(this Vector2 vector1, Vector2 vector2)
        {
            return Vector2.DistanceSquared(vector1, vector2);
        }

        public static Vector2 PolarToCartesian(float phi, float r)
        {
            Vector2 result = AngleToUnitVector(phi);
            return result * r;
        }

        public static Vector2 Normalized(this Vector2 v)
        {
            return Vector2.Normalize(v);
        }

    }
}
