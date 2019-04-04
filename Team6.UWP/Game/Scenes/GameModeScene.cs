using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Team6.Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Team6.Engine.Entities;
using Team6.Engine.Input;
using Team6.Engine.Components;
using Team6.Game.Components.AI;
using Team6.Engine.UI;
using Team6.Engine.Audio;
using Team6.Engine.Graphics2d;
using Team6.Game.Components;
using Team6.Game.Misc;
using Team6.Game.Entities;
using Team6.Engine.Animations;
using System.IO.IsolatedStorage;
using Team6.Engine.Misc;
using Team6.Game.Mode;

namespace Team6.Game.Scenes
{
    class GameModeScene : Scene
    {
        private HUDComponent playerMode2vs2;
        private HUDComponent playerMode2vs2Selected;
        private HUDComponent playerModeFree4All;
        private HUDComponent playerModeFree4AllSelected;

        private HUDTextComponent gameModeWaves;
        private HUDTextComponent gameModeBigHerd;

        private HUDTextComponent[] runtimeModeOptions;

        private HUDTextComponent continueText;
        private Animation continueTextAnimation;

        private PlayerMode playerMode = PlayerMode.TwoVsTwo;
        private GameMode gameMode = GameMode.Waves;

        private int selectedRuntimeMode = 0;

        private bool areControlsEnabled = false;

        private MenuState state = MenuState.Undefined;


        private readonly Color desaturatedColor = new Color(120, 120, 120, 255);
        private readonly float InActiveOpacity = 0.4f;

        public GameModeScene(MainGame game) : base(game)
        {
        }

        public override void Initialize()
        {

        }

