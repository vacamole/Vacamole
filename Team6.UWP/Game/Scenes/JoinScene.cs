using System;
using Team6.Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Team6.Engine.Entities;
using Team6.Engine.Input;
using Team6.Engine.UI;
using Team6.Engine.Audio;
using Team6.Engine.Content;
using Team6.Engine.Graphics2d;
using Team6.Game.Entities;
using Team6.Engine.Components;
using System.Collections.Generic;
using System.Linq;
using Comora;
using Team6.Engine.Animations;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using Team6.Game.Misc;
using Team6.Engine.Misc;
using Team6.Game.Components;
using Team6.Game.Mode;
using System.Diagnostics;

namespace Team6.Game.Scenes
{
    class JoinScene : Scene
    {
        private List<JoinState> players;
        private Vector2[] positions;
        private Vector2[] offsets;
        private Vector2[] startPositions;
        private string[] boardAssets;
        private string[] boardWithButtonsAssets;

        AnimationDefintion joinAnimationDefinition;
        string[] availablePlayerNames;

        private int numJoinedPlayer = 0;
        private HUDAnimatedComponent joinComponent;
        private HUDTextComponent joinTextComponent;
        private HUDTextComponent pressStartToStart;
        private Animation pressStartToStartAnimation;

        public JoinScene(MainGame game) : base(game)
        {
            players = new List<JoinState>();
            positions = new[] { new Vector2(0.25f, 0.25f), new Vector2(0.75f, 0.25f), new Vector2(0.25f, 0.75f), new Vector2(0.75f, 0.75f) };
            offsets = new[] { new Vector2(-0.15f, -0.15f), new Vector2(0.15f, -0.15f), new Vector2(-0.15f, 0.15f), new Vector2(0.15f, 0.15f) };
            startPositions = new[] { new Vector2(-8.0f, -4.5f), new Vector2(8.0f, -4.5f), new Vector2(-8.0f, 4.5f), new Vector2(8.0f, 4.5f) };
            boardWithButtonsAssets = new[] { "board_blue_unselected", "board_red_unselected", "board_purple_unselected", "board_yellow_unselected" };
            boardAssets = new[] { "board_blue", "board_red", "board_purple", "board_yellow" };
        }


