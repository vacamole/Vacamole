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

namespace Team6.Game.Scenes
{
    class LevelSelectionScene : Scene
    {
        private Type selectedLevel;
        private List<Tuple<Type, string>> levels;

        public LevelSelectionScene(MainGame game) : base(game)
        {
        }

        public override void Initialize()
        {
            this.levels = new List<Tuple<Type, string>>() {
                Tuple.Create(typeof(BasicGameScene), "One Big Herd"),
                Tuple.Create(typeof(RightToLeftGameScene), "Right to Left in Waves"),
                Tuple.Create(typeof(CenterSpawnGameScene), "Waves coming from the center"),
                Tuple.Create(typeof(EndlessGameScene), "Sandbox"),
                Tuple.Create((Type)null, "Return") };

        selectedLevel = levels.First().Item1;
        }

        public override void LoadContent()
        {
            base.LoadContent();
            var backgroundSize = new Vector2(6f / 3.508f * 1.1f * GameConstants.ScreenHeight, 1.1f * GameConstants.ScreenHeight);
            AddEntity(new Entity(this, EntityType.Game, new SpriteComponent("background", backgroundSize, new Vector2(0.5f, 0.5f), layerDepth: -1.0f)));
            AddEntity(new Entity(this, EntityType.Game, new SpriteComponent("trees_border", backgroundSize, new Vector2(0.5f, 0.5f), layerDepth: 0.9f)));

            // UI
            var buttonBackground = new HUDComponent("button_bg", new Vector2(1.2f, 0.2f), offset: new Vector2(0.5f, 0f), origin: new Vector2(0.5f, 0f));

            AddEntity(new Entity(this, EntityType.UI, buttonBackground,
                new HUDTextComponent(MainFont, 0.1f, "Select a game mode:",
                    offset: buttonBackground.LocalPointToWorldPoint(new Vector2(0.5f, 0.5f)),
                    origin: new Vector2(0.5f, 0.5f))
            ));

            HUDListEntity levelList = new HUDListEntity(this, new Vector2(0.5f, 0.5f), menuEntries: levels.Select(l => new HUDListEntity.ListEntry(l.Item2, item =>
            {
                selectedLevel = l.Item1;
                Dispatcher.NextFrame(StartLevel);

            })).ToArray());

            AddEntity(levelList);

            AddEntity(new Entity(this, EntityType.LayerIndependent, new CenterCameraComponent(Game.Camera)));
        }

        private void StartLevel()
        {
            if (selectedLevel != null)
            {
                var scene = (Scene)Activator.CreateInstance(selectedLevel, Game);
                this.TransitionOutAndSwitchScene(scene);
            }
            else
                this.TransitionOutAndSwitchScene(new JoinScene(this.Game));
        }

        public override void OnShown()
        {
            Game.EngineComponents.Get<AudioManager>().PlaySong("Audio\\joinScreen");
        }
    }
}
