using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Comora;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework.Graphics;
using Team6.Engine.Audio;
using Team6.Engine.Components;
using Team6.Engine.Content;
using Team6.Engine.Entities;
using Team6.Engine.Input;
using Team6.Engine.Misc;
using Team6.Game.Entities;
using Team6.Game.Mode;
using Team6.Engine.Graphics2d;
using Team6.Game.Misc;

namespace Team6.Game.Components
{
    public class PlayerControllerComponent : InputComponent, IUpdateableComponent, ILoadContent, IDebugDrawable
    {
        private Vector2 inputMovementDirection = Vector2.Zero;
        private float desiredRotation = 0;
        private PlayerEntity playerEntity;
        private AnimatedSpriteComponent animationComponent;
        private float dashTime = 0;
        private const float totalDashTime = 0.1f;
        private float stamina = 1f;
        // stamina cost per dash, with 0.6 we can perform roughly two dashes
        private const float dashStaminaCost = 0.6f;
        // regenerate to full in about 3s
        private const float staminaRegenerationRate = 0.33f;

        private float dizzyTime = 0;
        private const float totalDizzyTime = 3f;

        private Vector2 dashDirection;


        private Vector2 dashCollisionPoint;
        private Vector2 dashCollisionDirection;

        public PlayerControllerComponent(PlayerInfo player)
        {
            base.GamePadIndex = player.GamepadIndex;

            if (player.IsKeyboardPlayer)
            {
                Mappings.Add(new InputMapping(null, MovementFromKeyboard));

                Mappings.Add(new InputMapping(f => InputFunctions.KeyboardDash(f), f => StartDash()));

                Mappings.Add(new InputMapping(f => InputFunctions.KeyboardStartShout(f), f => SwitchState(PlayerState.Shouting)));
                Mappings.Add(new InputMapping(f => InputFunctions.KeyboardEndShout(f), f => SwitchState(PlayerState.Walking)));

                Mappings.Add(new InputMapping(f => InputFunctions.KeyboardStartLure(f), f => SwitchState(PlayerState.Luring)));
                Mappings.Add(new InputMapping(f => InputFunctions.KeyboardEndLure(f), f => SwitchState(PlayerState.Walking)));
            }
            else
            {
                Mappings.Add(new InputMapping(null, MovementFromGamepad));

                Mappings.Add(new InputMapping(f => InputFunctions.Dash(f), f => StartDash()));


                Mappings.Add(new InputMapping(f => InputFunctions.StartShout(f), f => SwitchState(PlayerState.Shouting)));
                Mappings.Add(new InputMapping(f => InputFunctions.EndShout(f), f => SwitchState(PlayerState.Walking)));

                Mappings.Add(new InputMapping(f => InputFunctions.StartLure(f), f => SwitchState(PlayerState.Luring)));
                Mappings.Add(new InputMapping(f => InputFunctions.EndLure(f), f => SwitchState(PlayerState.Walking)));
            }
        }

        public override void Initialize()
        {
            playerEntity = (PlayerEntity)Entity;
            animationComponent = playerEntity.GetComponentByName<AnimatedSpriteComponent>("playerAnim");

            base.Initialize();
        }

