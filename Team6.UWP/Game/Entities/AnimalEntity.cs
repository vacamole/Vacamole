using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team6.Engine;
using Team6.Engine.Audio;
using Team6.Engine.Misc;
using Team6.Engine.Components;
using Team6.Engine.Entities;
using Team6.Engine.Graphics2d;
using Team6.Game.Components;

namespace Team6.Game.Entities
{
    public abstract class AnimalEntity<T> : Entity
    {
        protected StateBasedAIComponent<T> aiComponent;
        protected AnimatedSpriteComponent animation;
        protected SensorComponent viewSensor;
        protected SensorComponent hearingSensor;

        public AnimalType<T> AnimalType { get; }

        public AnimalEntity(Scene scene, Vector2 location, AnimalType<T> animal, float rotation = 0) : base(scene, EntityType.Game, location, bodyType: BodyType.Dynamic)
        {
            AnimalType = animal;
            AddComponent(animation = new AnimatedSpriteComponent(animal.Asset, animal.Size, animal.AssetOrigin));
            AddComponent(new PhysicsComponent(animal.CollisionShape));
            AddComponent(viewSensor = new SensorComponent("view",
                new PolygonShape(PolygonTools.CreateCapsule(animal.ViewDistance, animal.ViewSize / 2, 4, animal.ViewSize, 8), 1),
                e => e is PlayerEntity && ((PlayerEntity)e).State == PlayerState.Luring));

            AddComponent(hearingSensor = new SensorComponent("hearing",
                new CircleShape(animal.HearingDistance, 1),
                e => e is PlayerEntity && ((PlayerEntity)e).State == PlayerState.Shouting));

            AddComponent(new MovementBasicsComponent());
            AddComponent(new RaycastSensorComponent(animal.ViewDistance));
            AddComponent(new AudioSourceComponent());

            aiComponent = new StateBasedAIComponent<T>(animal.InitalState);

            AddComponent(aiComponent);
            aiComponent.StateChanged += AiComponent_StateChanged1;

            // [FOREACH PERFORMANCE] Should not allocate garbage
            foreach (var b in animal.Behaviors)
                AddComponent(b());

            Body.Rotation = rotation;
        }

        private void AiComponent_StateChanged1(T arg1, T arg2)
        {
            string anim;
            if (!AnimalType.AnimationMappings.TryGetValue(arg2, out anim))
                anim = AnimalType.DefaultAnimation;

            animation.SwitchTo(anim);
        }
    }


}
