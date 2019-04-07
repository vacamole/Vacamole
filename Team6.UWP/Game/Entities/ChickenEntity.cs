using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
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
    public class ChickenEntity : AnimalEntity<ChickenState>
    {

        private FollowingBehavior<ChickenState> followingBehaviour;
        private FleeingBehavior<ChickenState> fleeingBehaviour;
        private PlayerFleeingBehavior<ChickenState> playerFleeingBehavior;
        private CohesionBehavior<ChickenState> cohesionBehaviour;
        private SeparationBehavior<ChickenState> separationBehaviour;
        private VelocityMatchBehavior<ChickenState> velocityBehaviour;
        private SensorComponent proximitySensor;
        private SensorComponent playerProximitySensor;

        public ChickenEntity(Scene scene, Vector2 location, float rotation = 0, int chickenType = -1) : base(scene, location, CreateType(scene.Game.Content, chickenType), rotation)
        {
            AddComponent(proximitySensor = new SensorComponent(
                "proximity",
                new PolygonShape(PolygonTools.CreateCapsule(2f, 0.1f, 1, 0.8f, 8), 1f),
                e => e is ChickenEntity
            ));

            AddComponent(playerProximitySensor = new SensorComponent(
                "playerproximity",
                new CircleShape(0.3f, 1f),
                e => e is PlayerEntity
            ));
        }
        public override void Initialize()
        {
            base.Initialize();

            Body.Mass = 3f;

            cohesionBehaviour = GetComponent<CohesionBehavior<ChickenState>>();
            separationBehaviour = GetComponent<SeparationBehavior<ChickenState>>();
            velocityBehaviour = GetComponent<VelocityMatchBehavior<ChickenState>>();

            followingBehaviour = GetComponent<FollowingBehavior<ChickenState>>();
            fleeingBehaviour = GetComponent<FleeingBehavior<ChickenState>>();
            playerFleeingBehavior = GetComponent<PlayerFleeingBehavior<ChickenState>>();
            aiComponent.WithTrigger(FleeingTrigger).WithTrigger(FollowingTrigger).WithTrigger(PlayerFleeingTrigger);

            cohesionBehaviour.TargetPositions = proximitySensor.SensedEntities.Select((e) => e.Body.Position);
            separationBehaviour.TargetPositions = proximitySensor.SensedEntities.Select((e) => e.Body.Position);
            velocityBehaviour.TargetVelocities = proximitySensor.SensedEntities.Select((e) => e.Body.LinearVelocity);
        }

        private void FleeingTrigger(ChickenState currentState, Entity entity, float elapsedSeconds, float totalElapsedSeconds)
        {
            // Only allow fleeing if the animal is not in folloing mode but when it is, only has a distance of smaller than 1 meters
            if (hearingSensor.SensedEntities.Any() &&
                (aiComponent.CurrentState != ChickenState.Following || (followingBehaviour.EntityToFollow.Body.Position - Body.Position).Length() > 1f))
            {
                fleeingBehaviour.FleeingPoint = hearingSensor.SensedEntities.Average(e => e.Body.Position);
                if (currentState != ChickenState.Fleeing)
                {
                    var randI = RandomExt.GetRandomInt(1, 4);
                    var delay = RandomExt.GetRandomFloat(0, 0.25f);
                    this.Scene.Dispatcher.Delay(delay,
                        () => GetComponent<AudioSourceComponent>().PlaySound($"Audio/chickenScared{randI}"));
                }

                aiComponent.ChangeState(ChickenState.Fleeing);
            }
        }

        private void FollowingTrigger(ChickenState currentState, Entity entity, float elapsedSeconds, float totalElapsedSeconds)
        {
            if (aiComponent.CurrentState != ChickenState.Fleeing && viewSensor.SensedEntities.Any())
            {
                followingBehaviour.EntityToFollow = viewSensor.SensedEntities.OrderBy(e => (e.Body.Position - this.Body.Position).LengthSquared()).First();
                aiComponent.ChangeState(ChickenState.Following);
            }
        }

        private void PlayerFleeingTrigger(ChickenState currentState, Entity entity, float elapsedSeconds, float totalElapsedSeconds)
        {
            if (aiComponent.CurrentState != ChickenState.AvoidingPlayer
                && aiComponent.CurrentState != ChickenState.Following
                && playerProximitySensor.SensedEntities.Any())
            {
                Entity player = playerProximitySensor.SensedEntities.OrderBy(e => (e.Body.Position - this.Body.Position).LengthSquared()).First();
                playerFleeingBehavior.FleeingPoint = player.Body.Position;

                aiComponent.ChangeState(ChickenState.AvoidingPlayer);
            }
        }

        public void Hit(Entity entity)
        {
            playerFleeingBehavior.FleeingPoint = entity.Body.Position;
            aiComponent.ChangeState(ChickenState.AvoidingPlayer);
        }

        private static AnimalType<ChickenState> CreateType(ContentManager content, int chickenType)
        {
            float baseSpeed = 2f;
            float slowSpeed = 0f;
            float fleeingDuration = RandomExt.GetRandomFloat(0.8f, 1.5f);
            float followingDuration = RandomExt.GetRandomFloat(1f, 1.5f);
            var asset = content.LoadFromJson<AnimationDefintion>("Animations/chicken_anim");
            float size = RandomExt.GetRandomFloat(0.9f, 1.2f);

            if (chickenType == -1 && RandomExt.GetRandomFloat(0,1) < 0.35f || chickenType == 1)
            {
                asset.AssetName = "chicken_walk-spritesheet_8x4_a512_orange";
                size = RandomExt.GetRandomFloat(0.5f, 0.7f);
            }


            return AnimalTypeFactory.CreateAnimal(ChickenState.Wandering)
                .WithAsset(asset, new Vector2(size, size))
                .WithCollisionShape(size * 0.13f, 0.6f)
                .WithSensing()
                .WithBehaviour(() => new WanderingBehavior<ChickenState>(ChickenState.Wandering, 3f, new OutgoingState<ChickenState>(0.4f, ChickenState.Picking), new OutgoingState<ChickenState>(0.6f, ChickenState.Wandering))
                {
                    CircleRadius = 2f,
                    CircleCenterOffset = 1f,
                    WanderingSpeedMin = baseSpeed * 0.35f,
                    WanderingSpeedMax = baseSpeed * 0.55f,
                    MaxTurningSpeed = 5f
                })
                .WithBehaviour(() => new WanderingBehavior<ChickenState>(ChickenState.Picking, 1.5f, new OutgoingState<ChickenState>(0.2f, ChickenState.Picking), new OutgoingState<ChickenState>(0.8f, ChickenState.Wandering))
                {
                    MaxTurningSpeed = 0.1f,
                    WanderingSpeedMin = 0,
                    WanderingSpeedMax = slowSpeed
                })
                .WithBehaviour(() => new FleeingBehavior<ChickenState>(ChickenState.Fleeing, fleeingDuration, new OutgoingState<ChickenState>(1f, ChickenState.Wandering))
                {
                    Weight = 10f,
                    Speed = baseSpeed * 1.5f,
                    MaxTurningSpeed = 10f
                })
                .WithBehaviour(() => new FollowingBehavior<ChickenState>(ChickenState.Following, followingDuration, new OutgoingState<ChickenState>(1f, ChickenState.Wandering))
                {
                    Weight = 30f,
                    Speed = baseSpeed * 0.7f
                })
                .WithBehaviour(() => new ObstacleAvoidanceBehaviour<ChickenState>(EnumExt.GetValues<ChickenState>(), new OutgoingState<ChickenState>[0])
                {
                    Weight = 50f,
                    AvoidingDistance = 1f
                })
                .WithBehaviour(() => new CohesionBehavior<ChickenState>(EnumExt.GetValues<ChickenState>(), new OutgoingState<ChickenState>[0]) { Weight = 1f })
                .WithBehaviour(() => new SeparationBehavior<ChickenState>(EnumExt.GetValues<ChickenState>(), new OutgoingState<ChickenState>[0])
                {
                    Name = "separation",
                    Weight = 1f
                })
                .WithBehaviour(() => new PlayerFleeingBehavior<ChickenState>(ChickenState.AvoidingPlayer, 0.5f, new OutgoingState<ChickenState>(1f, ChickenState.Wandering))
                {
                    Speed = 3f
                })
                .WithBehaviour(() => new VelocityMatchBehavior<ChickenState>(EnumExt.GetValues<ChickenState>(), new OutgoingState<ChickenState>[0]) { Weight = 2f })
                .WithBehaviour(() => new UnstuckBehavior<ChickenState>(EnumExt.GetValues<ChickenState>(), new OutgoingState<ChickenState>[0]) { Weight = 0.25f })
                .DefaultAnimation("walking").StateToAnimation(ChickenState.Picking, "eating").StateToAnimation(ChickenState.AvoidingPlayer, "flapping")
                .StateToAnimation(ChickenState.Fleeing, "flapping");
        }
    }




    public enum ChickenState
    {
        Picking,
        Wandering,
        Fleeing,
        Following,
        AvoidingPlayer
    }
}
