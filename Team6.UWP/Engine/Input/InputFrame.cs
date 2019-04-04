using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Team6.Engine.Input
{
    public struct InputFrame
    {
        public GamePadState LastFrame { get; set; }
        public GamePadState CurrentFrame { get; set; }

        public KeyboardState LastKeyboardFrame { get; set; }
        public KeyboardState CurrentKeyboardFrame { get; set; }

        public int PlayerIndex { get; set; }

        /// <summary>
        /// The key was released between this and last frame (and is still released in the current frame)
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>True if the key was just released, otherwise false</returns>
        public bool IsKeyJustReleased(Keys key)
        {
            return LastKeyboardFrame.IsKeyDown(key) && CurrentKeyboardFrame.IsKeyUp(key);
        }

        /// <summary>
        /// The key was pressed between this and last frame (and is still pressed in the current frame)
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>True if the key was just pressed, otherwise false</returns>
        public bool IsKeyJustPressed(Keys key)
        {
            return LastKeyboardFrame.IsKeyUp(key) && CurrentKeyboardFrame.IsKeyDown(key);
        }





        /// <summary>
        /// Either of the keys was pressed between this and last frame (and is still pressed in the current frame)
        /// </summary>
        /// <param name="keys">The keys</param>
        /// <returns>True if any of the keys was just pressed, otherwise false</returns>
        public bool IsAnyKeyJustPressed(params Keys[] keys)
        {
            InputFrame tmpThis = this;
            return keys.Any(k => tmpThis.IsKeyJustPressed(k));
        }

        /// <summary>
        /// The button was pressed between this and last frame (and is still pressed in the current frame)
        /// </summary>
        /// <param name="button">On button</param>
        /// <returns>True if the button was just pressed, otherwise false</returns>
        public bool IsJustPressed(Buttons button)
        {
            return LastFrame.IsButtonUp(button) && CurrentFrame.IsButtonDown(button);
        }


        /// <summary>
        /// Either of the buttons was pressed between this and last frame (and is still pressed in the current frame)
        /// </summary>
        /// <param name="buttons">The buttons</param>
        /// <returns>True if any of the buttons was just pressed, otherwise false</returns>
        public bool IsAnyJustPressed(params Buttons[] buttons)
        {
            InputFrame tmpThis = this;
            return buttons.Any(b => tmpThis.IsJustPressed(b));
        }

        /// <summary>
        /// The button was released between this and last frame (and is still released in the current frame)
        /// </summary>
        /// <param name="button">On button</param>
        /// <returns>True if the button was just released, otherwise false</returns>
        public bool IsJustReleased(Buttons button)
        {
            return LastFrame.IsButtonDown(button) && CurrentFrame.IsButtonUp(button);
        }

        /// <summary>
        /// The key is currently pressed
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>True if the key is currently pressed, otherwise false</returns>
        public bool IsKeyPressed(Keys key)
        {
            return CurrentKeyboardFrame.IsKeyDown(key);
        }

        /// <summary>
        /// The button is currently pressed
        /// </summary>
        /// <param name="button">The button</param>
        /// <returns>True if the button is currently pressed, otherwise false</returns>
        public bool IsKeyPressed(Buttons button)
        {
            return CurrentFrame.IsButtonDown(button);
        }
    }
}
