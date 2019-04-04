using System;
using System.Linq;
using Comora;
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
using Team6.Engine.Audio;
using Team6.Engine.Input;
using Team6.Engine.Graphics2d;
using Team6.Game.Components;
using Team6.Game.Misc;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Team6.Engine.Animations;
using Team6.Game.Mode;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Factories;
using FarseerPhysics.Dynamics;
using Team6.Engine.MemoryManagement;

namespace Team6.Game.Scenes
{
    public abstract class GameScene : Scene
    {
        protected Vector2[] locs;
        protected Vector2[] playerPositions;
        protected float[] rotations;
        protected const float screenWidth = GameConstants.ScreenWidth;
        protected const float screenHeight = GameConstants.ScreenHeight;
        protected const float centerX = screenWidth / 2.0f;
        protected const float centerY = screenHeight / 2.0f;
        protected readonly Color countdownColor = new Color(139, 112, 57);
        protected float gameDuration = 120f;

        private List<PlayerEntity> playerEntities = new List<PlayerEntity>();

        private HUDTextComponent timeComponent100;
        private HUDTextComponent timeComponent10;
        private HUDTextComponent timeComponent1;
        private HUDComponent timeComponentBackground;
        private HUDTextComponent initialCountdownComponent;

        private List<HUDComponent> pauseOverlayComponents = new List<HUDComponent>();
        private List<HUDTextComponent> pauseTextOverlayComponents = new List<HUDTextComponent>();

        private bool isInPauseAnimation = false;
        private HUDListEntity pauseMenuList;
        private HUDTextComponent playerName1Component;
        private HUDTextComponent playerName2Component;
        private HUDTextComponent playerName3Component;
        private HUDTextComponent playerName4Component;
        private HUDTextComponent separatorTeam1Component;
        private HUDTextComponent separatorTeam2Component;

        public GameScene(MainGame game, bool isCountdownRunning) : base(game)
        {
            if (isCountdownRunning)
                StartCountown();
        }

