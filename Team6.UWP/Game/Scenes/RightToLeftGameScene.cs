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
using Team6.Game.Mode;

namespace Team6.Game.Scenes
{
    public class RightToLeftGameScene : GameScene
    {
        private Timer minimumTimeBetweenWaves = new Timer(10f);
        private Timer maxiumumTimeBetweenWaves = new Timer(120f);
        private int remainingWaves = 4;
        private bool nextWaveCanRespawn = true;
        private bool spawnNextWave = true;

        private int currentAnimalCount = 0;
        private int animalThresholdForNextWave = 0;
        private float barnSpacing;

        public RightToLeftGameScene(MainGame game) : base(game, false)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            gameDuration = 60f;

            int nrOfBarns = 0;
            if (Game.CurrentGameMode.PlayerMode == PlayerMode.TwoVsTwo)
                nrOfBarns = 2;
            else
                nrOfBarns = Game.CurrentGameMode.PlayerInfos.Count;

            barnSpacing = screenHeight / (nrOfBarns + 1);
            float positionX = -(centerX - 2);
            locs = new Vector2[nrOfBarns];
            for (int i = 0; i < nrOfBarns; i++)
            {
                locs[i] = new Vector2(positionX, (i + 1) * barnSpacing - centerY);
            }

            playerPositions = new[] { new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0) };
            rotations = new[] { MathHelper.Pi, MathHelper.Pi, MathHelper.Pi, MathHelper.Pi };

            remainingWaves = this.Game.CurrentGameMode.RuntimeInfo * 2 + 3;

            IsCountdownRunning = false;
        }

        public override void LoadContent()
        {
            base.LoadContent();

            // Trees
            AddEntity(new Entity(this, EntityType.Game, new Vector2(screenWidth / 4.0f, 3f), 0.4f,
                new SpriteComponent("tree1", new Vector2(5.25f * 0.9f, 6.02f * 0.9f), new Vector2(0.5f, 0.5f), layerDepth: 0.9f),
                new PhysicsComponent(new PolygonShape(new Vertices(GameConstants.BoundingTree1.Select(v => v * 0.9f)), 1))
            ));
            AddEntity(new Entity(this, EntityType.Game, new Vector2(screenWidth / 4.0f, -3f), 1f,
                new SpriteComponent("tree1", new Vector2(5.25f * 0.9f, 6.02f * 0.9f), new Vector2(0.5f, 0.5f), layerDepth: 0.9f),
                new PhysicsComponent(new PolygonShape(new Vertices(GameConstants.BoundingTree1.Select(v => v * 0.9f)), 1))
            ));

            AddEntity(new Entity(this, EntityType.Game, new Vector2(1f, 0f), 0f,
                new SpriteComponent("tree1", new Vector2(5.25f * 1.1f, 6.02f * 1.1f), new Vector2(0.5f, 0.5f), layerDepth: 0.9f),
                new PhysicsComponent(new PolygonShape(new Vertices(GameConstants.BoundingTree1.Select(v => v * 1.1f)), 1))
            ));

            
            for (int i = 0; i < locs.Length + 1; i++)
            {
                Vector2 position = locs[0] + new Vector2(2, -barnSpacing/2.0f + i * barnSpacing);
                const float scale = 0.4f;
                AddEntity(new Entity(this, EntityType.Game, position, 1.4f,
                //300 x 344
                new SpriteComponent("tree1", new Vector2(5.25f * scale, 6.02f * scale), new Vector2(0.5f, 0.5f), layerDepth: 0.9f),
                new PhysicsComponent(new PolygonShape(new Vertices(GameConstants.BoundingTree1.Select(v => v * scale)), 1))
            ));
            }
        }

        public override void Update(float elapsedSeconds, float totalSeconds)
        {
            base.Update(elapsedSeconds, totalSeconds);

            nextWaveCanRespawn = nextWaveCanRespawn || minimumTimeBetweenWaves.Tick(elapsedSeconds);
            spawnNextWave = nextWaveCanRespawn && (currentAnimalCount <= animalThresholdForNextWave || maxiumumTimeBetweenWaves.Tick(elapsedSeconds));

            // if the next
            if ((spawnNextWave && remainingWaves > 0) || (currentAnimalCount == 0 && nextWaveCanRespawn))
            {
                nextWaveCanRespawn = false;
                spawnNextWave = false;
                minimumTimeBetweenWaves.Reset();
                maxiumumTimeBetweenWaves.Reset();

                WaveType waveType = RandomExt.GetRandomFloat(0, 1) >= 0.5f ? WaveType.Collecting : WaveType.Fighting;
                currentAnimalCount += this.SpawnWave(waveType, new Rectangle((int)(0.625f*centerX), (int)(-0.625f*centerY), 2, (int)screenHeight / 2));

                animalThresholdForNextWave = (int)Math.Ceiling(currentAnimalCount * 0.1f);

                remainingWaves--;

                if (remainingWaves == 0)
                    base.StartCountown();
            }
        }

        protected override void OnAnimalEnteredBarn(PlayerInfo owner, Entity obj)
        {
            currentAnimalCount--;
            base.OnAnimalEnteredBarn(owner, obj);
        }
    }
}