        public override void LoadContent()
        {
            base.LoadContent();

            Game.EngineComponents.Get<AudioManager>().PreloadSongs("Audio\\joinScreen", "Audio\\gamePlay");

            joinAnimationDefinition = Game.Content.LoadFromJson<AnimationDefintion>("Animations/join_anim");
            availablePlayerNames = Game.Content.LoadFromJson<PlayerNamesDefinition>("Text/player_names_text").Names;

            // [FOREACH PERFORMANCE] Should not allocate garbage
            foreach (var p in Game.CurrentGameMode.PlayerInfos)
            {
                JoinPlayer(p.IsKeyboardPlayer, p.GamepadIndex, p);
            }

            // Player mode
            AddEntity(new Entity(this, EntityType.UI, new Vector2(0.5f, 0.05f), 
                new HUDComponent(Game.CurrentGameMode.PlayerMode == PlayerMode.Free4All ? "gameMode_freeForAll_" : "gameMode_2VS2_", new Vector2(0.5f, 0.32f), origin: new Vector2(0.5f, 0.5f)),
                new HUDTextComponent(MainFont, 0.04f, Game.CurrentGameMode.PlayerMode == PlayerMode.TwoVsTwo ? "Team 1" : "", origin: new Vector2(0.5f, 0.5f), offset: new Vector2(-0.25f, 0f)),
                new HUDTextComponent(MainFont, 0.04f, Game.CurrentGameMode.PlayerMode == PlayerMode.TwoVsTwo ? "Team 2" : "", origin: new Vector2(0.5f, 0.5f), offset: new Vector2(0.25f, 0f))
            ));



            AddEntity(new Entity(this, EntityType.UI, new Vector2(0.5f, 0.95f), pressStartToStart = new HUDTextComponent(MainFont, 0.08f, "PRESS START", origin: new Vector2(0.5f, 0.5f)) { Opacity = 0 }
            ));

            pressStartToStartAnimation = Dispatcher.AddAnimation(Animation.Get(0, 1, 1.5f, true, val => pressStartToStart.Opacity = val, EasingFunctions.ToLoop(EasingFunctions.QuadIn)).Set(a => a.IsRunning = false));

            // Keyboard Entity
            AddEntity(new Entity(this, EntityType.LayerIndependent, MakeInput(true, 0)));
            // Gamepads
            AddEntity(new Entity(this, EntityType.LayerIndependent, MakeInput(false, 0)));
            AddEntity(new Entity(this, EntityType.LayerIndependent, MakeInput(false, 1)));
            AddEntity(new Entity(this, EntityType.LayerIndependent, MakeInput(false, 2)));
            AddEntity(new Entity(this, EntityType.LayerIndependent, MakeInput(false, 3)));


            AddEntity(new Entity(this, EntityType.UI,
                joinComponent = new HUDAnimatedComponent(joinAnimationDefinition, new Vector2(0.4f, 0.4f), origin: new Vector2(0.5f, 0.5f), offset: new Vector2(0.5f, 0.5f)),
                joinTextComponent = new HUDTextComponent(MainFont, 0.05f, "Press A to join", offset: new Vector2(0.5f, 0.575f), origin: new Vector2(0.5f, 0.5f), layerDepth: 0.1f)
            ));


            var backgroundSize = new Vector2(6f / 3.508f * 1.1f * GameConstants.ScreenHeight, 1.1f * GameConstants.ScreenHeight);
            AddEntity(new Entity(this, EntityType.Game, new SpriteComponent("background", backgroundSize, new Vector2(0.5f, 0.5f), layerDepth: -1.0f)));
            AddEntity(new Entity(this, EntityType.Game,
                new SpriteComponent("joinscreen_outerBorder", backgroundSize, new Vector2(0.5f, 0.5f), layerDepth: 0.9f),
                new PhysicsComponent(new PolygonShape(PolygonTools.CreateRectangle(GameConstants.ScreenWidth, 1, new Vector2(0, GameConstants.ScreenHeight / 2.0f), 0), 1)),
                new PhysicsComponent(new PolygonShape(PolygonTools.CreateRectangle(GameConstants.ScreenWidth, 1, new Vector2(0, -GameConstants.ScreenHeight / 2.0f), 0), 1)),
                new PhysicsComponent(new PolygonShape(PolygonTools.CreateRectangle(1, GameConstants.ScreenHeight, new Vector2(GameConstants.ScreenWidth / 2.0f, 0), 0), 1)),
                new PhysicsComponent(new PolygonShape(PolygonTools.CreateRectangle(1, GameConstants.ScreenHeight, new Vector2(-GameConstants.ScreenWidth / 2.0f, 0), 0), 1))
            ));
            AddEntity(new Entity(this, EntityType.Game,
                new SpriteComponent("joinscreen_trees_center", backgroundSize, new Vector2(0.5f, 0.5f), layerDepth: 0.9f),
                new PhysicsComponent(new CircleShape(GameConstants.ScreenHeight / 6.0f, 1f))
            ));
            AddEntity(new Entity(this, EntityType.Game,
                new SpriteComponent("joinscreen_trees_top", backgroundSize, new Vector2(0.5f, 0.5f), layerDepth: 0.9f),
                new PhysicsComponent(new PolygonShape(PolygonTools.CreateRectangle(1, GameConstants.ScreenHeight, new Vector2(0, -GameConstants.ScreenHeight / 4.0f), 0), 1))
            ));
            AddEntity(new Entity(this, EntityType.Game,
                new SpriteComponent("joinscreen_trees_bottom", backgroundSize, new Vector2(0.5f, 0.5f), layerDepth: 0.9f),
                new PhysicsComponent(new PolygonShape(PolygonTools.CreateRectangle(1, GameConstants.ScreenHeight, new Vector2(0, GameConstants.ScreenHeight / 4.0f), 0), 1))
            ));
            if (Game.CurrentGameMode.PlayerMode == PlayerMode.Free4All)
            {
                AddEntity(new Entity(this, EntityType.Game,
                    new SpriteComponent("joinscreen_trees_left", backgroundSize, new Vector2(0.5f, 0.5f), layerDepth: 0.9f),
                    new PhysicsComponent(new PolygonShape(PolygonTools.CreateRectangle(GameConstants.ScreenWidth, 1, new Vector2(-GameConstants.ScreenWidth / 4.0f, 0), 0), 1))
                ));
                AddEntity(new Entity(this, EntityType.Game,
                    new SpriteComponent("joinscreen_trees_right", backgroundSize, new Vector2(0.5f, 0.5f), layerDepth: 0.9f),
                    new PhysicsComponent(new PolygonShape(PolygonTools.CreateRectangle(GameConstants.ScreenWidth, 1, new Vector2(GameConstants.ScreenWidth / 4.0f, 0), 0), 1))
                ));
            }

            AddEntity(new Entity(this, EntityType.LayerIndependent, new CenterCameraComponent(Game.Camera)));


            this.TransitionIn();
        }

