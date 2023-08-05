// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using MaTech.Common.Utils;
using UnityEngine;
using UnityEngine.Assertions;
using UnityScene = UnityEngine.SceneManagement.Scene;

namespace MaTech.Common.Unity {
    // TODO: 替换成无需继承的实现，要求手动调用AddInstance等方法缓存自身引用（也相当于允许自定义有效生命周期，以及提供可选而充分的错误处理）
    [DefaultExecutionOrder(-800)]
    public abstract class SinglePerSceneBehaviour<TDerive> : MonoBehaviour where TDerive : SinglePerSceneBehaviour<TDerive> {
        private static readonly Dictionary<UnityScene, TDerive> instances = new Dictionary<UnityScene, TDerive>();

        public static TDerive GetInstance(UnityScene scene) {
            if (instances.TryGetValue(scene, out var result)) return result;
            var obj = scene.GetComponentInScene<TDerive>();
            if (obj != null) AddInstance(obj);
            return obj;
        }

        private static void AddInstance(TDerive instance) {
            var scene = instance.gameObject.scene;
            Assert.IsFalse(instances.TryGetValue(scene, out var result) && result != null && result != instance,
                $"<b>[SinglePerScene]</b> Cannot have multiple {typeof(TDerive).Name} in the same scene.");
            instances.Add(scene, instance);
        }

        private static void RemoveInstance(TDerive instance) {
            instances.Remove(instance.gameObject.scene);
        }

        protected virtual void Awake() => AddInstance(this as TDerive);
        protected virtual void OnDestroy() => RemoveInstance(this as TDerive);
    }
}