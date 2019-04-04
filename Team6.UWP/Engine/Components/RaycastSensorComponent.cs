using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team6.Engine.Entities;
using Team6.Engine.Misc;
using Team6.Game.Misc;
using Team6.Engine.Graphics2d;

namespace Team6.Engine.Components
{
    public class RaycastSensorComponent : Component, IUpdateableComponent, IDebugDrawable
    {
        private World world;
        public float SensingDistance { get; set; }

        public RaycastSensorComponent(float distance)
        {
            this.SensingDistance = distance;
        }

        public override void Initialize()
        {
            base.Initialize();
            world = Entity.Scene.World;
        }

        public void Update(float elapsedSeconds, float totalSeconds)
        {
            var body = Entity.Body;
            SensedEntity = null;

            var raycastResult = world.Raycast(body, SensingDistance);

            if(raycastResult.Success)
            {
                Point = raycastResult.Point;
                Normal = raycastResult.Normal;
                SensedEntity = raycastResult.SensedEntity;
                DistanceToCollision = raycastResult.DistanceToCollision;
            }
        }

        public void DebugDraw(DebugRenderer renderer, float elapsedSeconds, float totalSeconds)
        {
            Vector2 position = Entity.Body.Position;
            Vector2 lookPosition = position + VectorExtensions.AngleToUnitVector(Entity.Body.Rotation) * SensingDistance;
            var debugShapeColor = SensedEntity != null ? Color.Aqua : Color.FloralWhite;
            renderer.DebugDrawLine(position, lookPosition, color: debugShapeColor);

            if (SensedEntity != null)
            {
                Vector2 positionInWorldCoords = Point;
                renderer.DebugDrawPoint(positionInWorldCoords, debugShapeColor);
                renderer.DebugDrawLine(positionInWorldCoords, positionInWorldCoords + Normal, color: debugShapeColor);
            }
        }

        public Entity SensedEntity { get; private set; }
        public Vector2 Point { get; private set; }
        public Vector2 Normal { get; private set; }

        public float DistanceToCollision { get; private set; }
    }
}
