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
using Team6.Engine.Input;
using Microsoft.Xna.Framework.Input;
using Team6.Game.Misc;

namespace Team6.Game.Scenes
{
    public class EndlessGameScene : GameScene
    {
        public EndlessGameScene(MainGame game) : base(game, false)
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

            SpawnCattleInZone(new Rectangle(-10, -6, 20, 12), 1, 1);

            AddEntity(new Entity(this, EntityType.LayerIndependent,
                new InputComponent(0, new InputMapping(f => InputFunctions.SpawnBoar(f), f =>
                {
                    SpawnCattleInZone(new Rectangle(-10, -6, 20, 12), 1, 0);
                }), new InputMapping(f => InputFunctions.SpawnChicken(f), f =>
                {
                    SpawnCattleInZone(new Rectangle(-10, -6, 20, 12), 0, 1);
                }), new InputMapping(f => InputFunctions.EndRound(f), f =>
                {
                    this.Game.SwitchScene(new WinScene(this.Game));
                })
            )));

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
                new SpriteComponent("tree1", new Vector2(3 * 1.75f * 1.1f, 3.44f * 1.75f * 1.1f), new Vector2(0.5f, 0.5f), layerDepth: 0.9f),
                new PhysicsComponent(new PolygonShape(new Vertices(new[]
                {
                    new Vector2(-2, 0), new Vector2(1, -2), new Vector2(2, -1),
                    new Vector2(2, 1), new Vector2(0, 2)
                }.Select(v => v * 1.1f)), 1))
            ));
        }

        public override void Update(float elapsedSeconds, float totalSeconds)
        {
            base.Update(elapsedSeconds, totalSeconds);
        }

    }
}
