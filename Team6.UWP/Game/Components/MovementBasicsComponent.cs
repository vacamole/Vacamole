using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team6.Engine.Components;
using Team6.Engine.Misc;

namespace Team6.Game.Components
{
    class MovementBasicsComponent : Component, IUpdateableComponent
    {
        private Vector2 direction;
        private float totalWeight;
        private MovementState state = MovementState.Steering;

        public void Steer(Vector2 direction, float weight)
        {
            state = MovementState.Steering;
            
            if (direction.LengthSquared() > 0)
                direction.Normalize();
            direction *= weight;
            this.direction += direction;
            this.totalWeight += weight;
        }
        public void Slide(Vector2 direction)
        {
            state = MovementState.Sliding;

            direction.Normalize();
            direction *= Speed;
            Entity.Body.LinearVelocity = direction;
        }

        public void Update(float elapsedSeconds, float totalSeconds)
        {
            if (state == MovementState.Steering)
            {
                direction /= totalWeight;
                direction.Normalize();

                float angleDistance = MathHelper.WrapAngle(direction.UnitVectorToAngle() - Entity.Body.Rotation);
                LookInMovingDirection(elapsedSeconds, angleDistance, TurningSpeed);

                Move(elapsedSeconds, Speed);

                // reset for next Update
                totalWeight = 0;
                direction = Vector2.Zero;
            }            
        }

        private void Move(float elapsedSeconds, float desiredSpeed)
        {
            Vector2 desiredVelocity = VectorExtensions.AngleToUnitVector(Entity.Body.Rotation);
            desiredVelocity *= desiredSpeed;

            float drag = 0.1f;
            Entity.Body.LinearVelocity = drag * desiredVelocity + (1.0f - drag) * Entity.Body.LinearVelocity;
        }

        private void LookInMovingDirection(float elapsedSeconds, float angleDistance, float turningSpeed)
        {
            float drag = 0.1f;

            if (angleDistance > 0)
                Entity.Body.AngularVelocity = drag * Math.Min(angleDistance / elapsedSeconds, turningSpeed) + (1.0f - drag) * Entity.Body.AngularVelocity;
            else if (angleDistance < 0)
                Entity.Body.AngularVelocity = drag * Math.Max(angleDistance / elapsedSeconds, -turningSpeed) + (1.0f - drag) * Entity.Body.AngularVelocity;
            else
                Entity.Body.AngularVelocity = (1.0f - drag) * Entity.Body.AngularVelocity;
        }

        private void Arrive(ref Vector2 desiredVelocity, float distance, float maxSpeed)
        {
            float slowingRadius = 1f;
            if (desiredVelocity.Length() > 0)
            {
                desiredVelocity = VectorExtensions.AngleToUnitVector(Entity.Body.Rotation);
            }

            if (distance < slowingRadius)
            {
                // Inside the slowing area
                desiredVelocity.Normalize();
                desiredVelocity = desiredVelocity * maxSpeed * (distance / slowingRadius);
            }
            else
            {
                // Outside the slowing area.
                desiredVelocity.Normalize();
                desiredVelocity = desiredVelocity * maxSpeed;
            }

        }

        public float Speed { get; set; } = 1f;
        public float TurningSpeed { get; set; } = 2f;

        public enum MovementState
        {
            Steering, Sliding
        }
    }
}