        private bool OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            var other = (Entity)fixtureB.Body.UserData;
            if (playerEntity.State == PlayerState.Dashing)
            {
                var velocity = Entity.Body.LinearVelocity;
                if (velocity.LengthSquared() < 0.01f)
                    velocity = VectorExtensions.AngleToUnitVector(Entity.Body.Rotation);


                Vector2 worldNormal;
                FixedArray2<Vector2> worldPoints;
                contact.CalculateContactPoints(out worldNormal, out worldPoints);
                Vector2 collisionPoint = worldPoints[0];
                var dirToOther = collisionPoint - Entity.Body.Position;

                dashCollisionPoint = collisionPoint;

                // if one of our vectors somehow got 0, cancel, as otherwise NaNs can crash farseer
                if (dirToOther.LengthSquared() < 0.01f)
                    return true;

                var dirToOtherNorm = dirToOther.Normalized();
                dashCollisionDirection = dirToOther;
                var velocityNorm = velocity.Normalized();

                // if the other is not in the direction of the dash, cancel
                if (velocityNorm.AngleBetween(dirToOtherNorm) > MathHelper.PiOver2 - 0.09f)
                    return true;

                (other as BoarEntity)?.Hit(Entity);
                (other as ChickenEntity)?.Hit(Entity);
                other.Body.ApplyLinearImpulse(dirToOtherNorm * 10.0f, other.Body.Position);

                if ((other as ChickenEntity) != null)
                    return true;

                var reflectedDir = Vector2.Reflect(Entity.Body.LinearVelocity, contact.Manifold.LocalNormal);

                if (reflectedDir.LengthSquared() > 0.1f)
                    Entity.Body.ApplyLinearImpulse((-reflectedDir).Normalized() * 10.0f, Entity.Body.Position);

                // SHAKE THAT SCREEN!!!
                Entity.Game.Camera.Shake(TimeSpan.FromSeconds(0.5f), 20f);
                var randI = RandomExt.GetRandomInt(1, 4);
                Entity.GetComponent<AudioSourceComponent>().PlaySound($"Audio/Hit{randI}");

                var otherPlayer = other as PlayerEntity;
                if (otherPlayer != null)
                {
                    otherPlayer.GetComponent<AudioSourceComponent>().PlaySound("Audio/Dizzy");
                    otherPlayer.SwitchToState(PlayerState.Dizzy);
                    // since the other player becomes dizzy, it should vibrate stronger
                    Entity.Game.InputManager.SetRumble(otherPlayer.PlayerInfo, 1.0f, 0.7f, 0.6f);
                }
                if (other.Body.IsStatic)
                {
                    ResetDash();
                    playerEntity.GetComponent<AudioSourceComponent>().PlaySound("Audio/Dizzy");
                    playerEntity.SwitchToState(PlayerState.Dizzy);
                    Entity.Game.InputManager.SetRumble(playerEntity.PlayerInfo, 1.0f, 0.7f, 0.6f);
                }
                else
                    Entity.Game.InputManager.SetRumble(playerEntity.PlayerInfo, 0.75f, 0.25f, 0.3f);
            }
            return true;
        }


        private void MovementFromGamepad(InputFrame obj)
        {
            inputMovementDirection = obj.CurrentFrame.ThumbSticks.Left;
            inputMovementDirection.Y = -inputMovementDirection.Y;
        }

        private void MovementFromKeyboard(InputFrame obj)
        {
            inputMovementDirection = Vector2.Zero;
            if (obj.CurrentKeyboardFrame.IsKeyDown(Keys.A))
                inputMovementDirection.X = -1;

            if (obj.CurrentKeyboardFrame.IsKeyDown(Keys.D))
                inputMovementDirection.X = 1;

            if (obj.CurrentKeyboardFrame.IsKeyDown(Keys.W))
                inputMovementDirection.Y = -1;

            if (obj.CurrentKeyboardFrame.IsKeyDown(Keys.S))
                inputMovementDirection.Y = 1;

            if (inputMovementDirection.LengthSquared() > 0)
                inputMovementDirection.Normalize();
        }