        private JoinState MakePlayer(bool isKeyboard, int gamepadIndex, PlayerInfo existingPlayer = null)
        {
            int myIndex;
            if (existingPlayer == null)
                myIndex = GetFirstFreeIndex();
            else
                myIndex = existingPlayer.PlayerIndex - 1;

            // Create UI for name
            Entity hud = new Entity(this, EntityType.UI, positions[myIndex],
                new HUDComponent(boardWithButtonsAssets[myIndex], new Vector2(1.528f / 0.551f * 0.2f, 0.2f), origin: new Vector2(0.5f, 0.5f))
                {
                    Name = "namechooser"
                },
                new HUDTextComponent(MainFont, 0.095f, "Name", origin: new Vector2(0.58f, 0.43f), layerDepth: 0.1f)
                {
                    Name = "nameTextchooser"
                },
                new HUDComponent(boardAssets[myIndex], new Vector2(1.528f / 0.551f * 0.1f, 0.1f), origin: new Vector2(0.5f, 0.5f), offset: offsets[myIndex])
                {
                    Name = "name",
                    Opacity = 0f
                },
                new HUDTextComponent(MainFont, 0.048f, "Name", origin: new Vector2(0.58f, 0.43f), layerDepth: 0.1f, offset: offsets[myIndex])
                {
                    Name = "nameText",
                    Opacity = 0f
                }
            );

            var newPlayerJoinState = new JoinState(null, hud, isKeyboard, gamepadIndex, myIndex);
            players.Add(newPlayerJoinState);
            if (existingPlayer != null)
                SetName(newPlayerJoinState, Array.IndexOf(availablePlayerNames, existingPlayer.Name));
            else
                SetRandomName(newPlayerJoinState);

            PlayerInfo pInfo;
            // Create Player if doesn't already exist
            if (existingPlayer == null)
            {
                pInfo = Game.CurrentGameMode.AddPlayer(newPlayerJoinState.IsKeyboard, gamepadIndex, availablePlayerNames[newPlayerJoinState.NameIndex]);
            }
            else
            {
                pInfo = existingPlayer;
            }
            PlayerEntity player = new PlayerEntity(this, startPositions[newPlayerJoinState.PlayerIndex], pInfo);
            newPlayerJoinState.Player = player;

            return newPlayerJoinState;
        }

        // Use this to get the first free index.
        // You can't just use players.Count as a new index, as this one can already exist.
        // This happens e.g. if two players join and then the first leaves.
        private int GetFirstFreeIndex()
        {
            int i = 0;
            // [FOREACH PERFORMANCE] ALLOCATES GARBAGE
            var indices = players.Select(p => p.PlayerIndex).OrderBy(ii => ii);
            foreach (var index in indices)
            {
                if (index == i)
                    i++;
            }
            return i;
        }

