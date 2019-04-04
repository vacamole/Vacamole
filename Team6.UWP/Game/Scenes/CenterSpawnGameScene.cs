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

namespace Team6.Game.Scenes
{
    public class CenterSpawnGameScene : GameScene
    {
        private const float RESPAWN_INTERVAL = 15f;
        private float nextRespawn = 0f;

        public CenterSpawnGameScene(MainGame game) : base(game, true)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            playerPositions = new[] { new Vector2(-1, -1), new Vector2(1, 1), new Vector2(1, -1), new Vector2(-1, 1) };
            locs = new[] { new Vector2(centerX - 2, centerY - 2), new Vector2(-(centerX - 2), -(centerY - 2)), new Vector2(-(centerX - 2), centerY - 2), new Vector2(centerX - 2, -(centerY - 2)) };
            rotations = new[] { MathHelper.PiOver2, -MathHelper.PiOver2, MathHelper.Pi, 0f };
        }

        public override void LoadContent()
        {
            base.LoadContent();

            // Trees
            AddEntity(new Entity(this, EntityType.Game, new Vector2(-11f, 0f), 0.4f,
                //300 x 344
                new SpriteComponent("tree1", new Vector2(3 * 1.75f, 3.44f * 1.75f), new Vector2(0.5f, 0.5f), layerDepth: 0.9f),
                new PhysicsComponent(new PolygonShape(new Vertices(new[]
                {
                    new Vector2(-2, 0), new Vector2(1, -2), new Vector2(2, -1),
                    new Vector2(2, 1), new Vector2(0, 2)
                }), 1))
            ));
            AddEntity(new Entity(this, EntityType.Game, new Vector2(11f, 0f), 1.4f,
                //300 x 344
                new SpriteComponent("tree1", new Vector2(3 * 1.75f*1.1f, 3.44f * 1.75f*1.1f), new Vector2(0.5f, 0.5f), layerDepth: 0.9f),
                new PhysicsComponent(new PolygonShape(new Vertices(new[]
                {
                    new Vector2(-2, 0), new Vector2(1, -2), new Vector2(2, -1),
                    new Vector2(2, 1), new Vector2(0, 2)
                }.Select(v => v * 1.1f)), 1))
            ));

            AddEntity(new Entity(this, EntityType.Game, new Vector2(0f, 0f), 1.4f,
                //300 x 344
                new SpriteComponent("tree1", new Vector2(3 * 1.75f * 1.1f, 3.44f * 1.75f * 1.1f), new Vector2(0.5f, 0.5f), layerDepth: 0.9f),
                new PhysicsComponent(new PolygonShape(new Vertices(new[]
                {
                    new Vector2(-1, 0), new Vector2(0.5f, -1), new Vector2(1, -0.5f),
                    new Vector2(1, 0.5f), new Vector2(0, 1)
                }.Select(v => v * 1.1f)), 1))
            ));
        }

        public override void Update(float elapsedSeconds, float totalSeconds)
        {
            base.Update(elapsedSeconds, totalSeconds);

            /* Respawn cattle */
            nextRespawn -= elapsedSeconds;
            if (nextRespawn <= 0)
            {
                SpawnCattleInZone(new Rectangle(-2, -2, 4, 4), 1, 2);
                nextRespawn = RESPAWN_INTERVAL;
            }
        }

    }
}