        public override void LoadContent()
        {
            base.LoadContent();

            var backgroundSize = new Vector2(6f / 3.508f * 1.1f * GameConstants.ScreenHeight, 1.1f * GameConstants.ScreenHeight);
            AddEntity(new Entity(this, EntityType.Game, new SpriteComponent("background", backgroundSize, new Vector2(0.5f, 0.5f), layerDepth: -1.0f)));
            AddEntity(new Entity(this, EntityType.Game, new SpriteComponent("trees_border", backgroundSize, new Vector2(0.5f, 0.5f), layerDepth: 0.9f)));

            // UI
            var buttonBackground = new HUDComponent("overlayInstructionBox", new Vector2(1.2f, 0.2f), offset: new Vector2(0.5f, 0.05f), origin: new Vector2(0.5f, 0.05f));

            AddEntity(new Entity(this, EntityType.UI, buttonBackground,
                new HUDTextComponent(MainFont, 0.1f, "Game mode",
                    offset: buttonBackground.LocalPointToWorldPoint(new Vector2(0.5f, 0.5f)),
                    origin: new Vector2(0.5f, 0.5f), layerDepth: 0.1f)
            ));

            // Player mode selection
            playerMode2vs2 = new HUDComponent("gameMode_2VS2_notselected", new Vector2(0.5f, 0.32f), origin: new Vector2(0.5f, 0.5f), offset: new Vector2(-0.16f, 0))
            {
                Opacity = 0f
            };
            playerMode2vs2Selected = new HUDComponent("gameMode_2VS2_selected", new Vector2(0.5f, 0.32f), origin: new Vector2(0.5f, 0.5f), offset: new Vector2(-0.16f, 0));
            playerModeFree4All = new HUDComponent("gameMode_freeForAll_notselected", new Vector2(0.5f, 0.32f), origin: new Vector2(0.5f, 0.5f), offset: new Vector2(0.16f, 0));
            playerModeFree4AllSelected = new HUDComponent("gameMode_freeForAll_selected", new Vector2(0.5f, 0.32f), origin: new Vector2(0.5f, 0.5f), offset: new Vector2(0.16f, 0))
            {
                Opacity = 0f
            };

            AddEntity(new Entity(this, EntityType.UI, new Vector2(0.5f, 0.3f), playerMode2vs2, playerModeFree4All, playerMode2vs2Selected, playerModeFree4AllSelected));


            // Game mode selection
            gameModeWaves = new HUDTextComponent(MainFont, 0.05f, "WAVES", origin: new Vector2(0.5f, 0.5f), offset: new Vector2(-0.11f, 0), color: Color.Yellow)
            { Opacity = InActiveOpacity };
            gameModeBigHerd = new HUDTextComponent(MainFont, 0.05f, "BIG HERD", origin: new Vector2(0.5f, 0.5f), offset: new Vector2(0.11f, 0))
            { Opacity = InActiveOpacity };

            AddEntity(new Entity(this, EntityType.UI, new Vector2(0.5f, 0.55f), gameModeWaves, gameModeBigHerd));

            // Runtime mode selection
            runtimeModeOptions = Enumerable.Range(0, 4).Select(i => new HUDTextComponent(MainFont, 0.05f, "", origin: new Vector2(0.5f, 0.5f),
                offset: new Vector2(-0.225f, 0) + new Vector2(0.15f, 0) * i)
            { Opacity = InActiveOpacity, Color = i == 0 ? Color.Yellow : Color.Wheat }).ToArray();
            UpdateRuntimeTexts();
            AddEntity(new Entity(this, EntityType.UI, new Vector2(0.5f, 0.65f), runtimeModeOptions));

            // CONTINUE TEXT  
            continueText = new HUDTextComponent(MainFont, 0.05f, "press A to continue", origin: new Vector2(0.5f, 0.5f)) { Opacity = 0 };
            AddEntity(new Entity(this, EntityType.UI, new Vector2(0.5f, 0.8f), continueText));
            continueTextAnimation = Dispatcher.AddAnimation(Animation.Get(0, 1, 1.5f, true, val => continueText.Opacity = val, EasingFunctions.ToLoop(EasingFunctions.QuadIn))
                .Set(a => a.IsRunning = false));


            // add controls
            var layerIndependent = new Entity(this, EntityType.LayerIndependent, new CenterCameraComponent(Game.Camera));
            for (int i = 0; i < 4; i++)
                layerIndependent.AddComponent(new InputComponent(i,
                    new InputMapping(f => InputFunctions.MenuSelect(f) || InputFunctions.MenuDown(f), OnNextPressed),
                    new InputMapping(f => InputFunctions.MenuBack(f) || InputFunctions.MenuUp(f), OnBackPressed),
                    new InputMapping(f => InputFunctions.MenuStart(f), OnStartPressed),
                    new InputMapping(f => InputFunctions.MenuLeft(f), OnLeftPressed),
                    new InputMapping(f => InputFunctions.MenuRight(f), OnRightPressed)));


            layerIndependent.AddComponent(new InputComponent(
                new InputMapping(f => InputFunctions.KeyboardMenuSelect(f) || InputFunctions.KeyboardMenuDown(f), OnNextPressed),
                new InputMapping(f => InputFunctions.KeyboardMenuBack(f) || InputFunctions.KeyboardMenuUp(f), OnBackPressed),
                new InputMapping(f => InputFunctions.KeyboardMenuStart(f), OnStartPressed),
                new InputMapping(f => InputFunctions.KeyboardMenuLeft(f), OnLeftPressed),
                new InputMapping(f => InputFunctions.KeyboardMenuRight(f), OnRightPressed)));
            AddEntity(layerIndependent);

            GoToMenuState(MenuState.ChoosePlayerMode);


            this.TransitionIn();
        }

        private void UpdateRuntimeTexts()
        {
            int i = 0;
            if (gameMode == GameMode.BigHerd)
                // [FOREACH PERFORMANCE] Should not allocate garbage
                runtimeModeOptions.ForEach(h => h.Text = $"{20 + i++ * 10} animals");
            else
                // [FOREACH PERFORMANCE] Should not allocate garbage
                runtimeModeOptions.ForEach(h => h.Text = $"{3 + i++ * 2} waves");
        }



        private void SelectedRuntimeMode(int tryTo)
        {
            tryTo = MathHelper.Clamp(tryTo, 0, runtimeModeOptions.Length - 1);

            if (!areControlsEnabled || tryTo == selectedRuntimeMode)
                return;

            areControlsEnabled = false;

            var oldSelection = runtimeModeOptions[selectedRuntimeMode];
            selectedRuntimeMode = tryTo;
            var newSelection = runtimeModeOptions[tryTo];

            Dispatcher.AddAnimation(Animation.Get(0, 1, 0.3f, false, (val) =>
            {
                oldSelection.Color = Color.Lerp(Color.Yellow, Color.BlanchedAlmond, val);
                newSelection.Color = Color.Lerp(Color.BlanchedAlmond, Color.Yellow, val);
            }, EasingFunctions.QuadIn)).Then(() => areControlsEnabled = true);
        }

