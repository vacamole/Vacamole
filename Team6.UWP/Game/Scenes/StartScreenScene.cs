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
using Team6.Engine.Animations;
using Team6.Game.Misc;
using Team6.Engine.Graphics2d;
using Team6.Game.Components;

namespace Team6.Game.Scenes
{
    class StartScreenScene : Scene
    {
        public StartScreenScene(MainGame game) : base(game)
        {
        }

        public override void LoadContent()
        {
            base.LoadContent();

            //Game.CurrentGameMode.ClearPlayers();
            Game.EngineComponents.Get<AudioManager>().PreloadSongs("Audio\\joinScreen", "Audio\\gamePlay");
            var backgroundSize = new Vector2(6f / 3.508f * 1.1f * GameConstants.ScreenHeight, 1.1f * GameConstants.ScreenHeight);
            AddEntity(new Entity(this, EntityType.Game, new SpriteComponent("background", backgroundSize, new Vector2(0.5f, 0.5f), layerDepth: -1.0f)));
            AddEntity(new Entity(this, EntityType.Game, new SpriteComponent("trees_border", backgroundSize, new Vector2(0.5f, 0.5f), layerDepth: -0.9f)));

            // UI
            HUDTextComponent pressStartText = new HUDTextComponent(MainFont, 0.04f, "PRESS START", offset: new Vector2(0.5f, 0.9f), origin: new Vector2(0.5f, 0.5f), layerDepth: 1f);
            HUDComponent vignetteComponent = new HUDComponent(Game.Debug.DebugRectangle, Vector2.One, layerDepth: 0)
            {
                MaintainAspectRation = false, OnVirtualUIScreen = false,
                Color = Color.FromNonPremultiplied(22, 59, 59, 163)
            };
            Vector2 logoSize = new Vector2(1, 0.7684f);
            HUDComponent logoComponent = new HUDComponent("titleScreen", 0.9f * logoSize, offset: new Vector2(0.5f, 0.5f), origin: new Vector2(0.5f, 0.5f), layerDepth: 0.9f);

            AddEntity(new Entity(this, EntityType.UI, Vector2.Zero, pressStartText, vignetteComponent, logoComponent));
            Dispatcher.AddAnimation(Animation.Get(0, 1, 1.5f, true, val => pressStartText.Opacity = val, EasingFunctions.ToLoop(EasingFunctions.QuadIn)));


            var inputEntity = new Entity(this, EntityType.LayerIndependent,
                new InputComponent(0, new InputMapping(f => InputFunctions.KeyboardMenuStart(f), StartPressed))
            );

            for (int i = 0; i < 4; i++)
            {
                inputEntity.AddComponent(new InputComponent(i, new InputMapping(f => InputFunctions.MenuStart(f), StartPressed)));
            }
            AddEntity(inputEntity);

            AddEntity(new Entity(this, EntityType.LayerIndependent, new CenterCameraComponent(Game.Camera)));

            this.TransitionIn();
        }

        private void StartPressed(InputFrame obj)
        {
            this.TransitionOutAndSwitchScene(new MainMenuScene(Game));
        }

        public override void OnShown()
        {
            Game.EngineComponents.Get<AudioManager>().PlaySong("Audio\\joinScreen");
        }
    }
}
