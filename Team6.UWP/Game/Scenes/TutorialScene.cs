using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;
using System.Linq;
using Team6.Engine;
using Team6.Engine.Animations;
using Team6.Engine.Components;
using Team6.Engine.Entities;
using Team6.Engine.Graphics2d;
using Team6.Engine.Input;
using Team6.Engine.Misc;
using Team6.Engine.UI;
using Team6.Game.Entities;
using Team6.Game.Misc;
using Team6.Game.Mode;

namespace Team6.Game.Scenes
{
    public class TutorialScene : GameScene
    {

        private HUDTextComponent welcome;
        private HUDTextComponent playerAndBarn;
        private HUDTextComponent moveWithStick;
        private HUDTextComponent dash1;
        private TutorialState currentState;

        private PlayerEntity player;
        private Vector2 startPosition;
        private HUDTextComponent dash2;
        private HUDTextComponent dizzy;
        private HUDTextComponent lure;
        private bool hasCollidedWithAnimal;
        private bool hasLuured;
        private HUDTextComponent shout;
        private bool animalCollected;
        private HUDTextComponent fetchAnimal;
        private HUDTextComponent pressStartToLeave;
        private readonly bool navigateBackToMainMenu;

        public TutorialScene(MainGame game, bool navigateBackToMainMenu) : base(game, false, false)
        {
            this.navigateBackToMainMenu = navigateBackToMainMenu;
        }

        public override void Initialize()
        {
            base.Initialize();

            Game.CurrentGameMode.ClearPlayers();
            Game.CurrentGameMode.ClearScore();
            Game.CurrentGameMode.GameMode = Mode.GameMode.Waves;
            Game.CurrentGameMode.PlayerMode = Mode.PlayerMode.Free4All;

            int index = Game.EngineComponents.Get<InputManager>().GetFirstConnectedGamePad();
            Game.CurrentGameMode.PlayerInfos.Add(new Mode.PlayerInfo(1, 0, index == -1, index, Mode.PlayerColors.Green, "Alejandro"));

            playerPositions = new[] { new Vector2(-1, -1), new Vector2(1, 1), new Vector2(1, -1), new Vector2(-1, 1) };
            locs = new[] { new Vector2(centerX - 4, centerY - 3), new Vector2(-(centerX - 3), -(centerY - 3)), new Vector2(-(centerX - 3), centerY - 3), new Vector2(centerX - 4, -(centerY - 3)) };
            rotations = new[] { MathHelper.PiOver2, -MathHelper.PiOver2, MathHelper.Pi, 0f };

            GoToState(TutorialState.Start);

            Unpause();
        }

