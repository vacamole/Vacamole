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

namespace Team6.Game.Scenes
{
    class MainMenuScene : Scene
    {
        private HUDListEntity optionsList;
        private HUDListEntity mainMenuList;
        private HUDComponent controlInstructions;
        private HUDComponent controlInstructionsBackground;
        private HUDTextComponent[] creditsScene;

        private bool isControlInstructionsEnabled = false;
        private bool isCreditsEnabled = false;

        private MenuState state = MenuState.Undefined;

        public MainMenuScene(MainGame game) : base(game)
        {
        }

        public override void Initialize()
        {

        }

        public override void LoadContent()
        {
            base.LoadContent();

            Game.CurrentGameMode.ClearPlayers();

            var backgroundSize = new Vector2(6f / 3.508f * 1.1f * GameConstants.ScreenHeight, 1.1f * GameConstants.ScreenHeight);
            AddEntity(new Entity(this, EntityType.Game, new SpriteComponent("background", backgroundSize, new Vector2(0.5f, 0.5f), layerDepth: -1.0f)));
            AddEntity(new Entity(this, EntityType.Game, new SpriteComponent("trees_border", backgroundSize, new Vector2(0.5f, 0.5f), layerDepth: 0.9f)));

            //// UI
            //var buttonBackground = new HUDComponent("button_bg", new Vector2(1.2f, 0.2f), offset: new Vector2(0.5f, 0f), origin: new Vector2(0.5f, 0f));

            //AddEntity(new Entity(this, EntityType.UI, buttonBackground,
            //    new HUDTextComponent(MainFont, 0.1f, "Vacamole",
            //        offset: buttonBackground.LocalPointToWorldPoint(new Vector2(0.5f, 0.5f)),
            //        origin: new Vector2(0.5f, 0.5f))
            //));

            HUDComponent vignetteComponent = new HUDComponent(Game.Debug.DebugRectangle, Vector2.One, layerDepth: -0.1f)
            {
                MaintainAspectRation = false,
                OnVirtualUIScreen = false,
                Color = Color.FromNonPremultiplied(22, 59, 59, 163)
            };
            Vector2 logoSize = new Vector2(1, 0.7684f);
            HUDComponent logoComponent = new HUDComponent("titleScreen", 0.8f * logoSize, offset: new Vector2(0.5f, 0.1f), origin: new Vector2(0.5f, 0f), layerDepth: -0.1f);

            AddEntity(new Entity(this, EntityType.UI, Vector2.Zero, vignetteComponent, logoComponent));

            mainMenuList = new HUDListEntity(this, new Vector2(0.5f, 0.8f), elementSize: 0.1f, allowAllControllers: true, isHorizontal: true, menuEntries: new[] {
                new HUDListEntity.ListEntry("New Game", OnNewGame),
                new HUDListEntity.ListEntry("Options", (i) => GoToMenuState(MenuState.Options)),
                new HUDListEntity.ListEntry("Controls", (i) => GoToMenuState(MenuState.Controls)),
                new HUDListEntity.ListEntry("Tutorial", OnGoToTutorial),
                new HUDListEntity.ListEntry("Credits", (i) => GoToMenuState(MenuState.Credits)),
                new HUDListEntity.ListEntry("Exit", OnExit)
            })
            { Enabled = false, Opacity = 0 };

            optionsList = new HUDListEntity(this, new Vector2(0.5f, 0.8f), elementSize: 0.15f, allowAllControllers: true, isHorizontal: true, menuEntries: new[] {
                new HUDListEntity.ListEntry($"Master: {GetPercentage(Settings<GameSettings>.Value.MasterVolume)}%", OnMasterVolume),
                new HUDListEntity.ListEntry($"Sound: {GetPercentage(Settings<GameSettings>.Value.SoundVolume)}%", OnSoundVolume),
                new HUDListEntity.ListEntry($"Music: {GetPercentage(Settings<GameSettings>.Value.MusicVolume)}%", OnMusicVolume),
                new HUDListEntity.ListEntry("Back", SaveAndBack)
            })
            { Enabled = false, Opacity = 0 };

            controlInstructionsBackground = new HUDComponent(Game.Debug.DebugRectangle, Vector2.One, origin: new Vector2(0.5f, 0.5f), layerDepth: -0.1f)
            {
                MaintainAspectRation = false,
                OnVirtualUIScreen = false,
                Color = Color.FromNonPremultiplied(0, 0, 0, 160),
                Opacity = 0
            };
            controlInstructions = new HUDComponent("controls", new Vector2(0.85f, 0.4f), origin: new Vector2(0.5f, 0.5f)) { Opacity = 0, MaintainAspectRation = true };
            AddEntity(new Entity(this, EntityType.UI, new Vector2(0.5f, 0.5f), controlInstructionsBackground, controlInstructions));
            AddEntity(mainMenuList);
            AddEntity(optionsList);

            GoToMenuState(MenuState.MainMenu);

            var layerIndependent = new Entity(this, EntityType.LayerIndependent, new CenterCameraComponent(Game.Camera));


            // Add controls to hide the controls overlay or the credits overlay (later version)
            for (int i = 0; i < 4; i++)
                layerIndependent.AddComponent(new InputComponent(i, new InputMapping(f => InputFunctions.MenuSelect(f) || InputFunctions.MenuBack(f), HideControls)));
            layerIndependent.AddComponent(new InputComponent(new InputMapping(f => InputFunctions.KeyboardMenuSelect(f) || InputFunctions.KeyboardMenuBack(f), HideControls)));


            AddEntity(layerIndependent);

            creditsScene = new[] {
                new HUDTextComponent(MainFont, 0.03f, "Credits",
                offset: new Vector2(0.5f, 0.1f),
                origin: new Vector2(0.5f, 0.5f))
                { Opacity = 0 },
                new HUDTextComponent(MainFont, 0.04f, "Programming",
                offset: new Vector2(0.5f, 0.25f),
                origin: new Vector2(0.5f, 0.5f))
                { Opacity = 0 },
                new HUDTextComponent(MainFont, 0.06f, "Alexander Kayed\nMoritz Zilian\nFlorian Zinggeler",
                offset: new Vector2(0.5f, 0.4f),
                origin: new Vector2(0.5f, 0.5f))
                { Opacity = 0 },
                new HUDTextComponent(MainFont, 0.04f, "Art",
                offset: new Vector2(0.5f, 0.6f),
                origin: new Vector2(0.5f, 0.5f))
                { Opacity = 0 },
                new HUDTextComponent(MainFont, 0.06f, "Sonja Böckler",
                offset: new Vector2(0.5f, 0.675f),
                origin: new Vector2(0.5f, 0.5f))
                { Opacity = 0 },
                new HUDTextComponent(MainFont, 0.035f, "This game was created during the\nETH Game Programming Laboratory course\nin collaboration with ZHdK",
                offset: new Vector2(0.5f, 0.85f),
                origin: new Vector2(0.5f, 0.5f))
                { Opacity = 0 },
            };
            AddEntity(new Entity(this, EntityType.UI, creditsScene));



            this.TransitionIn();
        }

