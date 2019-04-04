using FarseerPhysics.Dynamics;
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
    public class WanderingBehavior<T> : StateBasedAIBehaviorComponent<T>
    {
        MovementBasicsComponent movement;
        Timer timerForChange;

        public WanderingBehavior(T state, float minimumTimeInState = 3f, params OutgoingState<T>[] outgoingStates) : base(state, outgoingStates)
        {
            timerForChange = new Timer(minimumTimeInState);
        }

        public override void Initialize()
        {
            base.Initialize();
            movement = Entity.GetComponent<MovementBasicsComponent>();
        }

        protected override void UpdateBehaviour(float elapsedSeconds, float totalSeconds)
        {
            // Switch to next state after some time
            if (timerForChange.Tick(elapsedSeconds))
                SwitchToOutputState();

            Body body = this.Entity.Body;
            var position = body.Position;
            var rotation = body.Rotation;

            Vector2 desiredPosition = position + VectorExtensions.AngleToUnitVector(rotation) * CircleCenterOffset;
            desiredPosition += VectorExtensions.AngleToUnitVector(RandomExt.GetRandomAngle()) * CircleRadius;

            Vector2 direction = (desiredPosition - position);

            movement.Speed = RandomExt.GetRandomFloat(WanderingSpeedMin, WanderingSpeedMax);
            movement.TurningSpeed = MaxTurningSpeed;
            movement.Steer(direction, Weight);
        }

        public float CircleCenterOffset { get; set; } = 1f;

        public float CircleRadius { get; set; } = 0.5f;

        public float WanderingSpeedMin { get; set; } = 0.5f;
        public float WanderingSpeedMax { get; set; } = 1.0f;
        public float MaxTurningSpeed { get; set; } = 5f;

    }

}
