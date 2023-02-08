// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace MaTech.Common.Algorithm {
    /// Think thrice before using this type.
    /// Do the derived class have application-wise lifetime and expected to be refactored to other lifetimes?
    /// Do you really want to give up the ability to replace the global object?
    /// Almost in all times, it's better to have the non-static class to be owned by a instance-managing mechanism or a instance of the game/level/world.
    public class Singleton<T> where T : Singleton<T>, new() {
        private static T instance;
        public static T G {
            get {
                instance ??= new T();
                return instance;
            }
        }
    }
}