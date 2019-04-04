using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarseerPhysics.Dynamics;
using Team6.Engine;
using Team6.Engine.Components;
using Team6.Engine.Entities;
using Team6.Game.Components;
using Team6.Game.Mode;
using Team6.Engine.Content;
using Team6.Engine.Graphics2d;
using Team6.Engine.Audio;
using Team6.Engine.Animations;

namespace Team6.Game.Entities
{
    public class PlayerEntity : Entity
    {
        private AnimatedSpriteComponent playerAnim;
        private AnimatedSpriteComponent attractAnim;
        private AnimatedSpriteComponent scareAnim;
        private ScoreComponent scoreComponent;

        public PlayerEntity(Scene scene, Vector2 position, PlayerInfo player) : base(scene, EntityType.Game, position, bodyType: FarseerPhysics.Dynamics.BodyType.Dynamic)
        {
            var playerAnimationDefinition = Scene.Game.Content.LoadFromJson<AnimationDefintion>("Animations/player_anim");
            var scareAnimDefinition = Scene.Game.Content.LoadFromJson<AnimationDefintion>("Animations/scare_anim");
            var attractAnimationDefinition = Scene.Game.Content.LoadFromJson<AnimationDefintion>("Animations/attract_anim");

            this.PlayerInfo = player; 
            switch (player.Color)
            {
                case PlayerColors.Green:
                    playerAnimationDefinition.AssetName = "player_green_anim";
                    break;
                case PlayerColors.Pink:
                    playerAnimationDefinition.AssetName = "player_pink_anim";
                    break;
                case PlayerColors.Purple:
                    playerAnimationDefinition.AssetName = "player_purple_anim";
                    break;
                case PlayerColors.Yellow:
                    playerAnimationDefinition.AssetName = "player_yellow_anim";
                    break;
                default:
                    playerAnimationDefinition.AssetName = "player_yellow_anim";
                    break;
            }



            AddComponent(playerAnim = new AnimatedSpriteComponent(playerAnimationDefinition, new Vector2(1.0f, 1.0f), new Vector2(0.5f, 0.5f), name: "playerAnim"));
            AddComponent(attractAnim = new AnimatedSpriteComponent(attractAnimationDefinition, new Vector2(3.0f, 3.0f), new Vector2(0.5f, 0.5f), name: "attractAnim"));
            AddComponent(scareAnim = new AnimatedSpriteComponent(scareAnimDefinition, new Vector2(5.0f, 5.0f), new Vector2(0.5f, 0.5f), name: "scareAnim"));

            AddComponent(scoreComponent = new ScoreComponent(player.Color.GetColor(), name: "score"));

            AddComponent(new PhysicsComponent(new CircleShape(0.25f, 1)));
            AddComponent(new PlayerControllerComponent(player));
            AddComponent(new AudioSourceComponent(new []{"Audio/Hit1", "Audio/Hit2", "Audio/Hit3"}, new string[] {}));
        }

        public override void Initialize()
        {
            base.Initialize();

            Body.Mass = 75f;
        }

        public PlayerInfo PlayerInfo { get; private set; }

        public void SwitchToState(PlayerState newState)
        {
            State = newState;

            switch (State)
            {
                case PlayerState.Luring:
                    attractAnim.SwitchTo("playing");
                    scareAnim.SwitchTo("stopped");
                    break;
                case PlayerState.Shouting:
                    attractAnim.SwitchTo("stopped");
                    scareAnim.SwitchTo("playing");
                    break;
                default:
                    attractAnim.SwitchTo("stopped");
                    scareAnim.SwitchTo("stopped");
                    break;
            }
        }

        public PlayerState State { get; set; }

        public void UnFreeze()
        {
            var controller = new PlayerControllerComponent(PlayerInfo);
            AddComponent(controller);
            controller.Initialize();
        }

        public void Freeze()
        {
            RemoveAllComponents<PlayerControllerComponent>();
        }

        internal void ShowScore(int score)
        {
            scoreComponent.ShowScore(score);
        }
    }

    public enum PlayerState
    {
        /// <summary>
        /// Normal state
        /// </summary>
        Walking,
        /// <summary>
        /// Push animals and players away with a quick dash
        /// </summary>
        Dashing,
        /// <summary>
        /// While dizzy, players move like drunk and can't dash
        /// </summary>
        Dizzy,
        /// <summary>
        /// Making animals follow
        /// </summary>
        Luring,
        /// <summary>
        /// Making animals go away
        /// </summary>
        Shouting
    }
}
