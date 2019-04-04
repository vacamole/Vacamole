using FarseerPhysics.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team6.Game.Misc;
using Microsoft.Xna.Framework;
using Team6.Engine.Entities;
using Team6.Engine.Misc;

namespace Team6.Engine.Graphics2d
{
    public static class PhysicsEngineExtensions
    {
        public static RaycastResult RayCast(this World world, Vector2 startPoint, Vector2 endPoint)
        {
            float lastUsedFraction = 1f;
            RaycastResult result = new RaycastResult();
            float sensingDistance = startPoint.DistanceTo(endPoint);

            world.RayCast((fixture, point, normal, fraction) =>
            {
                if (fixture.CollisionCategories == GameConstants.CollisionCategorySensor) // Ignore sensor fixtures
                    return lastUsedFraction;

                result.Success = true;
                result.Point = point;
                result.Normal = normal;
                result.SensedEntity = (Entity)fixture.Body.UserData;
                result.DistanceToCollision = fraction * sensingDistance;
                lastUsedFraction = fraction;
                return fraction; // we return fraction as this will find the neareast intersection as efficient as possible (http://www.iforce2d.net/b2dtut/world-querying)
            }, startPoint, endPoint);

            return result;
        }
        public static RaycastResult Raycast(this World world, Body startBody, float sensingDistance)
        {
            return RayCast(world, startBody.Position, startBody.Position + VectorExtensions.AngleToUnitVector(startBody.Rotation) * sensingDistance);
        }

        public struct RaycastResult
        {
            public bool Success { get; set; }
            public Vector2 Point { get; set; }
            public Vector2 Normal { get; internal set; }
            public Entity SensedEntity { get; internal set; }
            public float DistanceToCollision { get; internal set; }
        }
    }
}
