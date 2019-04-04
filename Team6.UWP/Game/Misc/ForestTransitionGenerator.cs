using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team6.Engine;
using Team6.Engine.Animations;
using Team6.Engine.Components;
using Team6.Engine.Entities;
using Team6.Engine.Graphics2d;
using Team6.Engine.MemoryManagement;
using Team6.Engine.Misc;
using Team6.Engine.UI;

namespace Team6.Game.Misc
{
    public static class ForestTransitionGenerator
    {
        public static readonly Vector2 WorldSize = new Vector2(16 * 2, 9 * 2);
        public static readonly Vector2 WorldUpperLeftCorner = new Vector2(-16, -9);
        public static readonly Vector2 OriginForAnimations = Vector2.Zero;

        public static readonly int ForestSeed = 1234;

        public static readonly object TreeIdentiferTag = new object();


        public static void GenerateTreesAndAnimate(Scene target, Action callback, bool fadeOut, bool animate = true, int treesX = 10, int treesY = 6)
        {
            Random r = new Random(ForestSeed);
            Vector2 treeSize = new Vector2(5.25f, 6.02f);
            Vector2 treeCount = new Vector2(treesX, treesY);
            Vector2 stepSize = WorldSize / treeCount;
            Vector2 initialOffset = stepSize / 2f + WorldUpperLeftCorner;

            float start = fadeOut ? 1 : 0;
            float end = 1 - start;

            Animation anim = null;
            // [FOREACH PERFORMANCE] ALLOCATES GARBAGE
            foreach (int x in Enumerable.Range(0, treesX))
            {
                // [FOREACH PERFORMANCE] ALLOCATES GARBAGE
                foreach (int y in Enumerable.Range(0, treesY))
                {
                    Vector2 position = new Vector2(x, y);
                    position = position * stepSize + initialOffset + r.GetRandomVector(0.1f, 0.3f);

                    SpriteComponent hudComponent = new SpriteComponent("tree1", new Vector2(3 * 1.75f, 3.44f * 1.75f), pivot: new Vector2(0.5f, 0.5f), layerDepth: 1f);
                    Entity tree = new Entity(target, EntityType.OverlayLayer, position, r.GetRandomAngle(), hudComponent);
                    tree.Tag = TreeIdentiferTag;

                    if (animate)
                        anim = target.Dispatcher.AddAnimation(Animation.Get(start, end, 0.5f, false, val => hudComponent.Opacity = val, EasingFunctions.QuadIn,
                                                (position - OriginForAnimations).Length() / 15f));

                    hudComponent.Opacity = start;

                    target.AddEntity(tree);
                }
            }
            if (animate)
            {
                anim.Then(() =>
                {
                    if (fadeOut)
                    {
                        // [FOREACH PERFORMANCE] ALLOCATES GARBAGE
                        target.GetEntities(EntityType.OverlayLayer).ToList().Where(e => e.Tag == TreeIdentiferTag).ForEach(e => e.Die());
                    }

                    callback?.Invoke();
                }); // at the moment this is always the last animation
            }
        }

        public static void TransitionIn(this Scene target, Action callback = null)
        {
            System.Diagnostics.Debug.WriteLine($"IsTransition == {target.IsTransitioning} (Transition In)");

            if (target.IsTransitioning)
                return;

            System.Diagnostics.Debug.WriteLine("IsTransition = true (Transition In)");
            target.IsTransitioning = true;
            GenerateTreesAndAnimate(target, () =>
            {
                System.Diagnostics.Debug.WriteLine("IsTransition = false (Transition In)");
                target.IsTransitioning = false;
                callback?.Invoke();
            }, true);
        }
        public static void TransitionOutAndSwitchScene(this Scene target, Scene newScene)
        {
            System.Diagnostics.Debug.WriteLine($"IsTransition == {target.IsTransitioning} (Transition Out)");
            if (target.IsTransitioning)
                return;

            System.Diagnostics.Debug.WriteLine("IsTransition = true (Transition Out)");
            target.IsTransitioning = true;
            GenerateTreesAndAnimate(target, () =>
            {
                System.Diagnostics.Debug.WriteLine("IsTransition = false (Transition Out)");
                target.IsTransitioning = false;
                target.Game.SwitchScene(newScene);
            }, false);
        }

    }
}
