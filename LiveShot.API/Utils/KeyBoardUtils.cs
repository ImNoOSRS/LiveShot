﻿using System.Windows.Input;

namespace LiveShot.API.Utils
{
    public static class KeyBoardUtils
    {
        public static readonly bool IsCtrlPressed =
            Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

        public static readonly bool IsShiftPressed =
            Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
    }
}