        private void GoToState(TutorialState start)
        {
            switch (currentState)
            {
                case TutorialState.Start:
                    Dispatcher.AddAnimation(Animation.Get(1, 0, 1, false, val => playerAndBarn.Opacity = val, EasingFunctions.ToEaseOut(EasingFunctions.QuadIn)));
                    break;
                case TutorialState.WaitTillMovingCanMoveOn:
                    Dispatcher.AddAnimation(Animation.Get(1, 0, 1, false, val => moveWithStick.Opacity = val, EasingFunctions.ToEaseOut(EasingFunctions.QuadIn)));
                    break;
                case TutorialState.Dash1:
                    Dispatcher.AddAnimation(Animation.Get(1, 0, 1, false, val => dash1.Opacity = val, EasingFunctions.ToEaseOut(EasingFunctions.QuadIn)));
                    break;
                case TutorialState.Dash2:
                    Dispatcher.AddAnimation(Animation.Get(1, 0, 1, false, val => dash2.Opacity = val, EasingFunctions.ToEaseOut(EasingFunctions.QuadIn)));
                    break;
                case TutorialState.Dizzy:
                    Dispatcher.AddAnimation(Animation.Get(1, 0, 1, false, val => dizzy.Opacity = val, EasingFunctions.ToEaseOut(EasingFunctions.QuadIn)));
                    break;
                case TutorialState.LureAnimals:
                    Dispatcher.AddAnimation(Animation.Get(1, 0, 1, false, val => lure.Opacity = val, EasingFunctions.ToEaseOut(EasingFunctions.QuadIn)));
                    player.Body.OnCollision -= Player_OnCollision;
                    break;
                case TutorialState.Shout:
                    Dispatcher.AddAnimation(Animation.Get(1, 0, 1, false, val => shout.Opacity = val, EasingFunctions.ToEaseOut(EasingFunctions.QuadIn)));
                    break;
                case TutorialState.FetchAnimal:
                    Dispatcher.AddAnimation(Animation.Get(1, 0, 1, false, val => fetchAnimal.Opacity = val, EasingFunctions.ToEaseOut(EasingFunctions.QuadIn)));
                    break;

            }
            currentState = start;

            switch (currentState)
            {
                case TutorialState.Start:
                    Dispatcher.AddAnimation(Animation.Get(0, 1, 1, false, val => welcome.Opacity = val, EasingFunctions.QuadIn));
                    Dispatcher.Delay(3, () =>
                     {
                         Dispatcher.AddAnimation(Animation.Get(1, 0, 1, false, val => welcome.Opacity = val, EasingFunctions.ToEaseOut(EasingFunctions.QuadIn)));
                         Dispatcher.AddAnimation(Animation.Get(0, 1, 1, false, val => playerAndBarn.Opacity = val, EasingFunctions.QuadIn));
                     })
                        .ThenDelay(3, () => GoToState(TutorialState.WaitTillMovingMessage));
                    break;
                case TutorialState.WaitTillMovingMessage:
                    Dispatcher.AddAnimation(Animation.Get(0, 1, 1, false, val => moveWithStick.Opacity = val, EasingFunctions.QuadIn));
                    Dispatcher.Delay(3, () => GoToState(TutorialState.WaitTillMovingCanMoveOn));
                    break;
                case TutorialState.Dash1:
                    Dispatcher.AddAnimation(Animation.Get(0, 1, 1, false, val => dash1.Opacity = val, EasingFunctions.QuadIn));
                    break;
                case TutorialState.Dash2:
                    Dispatcher.AddAnimation(Animation.Get(0, 1, 1, false, val => dash2.Opacity = val, EasingFunctions.QuadIn));
                    break;
                case TutorialState.Dizzy:
                    Dispatcher.AddAnimation(Animation.Get(0, 1, 1, false, val => dizzy.Opacity = val, EasingFunctions.QuadIn));
                    Dispatcher.Delay(4, () => GoToState(TutorialState.LureAnimals));
                    break;
                case TutorialState.LureAnimals:
                    player.Body.OnCollision += Player_OnCollision;
                    Dispatcher.AddAnimation(Animation.Get(0, 1, 1, false, val => lure.Opacity = val, EasingFunctions.QuadIn));
                    Vector2 pos = player.Body.Position;
                    SpawnCattleInZone(new Rectangle((int)(pos.X - 3), (int)(pos.Y - 3), 4, 4), 2, 5);
                    break;
                case TutorialState.Shout:
                    Dispatcher.AddAnimation(Animation.Get(0, 1, 1, false, val => shout.Opacity = val, EasingFunctions.QuadIn));
                    break;
                case TutorialState.FetchAnimal:
                    Dispatcher.AddAnimation(Animation.Get(0, 1, 1, false, val => fetchAnimal.Opacity = val, EasingFunctions.QuadIn));
                    break;
                case TutorialState.PressStartToLeaveTutorial:
                    Dispatcher.AddAnimation(Animation.Get(0, 1, 1, false, val => pressStartToLeave.Opacity = val, EasingFunctions.QuadIn));
                    break;
            }
        }