        public override void LoadContent()
        {
            base.LoadContent();

            Game.EngineComponents.Get<AudioManager>().PreloadSoundEffects("Audio/countDownLong", "Audio/countDownShort");

            Game.CurrentGameMode.ClearScore();

            AddPlayersWithBarns(locs, rotations);

            // Border trees
            Entity border;
            AddEntity(border = new Entity(this, EntityType.Game, new Vector2(0, 0),
                new SpriteComponent("joinscreen_outerBorder", new Vector2(screenWidth, screenWidth / 3.84f * 2.245f), new Vector2(0.5f, 0.5f), layerDepth: 0.9f)
            ));

            var triangulated = Triangulate.ConvexPartition(GameConstants.BoundingGameFieldTop, TriangulationAlgorithm.Earclip);
            triangulated.AddRange(Triangulate.ConvexPartition(GameConstants.BoundingGameFieldRight, TriangulationAlgorithm.Earclip));
            triangulated.AddRange(Triangulate.ConvexPartition(GameConstants.BoundingGameFieldBottom, TriangulationAlgorithm.Earclip));
            triangulated.AddRange(Triangulate.ConvexPartition(GameConstants.BoundingGameFieldLeft, TriangulationAlgorithm.Earclip));
            var fixtureList = FixtureFactory.AttachCompoundPolygon(triangulated, 1, border.Body);
            border.AddComponent(new PhysicsComponent(fixtureList[0].Shape));

            // Background
            AddEntity(new Entity(this, EntityType.Game, new Vector2(0, 0),
                // src image 6000x3508
                new SpriteComponent("background", new Vector2(screenWidth, screenWidth / 3.84f * 2.245f), new Vector2(0.5f, 0.5f), layerDepth: -1.0f)
            ));


            // HUD
            // src image 1365x460
            timeComponentBackground = new HUDComponent("timer_bg", new Vector2(1.365f / 0.46f * 0.1f, 1f * 0.1f), new Vector2(0.5f, 0f));

            this.timeComponent100 = new HUDTextComponent(MainFont, 0.07f, "", color: countdownColor,
                offset: timeComponentBackground.LocalPointToWorldPoint(new Vector2(0.35f, 0.5f)),
                origin: new Vector2(0.5f, 0.5f)
            );
            this.timeComponent10 = new HUDTextComponent(MainFont, 0.07f, "", color: countdownColor,
                offset: timeComponentBackground.LocalPointToWorldPoint(new Vector2(0.425f, 0.5f)),
                origin: new Vector2(0.5f, 0.5f)
            );
            this.timeComponent1 = new HUDTextComponent(MainFont, 0.07f, "", color: countdownColor,
                offset: timeComponentBackground.LocalPointToWorldPoint(new Vector2(0.5f, 0.5f)),
                origin: new Vector2(0.5f, 0.5f)
            );
            AddEntity(new Entity(this, EntityType.UI, new Vector2(0.5f, 0f),
                timeComponentBackground, timeComponent100, timeComponent10, timeComponent1
            ));

            if (!IsCountdownRunning)
            {
                timeComponentBackground.Opacity =
                    timeComponent100.Opacity =
                    timeComponent10.Opacity =
                    timeComponent1.Opacity = 0f;
            }

            UpdateCountdown(0);

            // Camera director
            AddEntity(new Entity(this, EntityType.LayerIndependent, new CameraDirectorComponent(playerEntities, Game.Camera)));

            // Add pause overlay
            AddEntity(new Entity(this, EntityType.UI, pauseOverlayComponents.AddAndReturn(new HUDComponent(Game.Debug.DebugRectangle, Vector2.One, layerDepth: 0.99f)
            {
                Color = Color.Black,
                MaintainAspectRation = false,
                OnVirtualUIScreen = false,
            })));

            AddEntity(new Entity(this, EntityType.UI, new Vector2(0.5f, 0.25f),
                pauseTextOverlayComponents.AddAndReturn(new HUDTextComponent(MainFont, 0.2f, "Game Paused", origin: new Vector2(0.5f, 0.5f), layerDepth: 1f))));

            pauseTextOverlayComponents.AddRange(
                AddEntity(pauseMenuList = new HUDListEntity(this, new Vector2(0.5f, 0.5f), layerDepth: 1f,
                menuEntries: new[] { new HUDListEntity.ListEntry("Resume", TogglePause), new HUDListEntity.ListEntry("Rejoin", BackToJoinScreen)
                ,new HUDListEntity.ListEntry("Back to main menu", BackToMainMenu)})
                {
                    Enabled = false
                })
                .GetAllComponents<HUDTextComponent>().ToList());

            // make overlay invisible
            // [FOREACH PERFORMANCE] Should not allocate garbage
            pauseOverlayComponents.ForEach(c => c.Opacity = 0f);
            // [FOREACH PERFORMANCE] Should not allocate garbage
            pauseTextOverlayComponents.ForEach(c => c.Opacity = 0f);

            // Add pause controls
            // [FOREACH PERFORMANCE] Should not allocate garbage
            foreach (var playerInfo in Game.CurrentGameMode.PlayerInfos)
            {
                if (playerInfo.IsKeyboardPlayer)
                    AddEntity(new Entity(this, EntityType.LayerIndependent, new InputComponent(new InputMapping(i => InputFunctions.KeyboardPause(i), (f) => TogglePause(null)))));
                else
                    AddEntity(new Entity(this, EntityType.LayerIndependent, new InputComponent(playerInfo.GamepadIndex,
                        new InputMapping(i => InputFunctions.Pause(i), (f) => TogglePause(null)),
                        new InputMapping(i => InputFunctions.StartCountdown(i), (f) => StartCountown())
                        )));
            }

            var playerInfos = Game.CurrentGameMode.PlayerInfos;
            var colors = ((PlayerColors[])Enum.GetValues(typeof(PlayerColors)));

            initialCountdownComponent = new HUDTextComponent(MainFont, 0.25f, Game.CurrentGameMode.PlayerMode == PlayerMode.TwoVsTwo ? "vs" : "free4all", origin: new Vector2(0.5f, 0.5f));
            playerName1Component = new HUDTextComponent(MainFont, 0.15f, playerInfos[0].Name, color: colors[0].GetColor(), origin: new Vector2(0f, 0.5f), offset: new Vector2(-0.25f, -0.25f));
            playerName2Component = new HUDTextComponent(MainFont, 0.15f, playerInfos.Count > 1 ? playerInfos[1].Name : "name2", color: colors[1].GetColor(), origin: new Vector2(0f, 0.5f), offset: new Vector2(-0.25f, 0.25f));
            playerName3Component = new HUDTextComponent(MainFont, 0.15f, playerInfos.Count > 2 ? playerInfos[2].Name : "name3", color: colors[2].GetColor(), origin: new Vector2(1f, 0.5f), offset: new Vector2(0.25f, -0.25f));
            playerName4Component = new HUDTextComponent(MainFont, 0.15f, playerInfos.Count > 3 ? playerInfos[3].Name : "name4", color: colors[3].GetColor(), origin: new Vector2(1f, 0.5f), offset: new Vector2(0.25f, 0.25f));
            separatorTeam1Component = new HUDTextComponent(MainFont, 0.15f, "&", origin: new Vector2(0.5f, 0.5f), offset: new Vector2(0f, -0.25f));
            separatorTeam2Component = new HUDTextComponent(MainFont, 0.15f, "&", origin: new Vector2(0.5f, 0.5f), offset: new Vector2(0f, 0.25f));
            Entity countdownEntity;
            AddEntity(countdownEntity = new Entity(this, EntityType.UI, new Vector2(0.5f, 0.5f), initialCountdownComponent));

            if (Game.CurrentGameMode.PlayerMode == PlayerMode.TwoVsTwo)
            {
                countdownEntity.AddComponent(playerName1Component);
                countdownEntity.AddComponent(playerName2Component);
                countdownEntity.AddComponent(playerName3Component);
                countdownEntity.AddComponent(playerName4Component);
                countdownEntity.AddComponent(separatorTeam1Component);
                countdownEntity.AddComponent(separatorTeam2Component);
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            Pause();
            this.TransitionIn();
            Func<int, Action> makeCountDown = (i) =>
            {
                return () =>
                {
                    NonPositionalAudio.PlaySound("Audio/countDownShort");
                    initialCountdownComponent.Text = i.ToString();
                };
            };

            Dispatcher.Delay(2f, () =>
            {
                Dispatcher.AddAnimation(Animation.Get(1f, 0f, 1f, false,
                        (v) => playerName1Component.Opacity = v,
                        EasingFunctions.ToEaseOut(EasingFunctions.QuadIn), 0.5f));
                Dispatcher.AddAnimation(Animation.Get(1f, 0f, 1f, false,
                        (v) => playerName2Component.Opacity = v,
                        EasingFunctions.ToEaseOut(EasingFunctions.QuadIn), 0.5f));
                Dispatcher.AddAnimation(Animation.Get(1f, 0f, 1f, false,
                        (v) => playerName3Component.Opacity = v,
                        EasingFunctions.ToEaseOut(EasingFunctions.QuadIn), 0.5f));
                Dispatcher.AddAnimation(Animation.Get(1f, 0f, 1f, false,
                        (v) => playerName4Component.Opacity = v,
                        EasingFunctions.ToEaseOut(EasingFunctions.QuadIn), 0.5f));
                Dispatcher.AddAnimation(Animation.Get(1f, 0f, 1f, false,
                        (v) => separatorTeam1Component.Opacity = v,
                        EasingFunctions.ToEaseOut(EasingFunctions.QuadIn), 0.5f));
                Dispatcher.AddAnimation(Animation.Get(1f, 0f, 1f, false,
                        (v) => separatorTeam2Component.Opacity = v,
                        EasingFunctions.ToEaseOut(EasingFunctions.QuadIn), 0.5f));

            }).ThenDelay(0.9f, makeCountDown(3))
            .ThenDelay(0.9f, makeCountDown(2))
            .ThenDelay(0.9f, makeCountDown(1))
            .ThenDelay(0.9f, () =>
            {
                NonPositionalAudio.PlaySound("Audio/countDownLong");
                initialCountdownComponent.Text = "GO!";
                Unpause();
                Dispatcher.AddAnimation(Animation.Get(1f, 0f, 1f, false,
                    (v) => initialCountdownComponent.Opacity = v,
                    EasingFunctions.ToEaseOut(EasingFunctions.QuadIn), 0.5f));
                if (IsCountdownRunning)
                    StartCountown();
            });
        }

        public void StartCountown()
        {
            if (IsCountdownRunning)
                return;
            IsCountdownRunning = true;

            Dispatcher.Delay(1f, () => UpdateCountdown(1f), (int)gameDuration)
            .ThenDelay(1f, () =>
            {
                Pause();
                this.TransitionOutAndSwitchScene(new WinScene(this.Game));
            });
            Dispatcher.AddAnimation(Animation.Get(0, 1, 0.3f, false,
                (val) => timeComponent100.Opacity = timeComponent10.Opacity = timeComponent1.Opacity = timeComponentBackground.Opacity = val,
             EasingFunctions.QuadIn));
        }


        public bool IsCountdownRunning { get; protected set; } = false;

        private void BackToJoinScreen(HUDListEntity.ListEntry item)
        {
            if (IsPaused)
            {
                this.TransitionOutAndSwitchScene(new JoinScene(Game));
            }
        }

        private void BackToMainMenu(HUDListEntity.ListEntry item)
        {
            if (IsPaused)
            {
                this.TransitionOutAndSwitchScene(new MainMenuScene(Game));
            }
        }

        private void TogglePause(HUDListEntity.ListEntry item)
        {
            if (isInPauseAnimation || IsTransitioning)
                return;
            isInPauseAnimation = true;

            if (IsPaused)
            {
                Dispatcher.AddAnimation(Animation.Get(1, 0, 0.2f, false, (value) =>
                {
                    // [FOREACH PERFORMANCE] Should not allocate garbage
                    pauseOverlayComponents.ForEach(c => c.Opacity = value * 0.7f);
                    // [FOREACH PERFORMANCE] Should not allocate garbage
                    pauseTextOverlayComponents.ForEach(c => c.Opacity = value);
                }, EasingFunctions.CubicIn)).Then(() =>
                {
                    isInPauseAnimation = false;
                    pauseMenuList.Enabled = false;
                    Game.EngineComponents.Get<AudioManager>().PlaySong("Audio\\gamePlay");
                    Unpause();
                });
            }
            else
            {
                Dispatcher.AddAnimation(Animation.Get(0, 1, 0.2f, false, (value) =>
                {
                    // [FOREACH PERFORMANCE] Should not allocate garbage (but capturing does)
                    pauseOverlayComponents.ForEach(c => c.Opacity = value * 0.7f);
                    // [FOREACH PERFORMANCE] Should not allocate garbage (but capturing does)
                    pauseTextOverlayComponents.ForEach(c => c.Opacity = value);
                }, EasingFunctions.CubicIn)).Then(() =>
                {
                    isInPauseAnimation = false;
                    pauseMenuList.Enabled = true;
                    Game.EngineComponents.Get<AudioManager>().PlaySong("Audio\\elevator_music");
                    Pause();
                });
            }
        }

        private void AddPlayersWithBarns(Vector2[] locs, float[] rotations)
        {
            // [FOREACH PERFORMANCE] Should not allocate garbage
            foreach (var p in Game.CurrentGameMode.PlayerInfos)
            {
                int index = p.PlayerIndex - 1;
                if (Game.CurrentGameMode.PlayerMode == PlayerMode.TwoVsTwo)
                    index = index % 2;

                var loc = locs[index];
                var rot = rotations[index];
                var secondPlayerOffset = Vector2.Zero;

                if (Game.CurrentGameMode.PlayerMode == PlayerMode.Free4All || Game.CurrentGameMode.PlayerMode == PlayerMode.TwoVsTwo && p.PlayerIndex <= 2)
                {
                    var newBarn = new BarnEntity(this, loc, rot, p);
                    AddEntity(newBarn).GetComponent<BarnComponent>().AnimalAdded += OnAnimalEnteredBarn;
                }

                if (Game.CurrentGameMode.PlayerMode == PlayerMode.TwoVsTwo && p.PlayerIndex > 2)
                {
                    secondPlayerOffset = playerPositions[p.PlayerIndex - 1];
                }

                var newPlayer = new PlayerEntity(this, loc + (BarnEntity.Size / 4) * playerPositions[index] + secondPlayerOffset, p);
                playerEntities.Add(newPlayer);
                AddEntity(newPlayer);
            }


        }

        protected virtual void OnAnimalEnteredBarn(PlayerInfo owner, Entity obj)
        {
            var playerEntity = playerEntities[owner.PlayerIndex - 1];
            playerEntity.ShowScore(obj is ChickenEntity ? 1 : 3);
        }

        public void SpawnCattleInZone(Rectangle zone, int boars, int chicken)
        {
            int amount = boars + chicken;
            for (int i = 0; i < amount; i++)
            {
                var x = RandomExt.GetRandomFloat(zone.Left, zone.Right);
                var y = RandomExt.GetRandomFloat(zone.Top, zone.Bottom);
                var r = RandomExt.GetRandomFloat(0, 2 * (float)Math.PI);

                if (boars > 0)
                {
                    AddEntity(new BoarEntity(this, new Vector2(x, y), r));
                    boars--;
                }
                else
                    AddEntity(new ChickenEntity(this, new Vector2(x, y), r));
            }
        }

        private void UpdateCountdown(float deltaTime)
        {
            gameDuration -= deltaTime;
            if (gameDuration <= 3f)
            {
                if (initialCountdownComponent.Opacity == 0)
                {
                    // fade out
                    Dispatcher.AddAnimation(Animation.Get(1f, 0f, 1f, false,
                        (v) => timeComponent100.Opacity = v,
                        EasingFunctions.ToEaseOut(EasingFunctions.QuadIn), 0.5f));
                    Dispatcher.AddAnimation(Animation.Get(1f, 0f, 1f, false,
                        (v) => timeComponent10.Opacity = v,
                        EasingFunctions.ToEaseOut(EasingFunctions.QuadIn), 0.5f));
                    Dispatcher.AddAnimation(Animation.Get(1f, 0f, 1f, false,
                        (v) => timeComponent1.Opacity = v,
                        EasingFunctions.ToEaseOut(EasingFunctions.QuadIn), 0.5f));
                    Dispatcher.AddAnimation(Animation.Get(1f, 0f, 1f, false,
                        (v) => timeComponentBackground.Opacity = v,
                        EasingFunctions.ToEaseOut(EasingFunctions.QuadIn), 0.5f));

                    //fade in
                    Dispatcher.AddAnimation(Animation.Get(0f, 1f, 1f, false,
                        (v) => initialCountdownComponent.Opacity = v,
                        EasingFunctions.QuadIn));
                    initialCountdownComponent.Color = Color.OrangeRed;
                }

                if (gameDuration > 0)
                {
                    initialCountdownComponent.Text = gameDuration.ToString("N0");
                    NonPositionalAudio.PlaySound("Audio/countDownShort");
                }
                else
                {
                    initialCountdownComponent.Text = "Game over!";
                    NonPositionalAudio.PlaySound("Audio/PigHappy");
                }
            }
            else
                UpdateCountdownText();

        }

        private void UpdateCountdownText()
        {
            int hundreds = (int)(gameDuration / 100f);
            int tens = (int)((gameDuration % 100) / 10f);
            int ones = (int)gameDuration % 10;

            timeComponent100.Text = (gameDuration >= 100) ? hundreds.ToString() : "";
            timeComponent10.Text = (gameDuration >= 10) ? tens.ToString() : "";
            timeComponent1.Text = ones.ToString();
        }

        public override void OnShown()
        {
            Game.EngineComponents.Get<AudioManager>().PlaySong("Audio\\gamePlay");
        }
    }
}