        private InputComponent MakeInput(bool isKeyboard, int gamepadIndex)
        {
            return new InputComponent(gamepadIndex,
                new InputMapping(f => (isKeyboard) ? InputFunctions.KeyboardMenuStart(f) : InputFunctions.MenuStart(f), f => StartIfReady()),
                new InputMapping(f => (isKeyboard) ? InputFunctions.KeyboardMenuSelect(f) : InputFunctions.MenuSelect(f), f =>
                 {
                     var player = players.FindPlayer(gamepadIndex, isKeyboard);

                     if (player != null)
                         PlayerIsReady(player);
                     else
                         JoinPlayer(isKeyboard, gamepadIndex);

                 }), new InputMapping(f => (isKeyboard) ? InputFunctions.KeyboardMenuBack(f) : InputFunctions.MenuBack(f), f =>
                         {
                             var player = players.FindPlayer(gamepadIndex, isKeyboard);

                             if (player != null)
                             {
                                 if (player.CurrentState == JoinState.State.Joined)
                                 {
                                     NonPositionalAudio.PlaySound("Audio/MenuActionSound");
                                     Game.CurrentGameMode.RemovePlayer(gamepadIndex, isKeyboard);
                                     RemoveEntity(player.HUD);
                                     RemoveEntity(player.Player);
                                     players.Remove(player);
                                     numJoinedPlayer--;

                                     if (numJoinedPlayer < 4)
                                     {
                                         Dispatcher.AddAnimation(Animation.Get(joinComponent.Opacity, 1, 0.2f, false, (val) =>
                                           {
                                               joinComponent.Opacity = val;
                                               joinTextComponent.Opacity = val;
                                           }, EasingFunctions.QuadIn));
                                     }
                                 }
                                 else if (player.CurrentState == JoinState.State.Ready)
                                 {
                                     NonPositionalAudio.PlaySound("Audio/MenuActionSound");
                                     player.Back();

                                     // Animate text
                                     var textChooser = player.HUD.GetComponentByName<HUDTextComponent>("nameTextchooser");
                                     Dispatcher.AddAnimation(Animation.Get(textChooser.Opacity, 1f, 1f, false, (o) => textChooser.Opacity = o, EasingFunctions.QuadIn));
                                     var boardChooser = player.HUD.GetComponentByName<HUDComponent>("namechooser");
                                     Dispatcher.AddAnimation(Animation.Get(boardChooser.Opacity, 1f, 1f, false, (o) => boardChooser.Opacity = o, EasingFunctions.QuadIn));

                                     var text = player.HUD.GetComponentByName<HUDTextComponent>("nameText");
                                     Dispatcher.AddAnimation(Animation.Get(text.Opacity, 0f, 1f, false, (o) => text.Opacity = o, EasingFunctions.ToEaseOut(EasingFunctions.QuadIn)));
                                     var board = player.HUD.GetComponentByName<HUDComponent>("name");
                                     Dispatcher.AddAnimation(Animation.Get(board.Opacity, 0f, 1f, false, (o) => board.Opacity = o, EasingFunctions.ToEaseOut(EasingFunctions.QuadIn)));

                                     player.Player.Freeze();

                                     if (players.Count(j => j.CurrentState == JoinState.State.Ready) >= (Game.CurrentGameMode.PlayerMode == PlayerMode.TwoVsTwo ? 4 : 1))
                                     {
                                         pressStartToStartAnimation.IsRunning = false;
                                         Dispatcher.AddAnimation(Animation.Get(pressStartToStart.Opacity, 0f, 0.3f, false, (o) => pressStartToStart.Opacity = o, EasingFunctions.ToEaseOut(EasingFunctions.QuadIn)));
                                     }
                                 }
                             }
                             else if (!players.Any())
                             {
                                 this.TransitionOutAndSwitchScene(new GameModeScene(Game));
                             }

                         }),
                new InputMapping(
                    f => (isKeyboard)
                        ? InputFunctions.KeyboardMenuRandom(f)
                        : InputFunctions.MenuRandom(f), f =>
                    {
                        var player = players.FindPlayer(gamepadIndex, isKeyboard);
                        if (player != null && player.CurrentState == JoinState.State.Joined)
                        {
                            NonPositionalAudio.PlaySound("Audio/MenuActionSound");
                            SetRandomName(player);
                        }
                    }),
                new InputMapping(
                    f => (isKeyboard)
                        ? InputFunctions.KeyboardMenuLeft(f) || InputFunctions.KeyboardMenuUp(f)
                        : InputFunctions.MenuLeft(f) || InputFunctions.MenuUp(f), f =>
                        {
                            var player = players.FindPlayer(gamepadIndex, isKeyboard);
                            if (player != null && player.CurrentState == JoinState.State.Joined)
                            {
                                NonPositionalAudio.PlaySound("Audio/MenuActionSound");
                                SetNextOrPreviousName(player, 1);
                            }
                        }),
                new InputMapping(
                    f => (isKeyboard)
                        ? InputFunctions.KeyboardMenuRight(f) || InputFunctions.KeyboardMenuDown(f)
                        : InputFunctions.MenuRight(f) || InputFunctions.MenuDown(f), f =>
                        {
                            var player = players.FindPlayer(gamepadIndex, isKeyboard);
                            if (player != null && player.CurrentState == JoinState.State.Joined)
                            {
                                NonPositionalAudio.PlaySound("Audio/MenuActionSound");
                                SetNextOrPreviousName(player, -1);
                            }
                        })
            );
        }

