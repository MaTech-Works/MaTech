// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace MaTech.Common.Algorithm {
    public class Singleton<T> where T : Singleton<T>, new() {
        private static T instance;
        public static T G {
            get {
                if (instance == null)
                    instance = new T();
                return instance;
            }
        }
    }
}