        public override void LoadContent()
        {
            base.LoadContent();

            // we create them to preload everything
            new BoarEntity(this, Vector2.Zero);
            new ChickenEntity(this, Vector2.Zero, 0); // normal chicken
            new ChickenEntity(this, Vector2.Zero, 1); // yellow chicken

            player = playerEntities[0];
            startPosition = player.Body.Position;

            // Trees
            AddEntity(new Entity(this, EntityType.Game, new Vector2(2.4f, 2f), 0.4f,
                new SpriteComponent("tree1", new Vector2(5.25f, 6.02f), new Vector2(0.5f, 0.5f), layerDepth: 0.9f),
                new PhysicsComponent(new PolygonShape(new Vertices(GameConstants.BoundingTree1), 1))
            ));
            AddEntity(new Entity(this, EntityType.Game, new Vector2(-2.8f, -3.5f), 1.4f,
                new SpriteComponent("tree1", new Vector2(5.25f * 1.1f, 6.02f * 1.1f), new Vector2(0.5f, 0.5f), layerDepth: 0.9f),
                new PhysicsComponent(new PolygonShape(new Vertices(GameConstants.BoundingTree1.Select(v => v * 1.1f)), 1))
            ));


            // UI
            AddEntity(new Entity(this, EntityType.UI, new Vector2(0.5f, 0.95f), welcome = new HUDTextComponent(MainFont, 0.08f, "Welcome to VACAMOLE.", origin: new Vector2(0.5f, 0.5f)) { Opacity = 0 },
                                                                                playerAndBarn = new HUDTextComponent(MainFont, 0.08f, "This is your player and your barn.", origin: new Vector2(0.5f, 0.5f)) { Opacity = 0 },
                                                                                moveWithStick = new HUDTextComponent(MainFont, 0.08f, "Try to move with the left stick of your controller.", origin: new Vector2(0.5f, 0.5f)) { Opacity = 0 },
                                                                                dash1 = new HUDTextComponent(MainFont, 0.08f, "Good. Now press the right bumper to dash.", origin: new Vector2(0.5f, 0.5f)) { Opacity = 0 },
                                                                                dash2 = new HUDTextComponent(MainFont, 0.08f, "Try to dash multiple times.", origin: new Vector2(0.5f, 0.5f)) { Opacity = 0 },
                                                                                lure = new HUDTextComponent(MainFont, 0.08f, "Press A to lure animals towards you!", origin: new Vector2(0.5f, 0.5f)) { Opacity = 0 },
                                                                                shout = new HUDTextComponent(MainFont, 0.08f, "Press B to shout. The animals will run!", origin: new Vector2(0.5f, 0.5f)) { Opacity = 0 }
                                                                                ));
            AddEntity(new Entity(this, EntityType.UI, new Vector2(0.5f, 0.9f), dizzy = new HUDTextComponent(MainFont, 0.08f, "If you dash too often or into a something, you get dizzy.\nHowever, if you hit another player you can make them dizzy.", origin: new Vector2(0.5f, 0.5f)) { Opacity = 0 },
                                                                               fetchAnimal = new HUDTextComponent(MainFont, 0.08f, "Now use luring (B) and shouting (A) to bring animals into your barn.\nBoars give 3 points, chicken 1 point.", origin: new Vector2(0.5f, 0.5f)) { Opacity = 0 },
                                                                               pressStartToLeave = new HUDTextComponent(MainFont, 0.08f, "Well done! Press START to continue!\nGrab your friends - VACAMOLE is enjoyed best with 4 players!", origin: new Vector2(0.5f, 0.5f)) { Opacity = 0 }));

            bool isKeyboard = Game.CurrentGameMode.PlayerInfos.First().IsKeyboardPlayer;
            int gamepadIndex = Game.CurrentGameMode.PlayerInfos.First().GamepadIndex;
            AddEntity(new Entity(this, EntityType.LayerIndependent, new InputComponent(gamepadIndex,
                new InputMapping(f => (isKeyboard) ? InputFunctions.KeyboardMenuStart(f) : InputFunctions.MenuStart(f), f =>
                {
                    if(currentState == TutorialState.PressStartToLeaveTutorial && !IsTransitioning)
                    {
                        Settings<GameSettings>.Value.ShowTutorial = false;
                        Settings<GameSettings>.Save();
                        Pause();
                        this.TransitionOutAndSwitchScene(navigateBackToMainMenu ? (Scene)new MainMenuScene(Game) : new GameModeScene(Game));
                    }

                }))));

        }

        public override void Update(float elapsedSeconds, float totalSeconds)
        {
            base.Update(elapsedSeconds, totalSeconds);

            switch (currentState)
            {
                case TutorialState.WaitTillMovingCanMoveOn:
                    if ((startPosition - player.Body.Position).LengthSquared() > 2)
                    {
                        GoToState(TutorialState.Dash1);
                    }
                    break;
                case TutorialState.Dash1:
                    if (player.State == PlayerState.Dashing)
                    {
                        GoToState(TutorialState.Dash2);
                    }
                    break;
                case TutorialState.Dash2:
                    if (player.State == PlayerState.Dizzy)
                    {
                        GoToState(TutorialState.Dizzy);
                    }
                    break;
                case TutorialState.LureAnimals:
                    hasLuured = hasLuured || player.State == PlayerState.Luring;
                    if (hasLuured && hasCollidedWithAnimal)
                    {
                        GoToState(TutorialState.Shout);
                    }
                    break;
                case TutorialState.Shout:
                    if (player.State == PlayerState.Shouting)
                    {
                        GoToState(TutorialState.FetchAnimal);
                    }
                    break;
                case TutorialState.FetchAnimal:
                    if (animalCollected)
                    {
                        GoToState(TutorialState.PressStartToLeaveTutorial);
                    }
                    break;
            }
        }

        private bool Player_OnCollision(FarseerPhysics.Dynamics.Fixture fixtureA, FarseerPhysics.Dynamics.Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            hasCollidedWithAnimal = hasCollidedWithAnimal || (fixtureB.Body.UserData as Entity).IsAnimal() || (fixtureA.Body.UserData as Entity).IsAnimal();
            return true;
        }

        protected override void OnAnimalEnteredBarn(PlayerInfo owner, Entity obj)
        {
            base.OnAnimalEnteredBarn(owner, obj);

            if (currentState == TutorialState.FetchAnimal)
            {
                animalCollected = true;
            }
        }

        private enum TutorialState
        {
            None,
            Start,
            WaitTillMovingMessage,
            WaitTillMovingCanMoveOn,
            Dash1,
            Dash2,
            Dizzy,
            LureAnimals,
            Shout,
            FetchAnimal,
            PressStartToLeaveTutorial
        }

    }
}