        private void SetNextOrPreviousName(JoinState player, int direction)
        {
            int nextName = player.NameIndex;

            while (players.Any(p => p.NameIndex == nextName))
                nextName = (nextName - direction + availablePlayerNames.Length) % availablePlayerNames.Length;

            SetName(player, nextName);
        }

        private void PlayerIsReady(JoinState player)
        {
            if (player.CurrentState == JoinState.State.Joined)
            {
                NonPositionalAudio.PlaySound("Audio/MenuActionSound");
                player.Next();

                // Animate text
                var textChooser = player.HUD.GetComponentByName<HUDTextComponent>("nameTextchooser");
                Dispatcher.AddAnimation(Animation.Get(textChooser.Opacity, 0f, 1f, false, (o) => textChooser.Opacity = o, EasingFunctions.ToEaseOut(EasingFunctions.QuadIn)));
                var boardChooser = player.HUD.GetComponentByName<HUDComponent>("namechooser");
                Dispatcher.AddAnimation(Animation.Get(boardChooser.Opacity, 0f, 1f, false, (o) => boardChooser.Opacity = o, EasingFunctions.ToEaseOut(EasingFunctions.QuadIn)));

                var text = player.HUD.GetComponentByName<HUDTextComponent>("nameText");
                Dispatcher.AddAnimation(Animation.Get(text.Opacity, 1f, 1f, false, (o) => text.Opacity = o, EasingFunctions.QuadIn));
                var board = player.HUD.GetComponentByName<HUDComponent>("name");
                Dispatcher.AddAnimation(Animation.Get(board.Opacity, 1f, 1f, false, (o) => board.Opacity = o, EasingFunctions.QuadIn));
                Dispatcher.NextFrame(() => player.Player.UnFreeze());

                if (players.Count(j => j.CurrentState == JoinState.State.Ready) >= (Game.CurrentGameMode.PlayerMode == PlayerMode.TwoVsTwo ? 4 : 1))
                    pressStartToStartAnimation.IsRunning = true;
            }
        }