        public void Update(float elapsedSeconds, float totalSeconds)
        {
            float currentMovementSpeed = GetCurrentSpeed();
            Vector2 desiredVelocity;

            // Regenerate stamina
            if (stamina < 1.0f)
            {
                stamina += staminaRegenerationRate * elapsedSeconds;
            }

            // While dashing
            if (playerEntity.State == PlayerState.Dashing)
            {
                desiredVelocity = dashDirection * currentMovementSpeed;
                dashTime += elapsedSeconds;
                //Entity.Body.Mass = 2000;
            }
            else if (playerEntity.State == PlayerState.Dizzy)
            {
                dizzyTime += elapsedSeconds;
                var intendedSpeed = inputMovementDirection.Length();
                var isStandingStill = intendedSpeed <= 0.001f;
                var perpendicularV =
                    (isStandingStill)
                        ? (VectorExtensions.AngleToUnitVector(Entity.Body.Rotation + (float)Math.PI / 2.0f))
                        : (new Vector2(inputMovementDirection.Y, -inputMovementDirection.X).Normalized());
                var inDirectionV = new Vector2(perpendicularV.Y, -perpendicularV.X);

                // Add 'drunk' on movement on top of input
                float drunkNoise1 = (float)(0.5 * Math.Sin(3.4 + totalSeconds * 7.1f) + 0.8 * Math.Sin(totalSeconds * 5.1f) + 1.3 * Math.Sin(1.2f + totalSeconds * 3.3f));
                float drunkNoise2 = (float)(0.3 * Math.Sin(2.1 + totalSeconds * 8.3f) + 0.5 * Math.Sin(totalSeconds * 4.7f) + 0.7 * Math.Sin(5.2f + totalSeconds * 2.5f));
                // adjust the noise when standing still, as otherwise the player just spinns around like crazy
                float d1 = (isStandingStill) ? 0.1f * drunkNoise1 : drunkNoise1;
                float d2 = (isStandingStill) ? (drunkNoise2 + 1.5f) * 0.3f : drunkNoise2;
                desiredVelocity =
                    (inputMovementDirection + inDirectionV * d2 + perpendicularV * d1).Normalized() *
                    (intendedSpeed * 0.3f + 0.3f) * currentMovementSpeed;
                UpdateRotation(elapsedSeconds, desiredVelocity);

            }
            else
            {
                desiredVelocity = inputMovementDirection * currentMovementSpeed;
                UpdateRotation(elapsedSeconds, desiredVelocity);
            }

            // reset dash
            if (playerEntity.State == PlayerState.Dashing && !(dashTime < totalDashTime))
                ResetDash();
            // reset dizzy
            if (playerEntity.State == PlayerState.Dizzy && !(dizzyTime < totalDizzyTime))
                ResetDizzy();



            float drag = 0.2f;
            // interpolate from the current velocity to the desired velocity
            Entity.Body.LinearVelocity = drag * desiredVelocity + (1.0f - drag) * Entity.Body.LinearVelocity;


            // player animation
            if (playerEntity.State == PlayerState.Dashing)
            {
                animationComponent.SwitchTo("dashing");
            }
            else if (playerEntity.State == PlayerState.Dizzy)
            {
                animationComponent.SwitchTo("dizzy");
            }
            else
            {
                if (Entity.Body.LinearVelocity.LengthSquared() > 0.1f)
                    animationComponent.SwitchTo("walking");
                else
                    animationComponent.SwitchTo("standing");
            }
        }

        // this is just a wrapper around playerEntity.SwitchToState(..), but it prevents switching away from dizzy
        private void SwitchState(PlayerState newState)
        {
            // don't allow switching while dizzy
            if (playerEntity.State == PlayerState.Dizzy)
                return;
            if (newState == PlayerState.Luring)
            {
                playerEntity.GetComponent<AudioSourceComponent>().PlaySoundIfNotAlreadyPlaying("Audio/Lure");
            }
            else if (newState == PlayerState.Shouting)
            {
                var randI = RandomExt.GetRandomInt(1, 5);
                Entity.GetComponent<AudioSourceComponent>().PlaySound($"Audio/Shout{randI}");
            }
            playerEntity.SwitchToState(newState);
        }

        private void ResetDizzy()
        {
            playerEntity.SwitchToState(PlayerState.Walking);
            dizzyTime = 0;
        }

