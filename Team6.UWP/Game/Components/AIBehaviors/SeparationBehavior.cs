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
    public class SeparationBehavior<T> : StateBasedAIBehaviorComponent<T>
    {
        MovementBasicsComponent movement;

        public SeparationBehavior(T state, params OutgoingState<T>[] outgoingStates) : base(state, outgoingStates)
        {
        }

        public SeparationBehavior(IEnumerable<T> states, IEnumerable<OutgoingState<T>> outgoingStates) : base(states, outgoingStates)
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

            // [FOREACH PERFORMANCE] ALLOCATES GARBAGE
            foreach (var t in TargetPositions)
            {
                Vector2 separationDirection = Entity.Body.Position - t;
                float dist = separationDirection.Length();

                if (dist < MaxSepDist)
                {
                    direction += separationDirection;
                }
            }

            if (direction.Length() > 0f)
            {
                direction.Normalize();
                movement.Steer(direction, Weight);
            }
        }

        public IEnumerable<Vector2> TargetPositions { get; set; }

        public float MaxTurningVelocity { get; set; } = 2f;
        public float MaxSepDist { get; set; } = 0.5f;
    }

}
