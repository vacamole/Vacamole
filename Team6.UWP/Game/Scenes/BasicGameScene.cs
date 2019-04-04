using System;
using System.Linq;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Team6.Engine;
using Team6.Engine.Entities;
using Team6.Engine.Components;
using Team6.Engine.Misc;
using Team6.Game.Entities;
using Team6.Engine.UI;
using Team6.Engine.Graphics2d;
using Team6.Game.Misc;

namespace Team6.Game.Scenes
{
    public class BasicGameScene : GameScene
    {
        public BasicGameScene(MainGame game) : base(game, true)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            playerPositions = new[] { new Vector2(-1, -1), new Vector2(1, 1), new Vector2(1, -1), new Vector2(-1, 1) };
            locs = new[] { new Vector2(centerX - 4, centerY - 3), new Vector2(-(centerX - 3), -(centerY - 3)), new Vector2(-(centerX - 3), centerY - 3), new Vector2(centerX - 4, -(centerY - 3)) };
            rotations = new[] { MathHelper.PiOver2, -MathHelper.PiOver2, MathHelper.Pi, 0f };
        }

        public override void LoadContent()
        {
            base.LoadContent();

            // AI
            int spawnWidth = (int)(screenWidth * 2f / 3f);
            int spawnHeight = (int)(screenHeight * 2f / 3f);

            int totalNumberOfAnimals = Game.CurrentGameMode.RuntimeInfo * 10 + 20;

            SpawnCattleInZone(new Rectangle(-spawnWidth / 2, -spawnHeight / 2, spawnWidth, spawnHeight), (int)(0.2* totalNumberOfAnimals), (int)(0.8 * totalNumberOfAnimals));

            // Trees
            AddEntity(new Entity(this, EntityType.Game, new Vector2(2.4f, 2f), 0.4f,
                new SpriteComponent("tree1", new Vector2(5.25f, 6.02f), new Vector2(0.5f, 0.5f), layerDepth: 0.9f),
                new PhysicsComponent(new PolygonShape(new Vertices(GameConstants.BoundingTree1), 1))
            ));
            AddEntity(new Entity(this, EntityType.Game, new Vector2(-2.8f, -3.5f), 1.4f,
                new SpriteComponent("tree1", new Vector2(5.25f * 1.1f, 6.02f * 1.1f), new Vector2(0.5f, 0.5f), layerDepth: 0.9f),
                new PhysicsComponent(new PolygonShape(new Vertices(GameConstants.BoundingTree1.Select(v => v * 1.1f)), 1))
            ));
        }

    }
}
