// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Linq;
using MaTech.Gameplay.Input.NativeInput;
using UnityEngine;

namespace MaTech.Gameplay.Input {
    public struct KeyBinding {
        public readonly KeyCode[] keyCodes;

        public KeyBinding(params KeyCode[] keyCodes) {
            // if the array shouldn't be shared, do it by your hands :)
            this.keyCodes = keyCodes;
        }

        public int KeyCount => keyCodes?.Length ?? 0;
        public bool IsValid => KeyCount > 0 && keyCodes.All(keyCode => keyCode != KeyCode.None);

        public KeyCode KeyCodeAt(int index) {
            if (keyCodes == null || index < 0 || index >= keyCodes.Length) return KeyCode.None;
            return keyCodes[index];
        }

        public RawKey RawKeyAt(int index) {
            if (keyCodes == null || index < 0 || index >= keyCodes.Length) return RawKey.None;
            return RawInput.ToRawKey(keyCodes[index]);
        }

        public int IndexOf(KeyCode key) {
            if (keyCodes == null) return -1;
            for (int i = 0; i < keyCodes.Length; i++) {
                if (keyCodes[i] == key) return i;
            }

            return -1;
        }

        public int[] ToArray() {
            var ret = new int[keyCodes.Length];
            for (int i = 0; i < keyCodes.Length; i++) {
                ret[i] = (int) keyCodes[i];
            }

            return ret;
        }
    
    }
}