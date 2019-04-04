using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team6.Engine.Animations;
using Team6.Engine.Components;
using Team6.Engine.Misc;

namespace Team6.Game.Components.AIBehaviors
{
    public class VelocityMatchBehavior<T> : StateBasedAIBehaviorComponent<T>
    {
        MovementBasicsComponent movement;

        public VelocityMatchBehavior(T state, params OutgoingState<T>[] outgoingStates) : base(state, outgoingStates)
        {
        }

        public VelocityMatchBehavior(IEnumerable<T> states, IEnumerable<OutgoingState<T>> outgoingStates) : base(states, outgoingStates)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            movement = Entity.GetComponent<MovementBasicsComponent>();
        }

        protected override void UpdateBehaviour(float elapsedSeconds, float totalSeconds)
        {
            Vector2 direction = Vector2.Zero;
            float speed = Entity.Body.LinearVelocity.Length();
            int count = TargetVelocities.Count();

            if (count > 0)
            {
                // [FOREACH PERFORMANCE] ALLOCATES GARBAGE
                foreach (var velocity in TargetVelocities)
                {
                    Vector2 a = velocity - Entity.Body.LinearVelocity;

                    a = a / TimeToMatch;

                    direction += a;
                }

                direction /= count;
                speed = direction.Length();
                direction.Normalize();

                movement.Steer(direction, Weight);
            }
        }

        public IEnumerable<Vector2> TargetVelocities { get; set; }

        public float MaxTurningVelocity { get; set; } = 2f;
        public float TimeToMatch { get; set; } = 1f;
    }

}
