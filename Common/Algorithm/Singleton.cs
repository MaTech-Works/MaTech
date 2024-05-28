// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Threading;

namespace MaTech.Common.Algorithm {
    /// Think thrice before using this type.
    /// Do the derived class have application-wise lifetime and expected to be refactored to other lifetimes?
    /// Do you really want to give up the ability to replace the global object?
    /// Almost in all times, it's better to have the non-static class to be owned by a instance-managing mechanism or a instance of the game/level/world.
    public class Singleton<T> where T : Singleton<T>, new() {
        private static T instance;
        
        // ReSharper disable once StaticMemberInGenericType
        private static readonly object syncRoot = new object();
        
        public static T SingletonInstance { // ugly name for your second thoughts
            get {
                instance ??= new T();
                return instance;
            }
        }
        
        public static T SingletonInstanceThreadSafe { // even uglier name for your second thoughts
            get {
                if (instance != null)
                    return instance;
                lock (syncRoot) {
                    if (instance != null)
                        return instance;
                    var t = new T();
                    Thread.MemoryBarrier(); // force assignment to happen after new and lock
                    instance = t;
                }
                return instance;
            }
        }
    }
}