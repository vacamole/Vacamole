using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team6.Engine;
using Team6.Engine.Audio;
using Team6.Engine.Components;
using Team6.Engine.Content;
using Team6.Engine.Entities;
using Team6.Engine.Graphics2d;
using Team6.Engine.Misc;
using Team6.Game.Components.AIBehaviors;

namespace Team6.Game.Entities
{
    public class BoarEntity : AnimalEntity<BoarState>
    {
        
        private FollowingBehavior<BoarState> followingBehaviour;
        private FleeingBehavior<BoarState> fleeingBehaviour;
        private HitBehavior<BoarState> hitBehaviour;

        public BoarEntity(Scene scene, Vector2 location, float rotation = 0) : base(scene, location, CreateType(scene.Game.Content), rotation)
        {

        }
        public override void Initialize()
        {
            base.Initialize();

            Body.Mass = 65f;

            followingBehaviour = GetComponent<FollowingBehavior<BoarState>>();
            fleeingBehaviour = GetComponentByName<FleeingBehavior<BoarState>>("fleeing");
            hitBehaviour = GetComponentByName<HitBehavior<BoarState>>("beinghit");
            aiComponent.WithTrigger(FleeingTrigger).WithTrigger(FollowingTrigger);
        }

        private void FleeingTrigger(BoarState currentState, Entity entity, float elapsedSeconds, float totalElapsedSeconds)
        {
            // Only allow fleeing if the animal is not in folloing mode but when it is, only has a distance of smaller than 1 meters
            if (hearingSensor.SensedEntities.Any() &&
                (aiComponent.CurrentState != BoarState.Following || (followingBehaviour.EntityToFollow.Body.Position - Body.Position).Length() > 1f))
            {
                fleeingBehaviour.FleeingPoint = hearingSensor.SensedEntities.Average(e => e.Body.Position);
                if (currentState != BoarState.Fleeing)
                {
                    var randI = RandomExt.GetRandomInt(1, 5);
                    var delay = RandomExt.GetRandomFloat(0, 0.25f);
                    this.Scene.Dispatcher.Delay(delay,
                        () => GetComponent<AudioSourceComponent>().PlaySound($"Audio/PigScared{randI}"));
                }
                aiComponent.ChangeState(BoarState.Fleeing);
            }
        }

        private void FollowingTrigger(BoarState currentState, Entity entity, float elapsedSeconds, float totalElapsedSeconds)
        {
            if (aiComponent.CurrentState != BoarState.Fleeing && viewSensor.SensedEntities.Any())
            {
                followingBehaviour.EntityToFollow = viewSensor.SensedEntities.OrderBy(e => (e.Body.Position - this.Body.Position).LengthSquared()).First();
                aiComponent.ChangeState(BoarState.Following);
            }
        }

        public void Hit(Entity entity)
        {
            hitBehaviour.FleeingPoint = entity.Body.Position;
            aiComponent.ChangeState(BoarState.BeingHit);
        }

        private static AnimalType<BoarState> CreateType(ContentManager content)
        {
            float baseSpeed = 1.3f;
            float slowSpeed = 0.2f;
            float fleeingDuration = RandomExt.GetRandomFloat(1f, 1.5f);
            float followingDuration = RandomExt.GetRandomFloat(1f, 1.5f);
            float size = RandomExt.GetRandomFloat(1.23f, 1.32f);

            return AnimalTypeFactory.CreateAnimal(BoarState.Wandering)
                .WithAsset(content.LoadFromJson<AnimationDefintion>("Animations/boar_anim"), new Vector2(size, size))
                .WithCollisionShape(new[]{
                                new Vector2(0, 0.4f), new Vector2(-0.2f, 0.3f), new Vector2(-0.2f, -0.3f),
                                new Vector2(0, -0.4f), new Vector2(0.2f, -0.3f), new Vector2(0.2f, 0.3f)
                            })
                .WithSensing()
                .WithBehaviour(() => new WanderingBehavior<BoarState>(BoarState.Wandering, 1.5f, new OutgoingState<BoarState>(0.4f, BoarState.Rootling), new OutgoingState<BoarState>(0.6f, BoarState.Wandering))
                {
                    CircleRadius = 4f,
                    CircleCenterOffset = 2f,
                    WanderingSpeedMin = baseSpeed,
                    WanderingSpeedMax = baseSpeed * 1.1f
                })
                .WithBehaviour(() => new WanderingBehavior<BoarState>(BoarState.Rootling, 1.5f, new OutgoingState<BoarState>(0.2f, BoarState.Rootling), new OutgoingState<BoarState>(0.8f, BoarState.Wandering))
                {
                    MaxTurningSpeed = 0.1f,
                    WanderingSpeedMin = 0,
                    WanderingSpeedMax = slowSpeed
                })
                // Fleeing
                .WithBehaviour(() => new FleeingBehavior<BoarState>(BoarState.Fleeing, fleeingDuration, new OutgoingState<BoarState>(1f, BoarState.Wandering))
                {
                    Name = "fleeing",
                    Speed = baseSpeed * 3f
                })
                // Following
                .WithBehaviour(() => new FollowingBehavior<BoarState>(BoarState.Following, followingDuration, new OutgoingState<BoarState>(1f, BoarState.Wandering))
                {
                    Speed = baseSpeed
                })
                // Being hit
                .WithBehaviour(() => new HitBehavior<BoarState>(BoarState.BeingHit, 1f, new OutgoingState<BoarState>(1f, BoarState.Fleeing))
                {
                    Name = "beinghit",
                    Speed = 0.5f
                })
                .WithBehaviour(() => new ObstacleAvoidanceBehaviour<BoarState>(EnumExt.GetValues<BoarState>(), new OutgoingState<BoarState>[0]) { Weight = 10f })
                .DefaultAnimation("walking").StateToAnimation(BoarState.Rootling, "neutral").StateToAnimation(BoarState.BeingHit, "hit");
        }
    }




    public enum BoarState
    {
        Rootling,
        Wandering,
        Fleeing,
        BeingHit,
        Following
    }
}