        private void StartDash()
        {
            if (stamina <= 0.0 || playerEntity.State == PlayerState.Dizzy)
                return;
            playerEntity.SwitchToState(PlayerState.Dashing);
            dashTime = 0;

            Entity.GetComponent<AudioSourceComponent>().PlaySound("Audio/dashSwoosh");

            var physicsComponent = playerEntity.GetComponent<PhysicsComponent>();
            physicsComponent.ReplaceShape(new CircleShape(0.5f, 1.0f));
            physicsComponent.Fixture.IsSensor = true;
            physicsComponent.Fixture.CollisionCategories = GameConstants.CollisionCategorySensor;
            physicsComponent.Fixture.CollidesWith = Category.All & ~GameConstants.CollisionCategorySensor;
            physicsComponent.Fixture.OnCollision += OnCollision;

            stamina -= dashStaminaCost;

            if (Entity.Body.LinearVelocity.LengthSquared() > 0.0000001f)
                dashDirection = Vector2.Normalize(Entity.Body.LinearVelocity);
            else
                dashDirection = VectorExtensions.AngleToUnitVector(Entity.Body.Rotation);
        }

        private void ResetDash()
        {
            if (stamina <= 0.0)
            {
                stamina = 0;
                playerEntity.SwitchToState(PlayerState.Dizzy);
                playerEntity.GetComponent<AudioSourceComponent>().PlaySound("Audio/Dizzy");
            }
            else
                playerEntity.SwitchToState(PlayerState.Walking);

            playerEntity.GetComponent<PhysicsComponent>().ReplaceShape(new CircleShape(0.25f, 1.0f));
        }

        private void UpdateRotation(float elapsedSeconds, Vector2 desiredVelocity)
        {
            Vector2 direction = desiredVelocity;
            var rotationSpeed =
                (playerEntity.State == PlayerState.Dizzy && inputMovementDirection.LengthSquared() <= 0.001f)
                    ? RotationSpeedPerSecond * 0.1f
                    : RotationSpeedPerSecond;

            if (direction.LengthSquared() > 0)
            {
                Vector2 rotDirection = direction;
                rotDirection.Normalize();
                float angle = rotDirection.UnitVectorToAngle();
                desiredRotation = angle;
            }

            float actualRotation = Entity.Body.Rotation;
            float angleDistance = desiredRotation - actualRotation;

            angleDistance = MathHelper.WrapAngle(angleDistance);

            if (angleDistance > 0)
                Entity.Body.AngularVelocity = Math.Min(angleDistance / elapsedSeconds, rotationSpeed);
            else if (angleDistance < 0)
                Entity.Body.AngularVelocity = Math.Max(angleDistance / elapsedSeconds, -rotationSpeed);
            else
                Entity.Body.AngularVelocity = 0;
        }

        private float GetCurrentSpeed()
        {
            switch (playerEntity.State)
            {
                case PlayerState.Luring:
                    return LuringMovementSpeedPerSecond;
                case PlayerState.Shouting:
                    return ShoutingMovementSpeedPerSecond;
                case PlayerState.Dashing:
                    return DashingMovementSpeedPerSecond;
                default:
                    return WalkingMovementSpeedPerSecond;
            }
        }

        public float DashingMovementSpeedPerSecond { get; set; } = 30;

        public float RotationSpeedPerSecond { get; set; } = 20;
        public float WalkingMovementSpeedPerSecond { get; set; } = 4;

        public float LuringMovementSpeedPerSecond { get; set; } = 1.5f;

        public float ShoutingMovementSpeedPerSecond { get; set; } = 0.5f;
        public void LoadContent()
        {
            Entity.Game.EngineComponents.Get<AudioManager>().PreloadSoundEffects(
                "Audio/Hit1", "Audio/Hit2", "Audio/Hit3", "Audio/Dizzy", "Audio/dashSwoosh", "Audio/Lure"
            );
        }

        public void DebugDraw(DebugRenderer renderer, float elapsedSeconds, float totalSeconds)
        {
            renderer.DebugDrawPoint(dashCollisionPoint, Color.PaleVioletRed);
            renderer.DebugDrawLine(dashCollisionPoint, dashCollisionPoint + dashCollisionDirection, color: Color.PaleVioletRed);
        }
    }
}