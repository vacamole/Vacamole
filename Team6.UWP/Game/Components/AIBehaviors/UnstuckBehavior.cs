using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team6.Engine.Components;
using Team6.Engine.Misc;

namespace Team6.Game.Components.AIBehaviors
{
    public class UnstuckBehavior<T> : StateBasedAIBehaviorComponent<T>, IDebugDrawable
    {
        private RaycastSensorComponent obstacleSensor;

        private float timeStuck = 0;
        private Vector2 lastLocation = new Vector2(0f, 0f);

        public UnstuckBehavior(IEnumerable<T> states, IEnumerable<OutgoingState<T>> outgoingStates) : base(states, outgoingStates)
        {
        }

        public UnstuckBehavior(T state, params OutgoingState<T>[] outgoingStates) : base(state, outgoingStates)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            obstacleSensor = Entity.GetComponent<RaycastSensorComponent>();
        }

        protected override void UpdateBehaviour(float elapsedSeconds, float totalSeconds)
        {
            if (Vector2.Distance(lastLocation, Entity.Body.Position) > 0.3)
            {
                lastLocation = Entity.Body.Position;
                timeStuck = 0;
            }
            timeStuck += elapsedSeconds;

            if (timeStuck > 2)
            {
                if (obstacleSensor.SensedEntity != null && obstacleSensor.SensedEntity.Body.BodyType == FarseerPhysics.Dynamics.BodyType.Static)
                {
                    Entity.Body.ApplyLinearImpulse(RandomExt.GetRandomVector(10, 20));
                }
            }
        }
        public virtual void DebugDraw(DebugRenderer renderer, float elapsedSeconds, float totalSeconds)
        {
            renderer.DebugDrawPoint(lastLocation, Color.Green);
        }
    }
}