        private void JoinPlayer(bool isKeyboard, int gamepadIndex, PlayerInfo existingPlayer = null)
        {
            JoinState joinState = MakePlayer(isKeyboard, gamepadIndex, existingPlayer);
            numJoinedPlayer++;

            // Animate text
            var text = joinState.HUD.GetComponentByName<HUDTextComponent>("nameTextchooser");
            Dispatcher.AddAnimation(Animation.Get(text.Opacity, 1f, 1f, false, (o) => text.Opacity = o, EasingFunctions.QuadIn));
            var board = joinState.HUD.GetComponentByName<HUDComponent>("namechooser");
            Dispatcher.AddAnimation(Animation.Get(board.Opacity, 1f, 1f, false, (o) => board.Opacity = o, EasingFunctions.QuadIn));

            Dispatcher.NextFrame(() =>
            {
                AddEntity(joinState.Player);
                joinState.Player.Freeze();
            });
            AddEntity(joinState.HUD);

            if (numJoinedPlayer == 4)
            {
                Dispatcher.AddAnimation(Animation.Get(1, 0, 0.2f, false, (val) =>
                {
                    joinComponent.Opacity = val;
                    joinTextComponent.Opacity = val;
                }, EasingFunctions.QuadIn));
            }
        }

        private void SetRandomName(JoinState joinState)
        {
            SetName(joinState, RandomExt.GetRandomInt(0, availablePlayerNames.Length, players.Select(p => p.NameIndex).ToArray()));
        }

        private void SetName(JoinState player, int newName)
        {
            player.NameIndex = newName;
            foreach (var textComponent in player.HUD.GetAllComponents<HUDTextComponent>())
                textComponent.Text = availablePlayerNames[player.NameIndex];
            Game.CurrentGameMode.SetNameForPlayer(availablePlayerNames[player.NameIndex], player.IsKeyboard, player.GamePadIndex);
        }

        private void StartIfReady()
        {
            if (!IsTransitioning)
            {
                if (players.Count > 0 && players.TrueForAll(t => t.CurrentState == JoinState.State.Ready))
                {
                    if (this.Game.CurrentGameMode.PlayerMode == PlayerMode.TwoVsTwo && !Debugger.IsAttached)
                    {
                        if (players.Count(t => t.CurrentState == JoinState.State.Ready) < 4)
                            return;
                    }

#if DEBUG
                    if (InputFunctions.DebugModifierIsOn(Keyboard.GetState()))
                    {
                        this.TransitionOutAndSwitchScene(new EndlessGameScene(this.Game));
                    }
                    else
#endif
                    if (this.Game.CurrentGameMode.GameMode == GameMode.BigHerd)
                    {

                        this.TransitionOutAndSwitchScene(new BasicGameScene(this.Game));
                    }
                    else if (this.Game.CurrentGameMode.GameMode == GameMode.Waves)
                    {
                        this.TransitionOutAndSwitchScene(new RightToLeftGameScene(this.Game));
                    }
                }
            }
        }

        public override void OnShown()
        {
            Game.EngineComponents.Get<AudioManager>().PlaySong("Audio\\joinScreen");
        }
    }

    class JoinState
    {
        public JoinState(PlayerEntity entity, Entity hud, bool isKeyboard, int gamepadIndex, int playerIndex)
        {
            this.Player = entity;
            this.HUD = hud;
            this.IsKeyboard = isKeyboard;
            this.GamePadIndex = gamepadIndex;
            this.PlayerIndex = playerIndex;
            CurrentState = State.Joined;
        }

        public int PlayerIndex { get; set; }

        public PlayerEntity Player { get; set; }
        public Entity HUD { get; set; }

        public int NameIndex { get; set; }

        public State CurrentState { get; private set; }

        public bool IsKeyboard { get; set; }

        public int GamePadIndex { get; set; }

        public enum State
        {
            Joined, Ready
        }

        public State Next()
        {
            if (CurrentState == State.Joined)
                CurrentState = State.Ready;

            return CurrentState;
        }

        public State Back()
        {
            if (CurrentState == State.Ready)
                CurrentState = State.Joined;

            return CurrentState;
        }
    }

    static class JoinStateExtension
    {
        public static JoinState FindPlayer(this List<JoinState> list, int gamePadIndex, bool isKeyboard)
        {
            return list.Find(p => p.IsKeyboard == isKeyboard && p.GamePadIndex == gamePadIndex);
        }
    }
}
