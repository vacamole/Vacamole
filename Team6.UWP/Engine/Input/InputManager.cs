using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Team6.Engine.Entities;
using Microsoft.Xna.Framework.Input;
using Team6.Engine.Animations;
using Team6.Engine.Components;
using Team6.Game.Mode;

namespace Team6.Engine.Input
{
    public class InputManager : EngineComponent
    {
        private GamePadState[] oldState;
        private GamePadState[] currentState;

        private InputFrame frame = new InputFrame();

        private List<InputMapping> globalHooks = new List<InputMapping>();


        public InputManager(MainGame game) : base(game)
        {
            oldState = new GamePadState[GetMaxGamePadCount()];
            currentState = new GamePadState[GetMaxGamePadCount()];
            this.Dispatcher = new Dispatcher();
        }

        private static int GetMaxGamePadCount()
        {
            return Math.Min(4, GamePad.MaximumGamePadCount);
        }

        public Dispatcher Dispatcher { get; set; }

        public override void Update(GameTime gameTime, float elapsedSeconds, float totalSeconds)
        {
            base.Update(gameTime, elapsedSeconds, totalSeconds);
            Dispatcher.Update(elapsedSeconds, totalSeconds);

            for (int i = 0; i < currentState.Length; i++)
            {
                oldState[i] = currentState[i];
                currentState[i] = GamePad.GetState(i);
            }

            frame.LastKeyboardFrame = frame.CurrentKeyboardFrame;
            frame.CurrentKeyboardFrame = Keyboard.GetState();


            // [FOREACH PERFORMANCE] Should not allocate garbage
            foreach (var entity in Game.CurrentScene.Entities)
            {
                if (!Game.CurrentScene.IsPaused || entity.EntityType != EntityType.Game)
                {
                    // [FOREACH PERFORMANCE] Should not allocate garbage
                    foreach (var component in entity.GetAllComponents<InputComponent>())
                    {
                        int i = component.GamePadIndex;
                        frame.PlayerIndex = i;
                        frame.LastFrame = oldState[i];
                        frame.CurrentFrame = currentState[i];
                        component.UpdateInput(ref frame);
                    }
                }
            }

            frame.PlayerIndex = 0;

            // [FOREACH PERFORMANCE] Should not allocate garbage
            foreach (var mapping in globalHooks)
                mapping.Execute(ref frame);
        }

        public void SetRumble(PlayerInfo playerInfo, float lowFrequencyStrength, float highFrequencyStrength, float duration)
        {
            if (playerInfo.IsKeyboardPlayer)
                return;
            Dispatcher.AddAnimation(Animation.Get(0, 1, duration, false, f =>
            {
                GamePad.SetVibration(playerInfo.GamepadIndex, lowFrequencyStrength * (1 - f), highFrequencyStrength * (1 - f));
            }, EasingFunctions.ToEaseOut(EasingFunctions.QuadIn)));

        }

        public void AddGlobalHooks(params InputMapping[] hooks)
        {
            globalHooks.AddRange(hooks);
        }
    }
}
