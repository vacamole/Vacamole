using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team6.Engine.Animations;
using Team6.Engine.Components;
using Team6.Engine.Entities;
using Team6.Engine.Misc;

namespace Team6.Game.Components.AIBehaviors
{
    public class FollowingBehavior<T> : StateBasedAIBehaviorComponent<T>
    {
        MovementBasicsComponent movement;
        Timer timerForChange;

        public FollowingBehavior(T state, float minimumFollowingTime = 1.5f, params OutgoingState<T>[] outgoingStates) : base(state, outgoingStates)
        {
            timerForChange = new Timer(minimumFollowingTime);
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

            Vector2 direction = EntityToFollow.Body.Position - position;

            movement.Speed = Speed;
            movement.TurningSpeed = MaxTurningSpeed;
            movement.Steer(direction, Weight);
        }

        public float Speed { get; set; } = 0.7f;
        public float MaxTurningSpeed { get; set; } = 2f;

        public Entity EntityToFollow { get; set; }


        public override void Reactivated()
        {
            timerForChange.Reset();
        }
    }

}
