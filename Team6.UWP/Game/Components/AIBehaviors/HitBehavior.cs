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
    public class HitBehavior<T> : StateBasedAIBehaviorComponent<T>
    {
        MovementBasicsComponent movement;
        Timer timerForChange;

        public HitBehavior(T state, float minimumFleeingTime = 1f, params OutgoingState<T>[] outgoingStates) : base(state, outgoingStates)
        {
            timerForChange = new Timer(minimumFleeingTime);
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

            Vector2 direction = (position - FleeingPoint);

            movement.Speed = Speed;
            movement.TurningSpeed = MaxTurningSpeed;
            movement.Slide(direction);
        }

        public float Speed { get; set; } = 2.5f;

        public Vector2 FleeingPoint { get; set; }

        public float MaxTurningSpeed { get; set; } = 2f;


        public override void Reactivated()
        {
            timerForChange.Reset();
        }
    }

}
