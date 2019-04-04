using System;
using Microsoft.Xna.Framework;
using Team6.Engine.Components;
using Team6.Engine.Misc;

namespace Team6.Game.Components.AI
{
    public class StayInBarnComponent : Component, IUpdateableComponent
    {
        private Vector2 barnPosition;

        public StayInBarnComponent(Vector2 barnPosition)
        {
            this.barnPosition = barnPosition;
        }

        public void Update(float elapsedSeconds, float totalSeconds)
        {
            Vector2 newDirection = barnPosition - Entity.Body.Position;
            newDirection.Normalize();
            float desiredRotation = newDirection.UnitVectorToAngle();
            float actualRotation = Entity.Body.Rotation;

            float angleDistance = desiredRotation - actualRotation;
            angleDistance = MathHelper.WrapAngle(angleDistance);

            if (angleDistance > 0)
                Entity.Body.AngularVelocity = Math.Min(angleDistance / elapsedSeconds, 6);
            else if (angleDistance < 0)
                Entity.Body.AngularVelocity = Math.Max(angleDistance / elapsedSeconds, -6);
            else
                Entity.Body.AngularVelocity = 0;

            if ((barnPosition - Entity.Body.Position).LengthSquared() < 0.5*0.5)
            {
                this.Entity.Die();
            }

            Entity.Body.LinearVelocity = VectorExtensions.AngleToUnitVector(Entity.Body.Rotation);
        }
    }
}
