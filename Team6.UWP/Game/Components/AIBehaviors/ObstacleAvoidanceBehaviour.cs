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
    public class ObstacleAvoidanceBehaviour<T> : StateBasedAIBehaviorComponent<T>
    {
        private MovementBasicsComponent movement;
        private RaycastSensorComponent obstacleSensor;

        public ObstacleAvoidanceBehaviour(IEnumerable<T> states, IEnumerable<OutgoingState<T>> outgoingStates) : base(states, outgoingStates)
        {
        }

        public ObstacleAvoidanceBehaviour(T state, params OutgoingState<T>[] outgoingStates) : base(state, outgoingStates)
        {
        }
        
        public override void Initialize()
        {
            base.Initialize();
            movement = Entity.GetComponent<MovementBasicsComponent>();
            obstacleSensor = Entity.GetComponent<RaycastSensorComponent>();
        }

        protected override void UpdateBehaviour(float elapsedSeconds, float totalSeconds)
        {
            if (obstacleSensor.SensedEntity != null && obstacleSensor.SensedEntity.Body.BodyType == FarseerPhysics.Dynamics.BodyType.Static)
            {
                float obstacleDistance = Vector2.Dot(Entity.Body.Position - obstacleSensor.Point, obstacleSensor.Normal);

                if (obstacleDistance < AvoidingDistance)
                {
                    Vector2 newDirection = Vector2.Reflect(VectorExtensions.AngleToUnitVector(Entity.Body.Rotation), obstacleSensor.Normal);
                    movement.Steer(newDirection, Weight);
                }
            }
        }

        public float AvoidingDistance { get; set; } = 2f;
    }
}
