using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team6.Engine;
using Team6.Engine.Components;
using Team6.Engine.Entities;
using Team6.Engine.Graphics2d;
using Team6.Engine.UI;
using Team6.Game.Components;
using Team6.Game.Misc;

namespace Team6.Game.Scenes
{
    class LoadingScene : Scene
    {
        public LoadingScene(MainGame game) : base(game)
        {
        }

        public override void LoadContent()
        {
            base.LoadContent();
            // Background
            AddEntity(new Entity(this, EntityType.Game, new Vector2(0, 0),
                // src image 6000x3508
                new SpriteComponent("background", new Vector2(6f / 3.508f * 1.1f * GameConstants.ScreenHeight, 1.1f * GameConstants.ScreenHeight), new Vector2(0.5f, 0.5f), layerDepth: -1.0f)
            ));
            AddEntity(new Entity(this, EntityType.LayerIndependent, new CenterCameraComponent(Game.Camera)));

            ForestTransitionGenerator.GenerateTreesAndAnimate(this, null, true, animate: false);
        }

    }
}