        private void HideControls(InputFrame input)
        {
            if (isControlInstructionsEnabled && state == MenuState.Controls)
                GoToMenuState(MenuState.MainMenu);
            if (isCreditsEnabled && state == MenuState.Credits)
                GoToMenuState(MenuState.MainMenu);
        }

        private void OnMasterVolume(HUDListEntity.ListEntry obj)
        {
            Settings<GameSettings>.Value.MasterVolume += 0.1f;

            if (Settings<GameSettings>.Value.MasterVolume > 1.05)
                Settings<GameSettings>.Value.MasterVolume = 0f;

            Settings<GameSettings>.Value.MasterVolume = MathHelper.Clamp(Settings<GameSettings>.Value.MasterVolume, 0, 1);
            Game.EngineComponents.Get<AudioManager>().LoadVolumeSettings();

            obj.Caption = $"Master: {GetPercentage(Settings<GameSettings>.Value.MasterVolume)}%";
        }

        private void OnMusicVolume(HUDListEntity.ListEntry obj)
        {
            Settings<GameSettings>.Value.MusicVolume += 0.1f;

            if (Settings<GameSettings>.Value.MusicVolume > 1.05)
                Settings<GameSettings>.Value.MusicVolume = 0f;

            Settings<GameSettings>.Value.MusicVolume = MathHelper.Clamp(Settings<GameSettings>.Value.MusicVolume, 0, 1);

            Game.EngineComponents.Get<AudioManager>().LoadVolumeSettings();

            obj.Caption = $"Music: {GetPercentage(Settings<GameSettings>.Value.MusicVolume)}%";
        }