        private void SwitchPlayerMode()
        {
            if (!areControlsEnabled)
                return;

            areControlsEnabled = false;

            if (playerMode == PlayerMode.Free4All)
            {
                playerMode = PlayerMode.TwoVsTwo;
                Dispatcher.AddAnimation(Animation.Get(0, 1, 0.3f, false, (val) =>
                {
                    playerMode2vs2Selected.Opacity = val;
                    playerMode2vs2.Opacity = 1 - val;
                    playerModeFree4All.Opacity = val;
                    playerModeFree4AllSelected.Opacity = 1 - val;
                }, EasingFunctions.QuadIn)).Then(() => areControlsEnabled = true);
            }
            else
            {
                playerMode = PlayerMode.Free4All;
                Dispatcher.AddAnimation(Animation.Get(0, 1, 0.3f, false, (val) =>
                {
                    playerMode2vs2Selected.Opacity = 1 - val;
                    playerMode2vs2.Opacity = val;
                    playerModeFree4All.Opacity = 1 - val;
                    playerModeFree4AllSelected.Opacity = val;
                }, EasingFunctions.QuadIn)).Then(() => areControlsEnabled = true);
            }

            
        }

        private void OnStartPressed(InputFrame obj)
        {
            if (!areControlsEnabled)
                return;
            NonPositionalAudio.PlaySound("Audio/MenuActionSound");

        }

        private void OnLeftPressed(InputFrame obj)
        {
            if (!areControlsEnabled)
                return;
            NonPositionalAudio.PlaySound("Audio/MenuActionSound");

            switch (state)
            {
                case MenuState.ChoosePlayerMode:
                    if (playerMode == PlayerMode.Free4All)
                        SwitchPlayerMode();
                    break;
                case MenuState.ChooseGameMode:
                    if (gameMode == GameMode.BigHerd)
                        SwitchGameMode();
                    break;
                case MenuState.ChooseRuntime:
                    SelectedRuntimeMode(selectedRuntimeMode - 1);
                    break;
            }
        }

        private void OnRightPressed(InputFrame obj)
        {
            if (!areControlsEnabled)
                return;
            NonPositionalAudio.PlaySound("Audio/MenuActionSound");

            switch (state)
            {
                case MenuState.ChoosePlayerMode:
                    if (playerMode == PlayerMode.TwoVsTwo)
                        SwitchPlayerMode();
                    break;
                case MenuState.ChooseGameMode:
                    if (gameMode == GameMode.Waves)
                        SwitchGameMode();
                    break;
                case MenuState.ChooseRuntime:
                    SelectedRuntimeMode(selectedRuntimeMode + 1);
                    break;
            }
        }

        private void SwitchGameMode()
        {
            if (!areControlsEnabled)
                return;

            areControlsEnabled = false;

            Color startColorWaves;
            Color endColorWaves;

            if (gameMode == GameMode.BigHerd)
            {
                startColorWaves = Color.BlanchedAlmond;
                endColorWaves = Color.Yellow;
                gameMode = GameMode.Waves;
            }
            else
            {
                startColorWaves = Color.Yellow;
                endColorWaves = Color.BlanchedAlmond;
                gameMode = GameMode.BigHerd;
            }

            Dispatcher.AddAnimation(Animation.Get(0, 1, 0.3f, false, (val) =>
            {
                gameModeWaves.Color = Color.Lerp(startColorWaves, endColorWaves, val);
                gameModeBigHerd.Color = Color.Lerp(endColorWaves, startColorWaves, val);
            }, EasingFunctions.QuadIn)).Then(() => areControlsEnabled = true);

            UpdateRuntimeTexts();
        }

        private void OnBackPressed(InputFrame obj)
        {
            if (!areControlsEnabled)
                return;
            NonPositionalAudio.PlaySound("Audio/MenuActionSound");

            switch (state)
            {
                case MenuState.PressStart:
                    GoToMenuState(MenuState.ChooseRuntime);
                    break;
                case MenuState.ChooseRuntime:
                    GoToMenuState(MenuState.ChooseGameMode);
                    break;
                case MenuState.ChooseGameMode:
                    GoToMenuState(MenuState.ChoosePlayerMode);
                    break;
                case MenuState.ChoosePlayerMode:
                    this.TransitionOutAndSwitchScene(new MainMenuScene(Game));
                    break;
            }
        }

