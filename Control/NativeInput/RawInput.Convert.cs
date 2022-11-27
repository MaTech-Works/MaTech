// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using UnityEngine;

namespace MaTech.Control.NativeInput {
    public static partial class RawInput {
        public static RawKey ToRawKey(KeyCode code) {
            if (code >= KeyCode.A && code <= KeyCode.Z) {
                return RawKey.A + (ushort) (code - KeyCode.A);
            }

            if (code >= KeyCode.Keypad0 && code <= KeyCode.Keypad9) {
                return RawKey.Numpad0 + (ushort) (code - KeyCode.Keypad0);
            }

            if (code >= KeyCode.Alpha0 && code <= KeyCode.Alpha9) {
                return RawKey.N0 + (ushort) (code - KeyCode.Alpha0);
            }

            if (code >= KeyCode.F1 && code <= KeyCode.F15) {
                return RawKey.F1 + (ushort) (code - KeyCode.F1); // RawKey actually support up to F24
            }

            switch (code) {
                case KeyCode.Mouse0: return RawKey.LeftButton;
                case KeyCode.Mouse1: return RawKey.RightButton;
                case KeyCode.Mouse2: return RawKey.MiddleButton;
                case KeyCode.Mouse3: return RawKey.ExtraButton1;
                case KeyCode.Mouse4: return RawKey.ExtraButton2;

                case KeyCode.LeftShift: return RawKey.LeftShift;
                case KeyCode.RightShift: return RawKey.RightShift;
                case KeyCode.LeftControl: return RawKey.LeftControl;
                case KeyCode.RightControl: return RawKey.RightControl;
                case KeyCode.LeftAlt: return RawKey.LeftMenu;
                case KeyCode.RightAlt: return RawKey.RightMenu;

                case KeyCode.UpArrow: return RawKey.Up;
                case KeyCode.DownArrow: return RawKey.Down;
                case KeyCode.LeftArrow: return RawKey.Left;
                case KeyCode.RightArrow: return RawKey.Right;

                case KeyCode.Numlock: return RawKey.NumLock;
                case KeyCode.CapsLock: return RawKey.CapsLock;
                case KeyCode.ScrollLock: return RawKey.ScrollLock;

                case KeyCode.Return: return RawKey.Return;
                case KeyCode.Space: return RawKey.Space;
                case KeyCode.Tab: return RawKey.Tab;
                case KeyCode.Backspace: return RawKey.Back;

                case KeyCode.PageUp: return RawKey.Next;
                case KeyCode.PageDown: return RawKey.Prior;
                case KeyCode.Home: return RawKey.Home;
                case KeyCode.End: return RawKey.End;
                case KeyCode.Insert: return RawKey.Insert;
                case KeyCode.Pause: return RawKey.Pause;
                case KeyCode.Delete: return RawKey.Delete;

                case KeyCode.Colon: return RawKey.OEM1;
                case KeyCode.Semicolon: return RawKey.OEM1;
                case KeyCode.Slash: return RawKey.OEM2;
                case KeyCode.Question: return RawKey.OEM2;
                case KeyCode.BackQuote: return RawKey.OEM3;
                case KeyCode.Tilde: return RawKey.OEM3;
                case KeyCode.LeftBracket: return RawKey.OEM4;
                case KeyCode.LeftCurlyBracket: return RawKey.OEM4;
                case KeyCode.Pipe: return RawKey.OEM5;
                case KeyCode.Backslash: return RawKey.OEM5;
                case KeyCode.RightBracket: return RawKey.OEM6;
                case KeyCode.RightCurlyBracket: return RawKey.OEM6;
                case KeyCode.Quote: return RawKey.OEM7;
                case KeyCode.DoubleQuote: return RawKey.OEM7;

                case KeyCode.Plus: return RawKey.OEMPlus;
                case KeyCode.Equals: return RawKey.OEMPlus;
                case KeyCode.Minus: return RawKey.OEMMinus;
                case KeyCode.Underscore: return RawKey.OEMMinus;
                case KeyCode.Comma: return RawKey.OEMComma; // KeyCode has no '<' or '>'
                case KeyCode.Period: return RawKey.OEMPeriod;

                case KeyCode.KeypadDivide: return RawKey.Divide;
                case KeyCode.KeypadMultiply: return RawKey.Multiply;
                case KeyCode.KeypadMinus: return RawKey.Subtract;
                case KeyCode.KeypadPlus: return RawKey.Add;
                case KeyCode.KeypadEnter: return RawKey.Return;
                case KeyCode.KeypadPeriod: return RawKey.Decimal;
            }

            return RawKey.None;
        }
    }
}
