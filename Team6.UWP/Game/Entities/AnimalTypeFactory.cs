
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Team6.Engine.Components;
using Team6.Engine.Graphics2d;

namespace Team6.Game.Entities
{
    public static class AnimalTypeFactory
    {

        public static AnimalType<T> CreateAnimal<T>(T initialState)
        {
            return new AnimalType<T>() { InitalState = initialState };
        }

        public static AnimalType<T> WithAsset<T>(this AnimalType<T> animal, AnimationDefintion anim, Vector2 size, Vector2? origin = null)
        {
            animal.Asset = anim;
            animal.AssetOrigin = origin ?? animal.AssetOrigin;
            animal.Size = size;
            return animal;
        }

        public static AnimalType<T> WithSensing<T>(this AnimalType<T> animal, float hearingDistance = 5f, float viewDistance = 5f, float viewSize = 2.4f)
        {
            animal.HearingDistance = hearingDistance;
            animal.ViewDistance = viewDistance;
            animal.ViewSize = viewSize;
            return animal;
        }

        public static AnimalType<T> WithCollisionShape<T>(this AnimalType<T> animal, float radius, float densitiy = 1)
        {
            animal.CollisionShape = new CircleShape(radius, densitiy);
            return animal;
        }

        public static AnimalType<T> WithCollisionShape<T>(this AnimalType<T> animal, Vector2[] vertices, float densitiy = 1)
        {
            animal.CollisionShape = new PolygonShape(new Vertices(vertices), densitiy);
            return animal;
        }

        public static AnimalType<T> WithBehaviour<T>(this AnimalType<T> animal, Func<StateBasedAIBehaviorComponent<T>> behaviour)
        {
            animal.Behaviors.Add(behaviour);
            return animal;
        }

        public static AnimalType<T> StateToAnimation<T>(this AnimalType<T> animal, T state, string animationState)
        {
            animal.AnimationMappings.Add(state, animationState);
            return animal;
        }

        public static AnimalType<T> DefaultAnimation<T>(this AnimalType<T> animal, string animationState)
        {
            animal.DefaultAnimation = animationState;
            return animal;
        }

    }

    public class AnimalType<T>
    {
        public AnimationDefintion Asset { get; set; }
        public Vector2 AssetOrigin { get; set; } = new Vector2(0.5f, 0.5f);
        public Vector2 Size { get; set; }
        public float ViewDistance { get; set; } = 4f;
        public float ViewSize { get; set; } = 2.4f;
        public float HearingDistance { get; set; } = 5f;
        public Shape CollisionShape { get; set; }

        public List<Func<StateBasedAIBehaviorComponent<T>>> Behaviors { get; } = new List<Func<StateBasedAIBehaviorComponent<T>>>();
        
        public T InitalState { get; set; }

        public Dictionary<T, string> AnimationMappings { get; } = new Dictionary<T, string>();

        public string DefaultAnimation { get; set; }
    }

}

