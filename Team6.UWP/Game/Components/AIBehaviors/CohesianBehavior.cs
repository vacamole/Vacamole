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
    public class CohesionBehavior<T> : StateBasedAIBehaviorComponent<T>
    {
        MovementBasicsComponent movement;

        public CohesionBehavior(T state, params OutgoingState<T>[] outgoingStates) : base(state, outgoingStates)
        {
        }

        public CohesionBehavior(IEnumerable<T> states, IEnumerable<OutgoingState<T>> outgoingStates) : base(states, outgoingStates)
        {
        } 

        public override void Initialize()
        {
            base.Initialize();
            movement = Entity.GetComponent<MovementBasicsComponent>();
        }

        protected override void UpdateBehaviour(float elapsedSeconds, float totalSeconds)
        {
            Vector2 centerOfMass = Vector2.Zero;
            int count = TargetPositions.Count();

            if (count > 0)
            {
                // [FOREACH PERFORMANCE] ALLOCATES GARBAGE
                foreach (var t in TargetPositions)
                {
                    centerOfMass += t;
                }

                centerOfMass = centerOfMass / count;

                Vector2 direction = centerOfMass - Entity.Body.Position;
                direction.Normalize();

                movement.Steer(direction, Weight);
            }
        }
        
        public IEnumerable<Vector2> TargetPositions { get; set; }

        public float MaxTurningVelocity { get; set; } = 2f;
    }

}
