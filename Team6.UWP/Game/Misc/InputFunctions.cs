using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Team6.Engine.Input;

namespace Team6.Game.Misc
{
    public static class InputFunctions
    {
        #region Keyboard Menu
        internal static bool KeyboardMenuLeft(InputFrame arg)
        {
            return arg.IsKeyJustPressed(Keys.A);
        }

        internal static bool KeyboardMenuRight(InputFrame arg)
        {
            return arg.IsKeyJustPressed(Keys.D);
        }

        internal static bool KeyboardMenuUp(InputFrame arg)
        {
            return arg.IsKeyJustPressed(Keys.W);
        }

        internal static bool KeyboardMenuDown(InputFrame arg)
        {
            return arg.IsKeyJustPressed(Keys.S);
        }

        internal static bool KeyboardMenuSelect(InputFrame arg)
        {
            return arg.IsKeyJustPressed(Keys.K);
        }

        internal static bool KeyboardMenuStart(InputFrame arg)
        {
            return arg.IsKeyJustPressed(Keys.Enter);
        }

        internal static bool KeyboardMenuBack(InputFrame arg)
        {
            return arg.IsKeyJustPressed(Keys.Back) || arg.IsKeyJustPressed(Keys.L);
        }

        internal static bool KeyboardMenuRandom(InputFrame arg)
        {
            return arg.IsKeyJustPressed(Keys.J);
        }
        #endregion

        #region Keyboard Gameplay
        internal static bool KeyboardStartShout(InputFrame arg)
        {
            return arg.IsKeyJustPressed(Keys.L);
        }

        internal static bool KeyboardEndShout(InputFrame arg)
        {
            return arg.IsKeyJustReleased(Keys.L);
        }

        internal static bool KeyboardStartLure(InputFrame arg)
        {
            return arg.IsKeyJustPressed(Keys.K);
        }

        internal static bool KeyboardEndLure(InputFrame arg)
        {
            return arg.IsKeyJustReleased(Keys.K);
        }

        internal static bool KeyboardDash(InputFrame arg)
        {
            return arg.IsKeyJustPressed(Keys.J);
        }

        internal static bool KeyboardPause(InputFrame arg)
        {
            return arg.IsKeyJustPressed(Keys.Escape) || arg.IsKeyJustPressed(Keys.Enter);
        }
        #endregion

        #region GamePad Menu

        internal static bool MenuLeft(InputFrame arg)
        {
            return arg.IsJustPressed(Buttons.LeftThumbstickLeft) || arg.IsJustPressed(Buttons.DPadLeft);
        }

        internal static bool MenuRight(InputFrame arg)
        {
            return arg.IsJustPressed(Buttons.LeftThumbstickRight) || arg.IsJustPressed(Buttons.DPadRight);
        }
        
        internal static bool MenuUp(InputFrame arg)
        {
            return arg.IsJustPressed(Buttons.LeftThumbstickUp) || arg.IsJustPressed(Buttons.DPadUp);
        }

        internal static bool MenuDown(InputFrame arg)
        {
            return arg.IsJustPressed(Buttons.LeftThumbstickDown) || arg.IsJustPressed(Buttons.DPadDown);
        }

        internal static bool MenuSelect(InputFrame arg)
        {
            return arg.IsJustPressed(Buttons.A);
        }

        internal static bool MenuStart(InputFrame arg)
        {
            return arg.IsJustPressed(Buttons.Start);
        }
        
        internal static bool MenuBack(InputFrame arg)
        {
            return arg.IsJustPressed(Buttons.B);
        }

        internal static bool MenuRandom(InputFrame arg)
        {
            return arg.IsJustPressed(Buttons.X);
        }
        #endregion

        #region Gamepad Gameplay
        internal static bool StartShout(InputFrame arg)
        {
            return arg.IsJustPressed(Buttons.B);
        }

        internal static bool EndShout(InputFrame arg)
        {
            return arg.IsJustReleased(Buttons.B);
        }

        internal static bool StartLure(InputFrame arg)
        {
            return arg.IsJustPressed(Buttons.A);
        }

        internal static bool EndLure(InputFrame arg)
        {
            return arg.IsJustReleased(Buttons.A);
        }

        internal static bool Dash(InputFrame arg)
        {
            return arg.IsJustPressed(Buttons.RightShoulder);
        }

        internal static bool Pause(InputFrame arg)
        {
            return arg.IsJustPressed(Buttons.Start);
        }
        #endregion

        #region Debug and PC specific
        internal static bool DrawDebug(KeyboardState keyboardState)
        {
            return keyboardState.IsKeyDown(Keys.Space);
        }

        internal static bool FullScreen(InputFrame arg)
        {
            return arg.IsKeyJustPressed(Keys.F11);
        }

        internal static bool SpawnBoar(InputFrame arg)
        {
            return arg.IsKeyJustPressed(Keys.F5);
        }

        internal static bool SpawnChicken(InputFrame arg)
        {
            return arg.IsKeyJustPressed(Keys.F6);
        }

        internal static bool EndRound(InputFrame arg)
        {
            return arg.IsKeyJustPressed(Keys.Delete);
        }

        internal static bool DebugModifierIsOn(KeyboardState keyboardState)
        {
            return keyboardState.IsKeyDown(Keys.LeftShift);
        }

        internal static bool StartCountdown(InputFrame i)
        {
            return i.IsJustPressed(Buttons.RightStick);
        }

        #endregion
    }
}