        private void OnNextPressed(InputFrame obj)
        {
            if (!areControlsEnabled)
                return;
            NonPositionalAudio.PlaySound("Audio/MenuActionSound");
            switch (state)
            {
                case MenuState.ChoosePlayerMode:
                    GoToMenuState(MenuState.ChooseGameMode);
                    break;
                case MenuState.ChooseGameMode:
                    GoToMenuState(MenuState.ChooseRuntime);
                    break;
                case MenuState.ChooseRuntime:
                    GoToMenuState(MenuState.PressStart);
                    break;
                case MenuState.PressStart:
                    Game.CurrentGameMode.GameMode = gameMode;
                    Game.CurrentGameMode.PlayerMode = playerMode;
                    Game.CurrentGameMode.RuntimeInfo = selectedRuntimeMode;
                    this.TransitionOutAndSwitchScene(new JoinScene(Game));
                    break;
            }
        }

        private void GoToMenuState(MenuState state)
        {
            areControlsEnabled = false;
            float duration = 0.4f;
            float delay = duration - 0.1f;
            // old state
            switch (this.state)
            {
                case MenuState.ChoosePlayerMode:
                    Dispatcher.AddAnimation(Animation.Get(1f, InActiveOpacity, duration, false, (val) =>
                    {
                        if (playerMode == PlayerMode.TwoVsTwo)
                        {
                            playerMode2vs2Selected.Opacity = val;
                            playerModeFree4All.Opacity = val;
                        }
                        else
                        {
                            playerMode2vs2.Opacity = val;
                            playerModeFree4AllSelected.Opacity = val;
                        }
                    }, EasingFunctions.QuadIn, delay));
                    break;
                case MenuState.ChooseGameMode:
                    Dispatcher.AddAnimation(Animation.Get(1f, InActiveOpacity, duration, false, (val) =>
                    {
                        gameModeWaves.Opacity = val;
                        gameModeBigHerd.Opacity = val;
                    }, EasingFunctions.QuadIn, delay));
                    break;
                case MenuState.ChooseRuntime:
                    Dispatcher.AddAnimation(Animation.Get(1f, InActiveOpacity, duration, false, (val) =>
                    {
                        // [FOREACH PERFORMANCE] Should not allocate garbage
                        runtimeModeOptions.ForEach(h => h.Opacity = val);
                    }, EasingFunctions.QuadIn, delay));
                    break;
                case MenuState.PressStart:
                    continueTextAnimation.IsRunning = false;
                    Dispatcher.AddAnimation(Animation.Get(continueText.Opacity, 0, duration, false, (val) => continueText.Opacity = val, EasingFunctions.QuadIn));
                    break;
                default:
                    delay = 0f;
                    break;
            }

            // set new state
            this.state = state;

            switch (this.state)
            {
                case MenuState.ChoosePlayerMode:
                    Dispatcher.AddAnimation(Animation.Get(InActiveOpacity, 1f, duration, false, (val) =>
                     {
                         if (playerMode == PlayerMode.TwoVsTwo)
                         {
                             playerMode2vs2Selected.Opacity = val;
                             playerModeFree4All.Opacity = val;
                         }
                         else
                         {
                             playerMode2vs2.Opacity = val;
                             playerModeFree4AllSelected.Opacity = val;
                         }
                     }, EasingFunctions.QuadIn, delay)).Then(() => areControlsEnabled = true);
                    break;
                case MenuState.ChooseGameMode:
                    Dispatcher.AddAnimation(Animation.Get(InActiveOpacity, 1f, duration, false, (val) =>
                    {
                        gameModeWaves.Opacity = val;
                        gameModeBigHerd.Opacity = val;
                    }, EasingFunctions.QuadIn, delay)).Then(() => areControlsEnabled = true);
                    break;
                case MenuState.ChooseRuntime:
                    Dispatcher.AddAnimation(Animation.Get(InActiveOpacity, 1f, duration, false, (val) =>
                    {
                        // [FOREACH PERFORMANCE] Should not allocate garbage
                        runtimeModeOptions.ForEach(h => h.Opacity = val);
                    }, EasingFunctions.QuadIn, delay)).Then(() => areControlsEnabled = true);
                    break;
                case MenuState.PressStart:
                    continueTextAnimation.IsRunning = true;
                    continueTextAnimation.Reset();
                    Dispatcher.Delay(delay, () => areControlsEnabled = true);
                    break;
                default:
                    throw new Exception("Wrong state");
            }
        }

        private void OnExit(HUDListEntity.ListEntry item)
        {
            Game.Exit();
        }

        private void OnNewGame(HUDListEntity.ListEntry item)
        {
            this.TransitionOutAndSwitchScene(new JoinScene(Game));
        }

        private enum MenuState
        {
            Undefined,
            ChoosePlayerMode,
            ChooseGameMode,
            ChooseRuntime,
            PressStart
        }

    }
}