        private void OnSoundVolume(HUDListEntity.ListEntry obj)
        {
            Settings<GameSettings>.Value.SoundVolume += 0.1f;

            if (Settings<GameSettings>.Value.SoundVolume > 1.05)
                Settings<GameSettings>.Value.SoundVolume = 0f;

            Settings<GameSettings>.Value.SoundVolume = MathHelper.Clamp(Settings<GameSettings>.Value.SoundVolume, 0, 1);

            Game.EngineComponents.Get<AudioManager>().LoadVolumeSettings();

            obj.Caption = $"Sound: {GetPercentage(Settings<GameSettings>.Value.SoundVolume)}%";
        }

        private float GetPercentage(float value)
        {
            return (float)Math.Round(MathHelper.Clamp(value, 0, 1) * 100, 0);
        }

        private void SaveAndBack(HUDListEntity.ListEntry item)
        {
            Settings<GameSettings>.Save();
            GoToMenuState(MenuState.MainMenu);
        }

        private void GoToMenuState(MenuState state)
        {
            float duration = 0.4f;
            float delay = duration - 0.1f;
            // old state
            switch (this.state)
            {
                case MenuState.MainMenu:
                    mainMenuList.Enabled = false;
                    Dispatcher.AddAnimation(Animation.Get(1, 0, duration, false, val => mainMenuList.Opacity = val, EasingFunctions.QuadIn));
                    break;
                case MenuState.Options:
                    optionsList.Enabled = false;
                    Dispatcher.AddAnimation(Animation.Get(1, 0, duration, false, val => optionsList.Opacity = val, EasingFunctions.QuadIn));
                    break;
                case MenuState.Controls:
                    isControlInstructionsEnabled = false;
                    Dispatcher.AddAnimation(Animation.Get(1, 0, duration, false, val => controlInstructions.Opacity = controlInstructionsBackground.Opacity = val, EasingFunctions.QuadIn));
                    break;
                case MenuState.Credits:
                    isCreditsEnabled = false;
                    Dispatcher.AddAnimation(Animation.Get(1, 0, duration, false, val =>
                        {
                            controlInstructionsBackground.Opacity = val;
                            creditsScene.ForEach(c => c.Opacity = val);
                        }, EasingFunctions.QuadIn));
                    break;
                default:
                    delay = 0f;
                    break;
            }

            // set new state
            this.state = state;

            switch (this.state)
            {
                case MenuState.MainMenu:
                    Dispatcher.AddAnimation(Animation.Get(0, 1, duration, false, val => mainMenuList.Opacity = val, EasingFunctions.QuadIn, delay))
                        .Then(() => mainMenuList.Enabled = true);
                    break;
                case MenuState.Options:
                    Dispatcher.AddAnimation(Animation.Get(0, 1, duration, false, val => optionsList.Opacity = val, EasingFunctions.QuadIn, delay))
                       .Then(() => optionsList.Enabled = true);
                    break;
                case MenuState.Controls:
                    Dispatcher.AddAnimation(Animation.Get(0, 1, duration, false, val => controlInstructions.Opacity = controlInstructionsBackground.Opacity = val, EasingFunctions.QuadIn))
                        .Then(() => isControlInstructionsEnabled = true);
                    break;
                case MenuState.Credits:
                    Dispatcher.AddAnimation(Animation.Get(0, 1, duration, false, val =>
                        {
                            controlInstructionsBackground.Opacity = val;
                            creditsScene.ForEach(c => c.Opacity = val);
                        }, EasingFunctions.QuadIn))
                        .Then(() => isCreditsEnabled = true);
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
            if (Settings<GameSettings>.Value.ShowTutorial)
            {
                this.TransitionOutAndSwitchScene(new TutorialScene(Game, false));
            }
            else
            {
                this.TransitionOutAndSwitchScene(new GameModeScene(Game));
            }
        }

        private void OnGoToTutorial(HUDListEntity.ListEntry obj)
        {
            this.TransitionOutAndSwitchScene(new TutorialScene(Game, true));
        }

        public override void OnShown()
        {
            Game.EngineComponents.Get<AudioManager>().PlaySong("Audio\\joinScreen");
            base.OnShown();
        }

        private enum MenuState
        {
            Undefined,
            MainMenu,
            Options,
            Credits,
            Controls
        }
    }
}
