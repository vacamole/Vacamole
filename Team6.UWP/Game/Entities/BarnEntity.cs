using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Team6.Engine;
using Team6.Engine.Audio;
using Team6.Engine.Components;
using Team6.Engine.Entities;
using Team6.Engine.Graphics2d;
using Team6.Engine.UI;
using Team6.Game.Components;
using Team6.Game.Mode;

namespace Team6.Game.Entities
{
    internal class BarnEntity : Entity
    {
        public BarnEntity(Scene scene, Vector2 loc, float rotation, PlayerInfo player) : base(scene, EntityType.Game, loc, rotation)
        {
            string resource;

            switch (player.Color)
            {
                case PlayerColors.Green:
                    resource = "barn_green";
                    break;
                case PlayerColors.Pink:
                    resource = "barn_pink";
                    break;
                case PlayerColors.Purple:
                    resource = "barn_purple";
                    break;
                case PlayerColors.Yellow:
                    resource = "barn_yellow";
                    break;
                default:
                    resource = "barn_yellow";
                    break;
            }

            AddComponent(new SpriteComponent(resource, Size, new Vector2(0.5f, 0.5f), layerDepth: 0.5f));
            AddComponent(new TextDisplayComponent(Scene.MainFont, 1f, "0", player.Color.GetColor()));
            AddComponent(new SensorComponent("enclosure", new PolygonShape(PolygonTools.CreateRectangle(Size.X / 2.5f, Size.Y / 2.5f), 0), e => e.IsAnimal()));
            AddComponent(new BarnComponent(player));
            AddComponent(new AudioSourceComponent());
        }

        public static Vector2 Size = new Vector2(8f / 6.11f * 3f, 1.0f * 3f);
    }
}