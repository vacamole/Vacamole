using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Team6.Game.Misc
{
    class GameConstants
    {
        private const float scale = 1.8f;
        public const float ScreenWidth = 16 * scale;
        public const float ScreenHeight = 9 * scale;

        public const Category CollisionCategoryBoundingBox = Category.Cat1;
        public const Category CollisionCategorySensor = Category.Cat2;
        public const Category CollisionCategoryBarn = Category.Cat3;

        public static Vertices BoundingGameFieldTop { get; } = new Vertices(new[]
        {
                new Vector2(-16, -7), new Vector2(-14, -7), new Vector2(-13, -8), new Vector2(-10, -8f), new Vector2(-9, -8.8f), new Vector2(-7, -8.9f), new Vector2(-5, -9), new Vector2(-2.5f,-7.5f), new Vector2(-1.5f,-8f), new Vector2(-0.3f,-8f),
                new Vector2(0.3f, -7.3f), new Vector2(1.6f, -7.6f), new Vector2(3, -7.6f), new Vector2(5, -8.5f), new Vector2(6, -8.5f), new Vector2(8, -9), new Vector2(9, -8), new Vector2(12, -8), new Vector2(13, -7), new Vector2(16, -7),
                new Vector2(16, -9), new Vector2(-16, -9)
        }.Select((v) => v / 2 * scale));

        public static Vertices BoundingGameFieldRight { get; } = new Vertices(new[]
        {
                new Vector2(13, -9), new Vector2(13, 9), new Vector2(16, 9), new Vector2(16, -9)
        }.Select((v) => v / 2 * scale));

        public static Vertices BoundingGameFieldBottom { get; } = new Vertices(new[]
        {
                new Vector2(16, 7), new Vector2(13f, 7), new Vector2(12, 8), new Vector2(11, 8), new Vector2(10, 8.2f), new Vector2(8, 8), new Vector2(5.4f, 7.4f), new Vector2(4.8f, 8), new Vector2(4, 8), new Vector2(3, 7.2f), new Vector2(2, 7.2f), new Vector2(1.5f, 6.7f), new Vector2(0, 8),
                new Vector2(1, 8), new Vector2(-2, 7.2f), new Vector2(-3, 7.2f), new Vector2(-3.7f, 8), new Vector2(-4.5f, 7.5f), new Vector2(-5.5f, 8), new Vector2(-6.3f, 7.5f), new Vector2(-8.8f, 9), new Vector2(-9.5f, 8.7f), new Vector2(-11, 9), new Vector2(-12.8f, 8.2f), new Vector2(-14, 7), new Vector2(-16, 7),
                new Vector2(-16, 9), new Vector2(16, 9)
        }.Select((v) => v / 2 * scale));

        public static Vertices BoundingGameFieldLeft { get; } = new Vertices(new[]
        {
                new Vector2(-14, -9), new Vector2(-14, 9), new Vector2(-16, 9), new Vector2(-16, -9)
        }.Select((v) => v / 2 * scale));

        public static Vertices BoundingTree1 { get; } = new Vertices(new[]
        {
                new Vector2(-2.1f, 0), new Vector2(-0.5f, -1.5f), new Vector2(0.4f, -2.3f), new Vector2(1.3f, -2.2f), new Vector2(2.1f, -1.2f),
                new Vector2(2, 1), new Vector2(1, 2.3f), new Vector2(-0.4f, 2.2f), new Vector2(-0.8f, 1.2f), new Vector2(-2f, 0.7f)
        });

    }
}
