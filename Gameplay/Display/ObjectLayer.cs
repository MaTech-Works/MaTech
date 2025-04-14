// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MaTech.Common.Algorithm;
using MaTech.Common.Data;
using MaTech.Common.Utils;
using MaTech.Gameplay.Processor;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.Serialization;
using static MaTech.Gameplay.ChartPlayer;

#nullable enable

namespace MaTech.Gameplay.Display {
    public class ObjectLayer<TCarrier, TLayer> : MonoBehaviour
        where TCarrier : ObjectCarrier<TCarrier, TLayer>
        where TLayer : ObjectLayer<TCarrier, TLayer> {
        
        // todo: remove ratio calculate methods and give out Range<RollUnit>
        // todo: 支持为每个VisualUnit实例化图形，而非为每个NoteCarrier实例化图形
        
        #region Private Fields - Runtime Containers
        
        private readonly struct ObjectTuple {
            public readonly TCarrier carrier;
            public readonly GameObject obj;
            public readonly IObjectVisual<TCarrier, TLayer> visual;
            public ObjectTuple(TCarrier carrier, GameObject obj) {
                this.carrier = carrier;
                this.obj = obj;
                this.visual = obj.GetComponent<IObjectVisual<TCarrier, TLayer>>();
            }
        }
        
        private PointerList<TCarrier> listCarrierEarlyRoll = null!;    // 按远端卷轴进入顺序排序
        private PointerList<TCarrier> listCarrierLateRoll = null!;     // 按近端卷轴退出顺序排序
        private PointerList<TCarrier> listCarrierEarlyTime = null!;    // 按判定时间进入顺序排序
        
        private readonly Dictionary<Carrier, IObjectVisual<TCarrier, TLayer>> hashsetCarrierRealized = new();
        private readonly List<ObjectTuple> listObjectRealized = new();

        protected Dictionary<Carrier, IObjectVisual<TCarrier, TLayer>>.KeyCollection RealizedCarriers => hashsetCarrierRealized.Keys;

        #endregion
        
        #region Private Fields - Pool
        
        [Serializable]
        private class PrefabEntry {
            public DataEnum<ObjectType> type = ObjectType.None;
            public GameObject? prefab = null;
            public int bufferCountInPool = 50;
            public int maxInstantiationPerFrame = 1;
        }

        private class PrefabPool {
            private readonly StackList<GameObject> bufferedGameObjects;

            private readonly ObjectLayer<TCarrier, TLayer> self;
            private readonly PrefabEntry prefabEntry;
            
            private int instantiationCountThisFrame = 0;
            
            private GameObject InstantiateGameObject() {
                instantiationCountThisFrame += 1;
                return Instantiate(prefabEntry.prefab, self.transform)!;
            }
            
            private void BufferSingleGameObject() {
                bufferedGameObjects.Add(InstantiateGameObject());
            }

            public DataEnum<ObjectType> PrefabLayerType => prefabEntry.type;

            public int MaxInstantiationPerFrame => prefabEntry.maxInstantiationPerFrame;
            public int InstantiationCountThisFrame => instantiationCountThisFrame;
            
            public bool CanInstantiateInBudgetThisFrame => instantiationCountThisFrame < prefabEntry.maxInstantiationPerFrame;
            public bool IsInstantiationOverBudgetThisFrame => instantiationCountThisFrame > prefabEntry.maxInstantiationPerFrame;
            
            public PrefabPool(ObjectLayer<TCarrier, TLayer> self, PrefabEntry prefabEntry) {
                this.bufferedGameObjects = new StackList<GameObject>(prefabEntry.bufferCountInPool);
                this.prefabEntry = prefabEntry;
                this.self = self;
            }
            
            public GameObject GetPooledGameObject() {
                return bufferedGameObjects.Count == 0 ? InstantiateGameObject() : bufferedGameObjects.Pop();
            }

            public void RecycleGameObject(GameObject obj) {
                if (self.destroyWithoutRecycling) {
                    Destroy(obj);
                    return;
                }
                bufferedGameObjects.Add(obj);
            }
            
            /// <summary> 循环全object种类，每次给一种类型buffer一个实例，直到pool内数额足够（无视每帧buffer额度） </summary>
            public void BufferGameObjectsUntilFull(int? overrideTotalCount = null) {
                if (!Application.IsPlaying(self)) return; // Unity seems to be executing tasks after play stopped
                int targetCount = overrideTotalCount ?? prefabEntry.bufferCountInPool;
                for (int count = bufferedGameObjects.Count; count < targetCount; ++count) {
                    BufferSingleGameObject();
                }
            }

            /// <summary> 循环全object种类，每次给一种类型buffer一个实例，直到pool内数额足够，或者到达本帧buffer额度上限 </summary>
            /// <returns> 若buffer在budget限度内完全填满返回true，否则返回false </returns>
            public bool BufferGameObjectsForFrame(bool resetInstantiationCount = true) {
                if (!Application.IsPlaying(self)) return false; // Unity seems to be executing tasks after play stopped
                if (resetInstantiationCount) instantiationCountThisFrame = 0;
                while (bufferedGameObjects.Count < prefabEntry.bufferCountInPool) {
                    if (!CanInstantiateInBudgetThisFrame) return false;
                    BufferSingleGameObject();
                }
                return true;
            }

            public void DestroyAllBufferedGameObjects() {
                foreach (var obj in bufferedGameObjects) {
                    Destroy(obj);
                }
            }
        }
        
        private readonly Dictionary<DataEnum<ObjectType>, PrefabPool> pools = new Dictionary<DataEnum<ObjectType>, PrefabPool>();

        #endregion
        
        #region Public Fields - Unity Serialized Fields

        [Header("Visuals")]
        
        [SerializeField]
        [Tooltip("逐note实例化的prefab，实现IDisplayObject的mono")]
        private PrefabEntry[] prefabEntries = null!;

        [Space]

        [Tooltip("是否重载判定逻辑给定的最大范围，不选择重载则数值会在运行时自动更新。")]
        public bool overrideJudgeWindow;
        [Tooltip("判定时机的最大范围（较晚一侧），在此范围内音符的图形不会被移除。"), EnableIf("overrideJudgeWindow"), FormerlySerializedAs("judgeWindowUp")] 
        public double windowDeltaTimeEarly = 0.5;
        [Tooltip("判定时机的最大范围（较早一侧），在此范围内音符的图形不会被移除。"), EnableIf("overrideJudgeWindow"), FormerlySerializedAs("judgeWindowDown")] 
        public double windowDeltaTimeLate = -0.5;
        
        [Tooltip("是否重载ChartPlayer中指定的显示范围，不选择重载则数值会在运行时自动更新。")]
        public bool overrideDisplayWindow;
        [Tooltip("图形轴的显示范围（较晚一侧），在此范围内音符的图形不会被移除。"), EnableIf("overrideDisplayWindow"), FormerlySerializedAs("displayWindowUpY")] 
        public double windowDeltaRollEarly = 1;
        [Tooltip("图形轴的显示范围（较早一侧），在此范围内音符的图形不会被移除。"), EnableIf("overrideDisplayWindow"), FormerlySerializedAs("displayWindowDownY")] 
        public double windowDeltaRollLate = -0.1;

        [Header("Debugging")]

        public bool doNotClearChildrenOnAwake = false;
        public bool bufferAllObjectsAtOnce = false;
        public bool destroyWithoutRecycling = false;
        public bool destroyPoolObjectsOnReload = false;
        public bool logRealization = false;

        [HideInInspector]
        [Tooltip("Whether the layer should be updating itself using Unity's Update callback, or let it to be driven by others. " +
                 "This will be set to false when passed to a ChartPlayer.")]
        public bool updateSelf; // todo: 把这个过程自动化

        [HideInInspector]
        public UnityEvent onAfterUpdate = null!;

        #endregion
        
        #region Fields - Non-Container Fields
        
        private bool isLoaded = false;
        
        private double lastRoll = double.MinValue;

        private double speedScale = 1.0;
        private double invSpeedScale = 1.0;

        private double WindowDeltaRollScaledEarly => windowDeltaRollEarly * invSpeedScale;
        private double WindowDeltaRollScaledLate => windowDeltaRollLate * invSpeedScale;
        
        private Func<TCarrier, bool> cachedCarrierSelectCondition = null!;
        private Func<TCarrier, bool>[] cachedIterateStopConditions = null!;
        private Action<TCarrier> cachedRealizationAction = null!;
        private Func<ObjectTuple, bool> cachedVirtualizationAction = null!;
        private bool isActionAndFuncCached = false;
        
        #endregion

        #region Public Methods - Setters and Getters
        
        public double SpeedScale => speedScale;

        public void UpdateSpeedScale(double value, bool updateGraphics = true) {
            speedScale = value;
            invSpeedScale = 1.0 / value;
            if (isLoaded && updateGraphics) {
                SortCarriers();
                UpdateGraphics();
            }
        }
        
        public float DeltaRollToRatio(double roll) => (float)(roll / windowDeltaRollEarly);
        
        public float CalculateRatio(double roll, TCarrier carrier, bool clampToDisplayWindow) => DeltaRollToRatio(CalculateDeltaRoll(roll, carrier, clampToDisplayWindow));
        public double CalculateDeltaRoll(double roll, TCarrier carrier, bool clampToDisplayWindow) {
            var deltaRoll = carrier.DeltaRoll(PlayTime.GlobalRoll, roll) * speedScale;
            if (clampToDisplayWindow) return MathUtil.Clamp(deltaRoll, windowDeltaRollLate, windowDeltaRollEarly);
            return deltaRoll;
        }

        public (double roll, float ratio) CalculateDeltaRollAndRatio(double roll, TCarrier carrier, bool clampToDisplayWindow) {
            var deltaRoll = CalculateDeltaRoll(roll, carrier, clampToDisplayWindow);
            return (deltaRoll, DeltaRollToRatio(deltaRoll));
        }

        public IObjectVisual<TCarrier, TLayer>? FindVisual(TCarrier carrier) => hashsetCarrierRealized.GetOrNull(carrier);
        public T? FindVisual<T>(TCarrier carrier) where T : class, IObjectVisual<TCarrier, TLayer> => FindVisual(carrier) as T;

        #endregion
        
        #region Public Methods - Load and Update
        
        // TODO: 改为继承自PlayBehavior，并且将ChartPlayer中的Processor依赖移至这里
        public async UniTask Load(IList<TCarrier> carriers) {
            Assert.IsNotNull(carriers);

            await UniTask.SwitchToMainThread();
            if (isLoaded) await Unload();
            await UniTask.SwitchToThreadPool();
            
            // 以下各种函数中用到的回调依赖this捕获，而C#尚未引入静态lambda对象优化，所以在这里一次性创建好
            if (!isActionAndFuncCached) {
                cachedCarrierSelectCondition = carrier => pools.ContainsKey(carrier.type);
                cachedIterateStopConditions = new Func<TCarrier, bool>[] {
                    carrier => carrier.TargetRoll(WindowDeltaRollScaledEarly, true) > PlayTime.GlobalRoll, // Early Roll Forward
                    carrier => carrier.TargetRoll(WindowDeltaRollScaledEarly, true) < PlayTime.GlobalRoll, // Early Roll Backward
                    carrier => carrier.TargetRoll(WindowDeltaRollScaledLate, false) > PlayTime.GlobalRoll, // Late Roll Forward
                    carrier => carrier.TargetRoll(WindowDeltaRollScaledLate, false) < PlayTime.GlobalRoll, // Late Roll Backward
                    carrier => carrier.StartTime - windowDeltaTimeEarly > PlayTime.InputTime.Seconds,   // Early Time Forward
                }; 
                cachedRealizationAction = RealizeCarrierIfInRange;
                cachedVirtualizationAction = tuple => {
                    bool outOfRange = !IsCarrierInRange(tuple.carrier);
                    bool isFinished = tuple.visual.IsVisualFinished;
                    bool ignoreDisplayWindow = tuple.visual.IgnoreDisplayWindow;
                    if (isFinished || (!ignoreDisplayWindow && outOfRange)) {
                        VirtualizeObject(tuple);
                        return true;
                    }
                    return false;
                };
                isActionAndFuncCached = true;
            }
            
            // Pool
            
            hashsetCarrierRealized.Clear();
            listObjectRealized.Clear();

            if (destroyPoolObjectsOnReload) {
                await UniTask.SwitchToMainThread();
                foreach (var pool in pools.Values) {
                    pool.DestroyAllBufferedGameObjects();
                }
                await UniTask.SwitchToThreadPool();
                pools.Clear();
                foreach (var entry in prefabEntries) {
                    if (UnityUtil.IsUnassigned(entry.prefab)) continue;
                    pools[entry.type] = new PrefabPool(this, entry);
                }
            } else {
                foreach (var entry in prefabEntries) {
                    if (UnityUtil.IsUnassigned(entry.prefab)) continue;
                    if (!pools.ContainsKey(entry.type)) {
                        pools[entry.type] = new PrefabPool(this, entry);
                    }
                }
            }
            
            // Sorted lists
            
            var selectedCarriers = carriers.Where(cachedCarrierSelectCondition).ToArray();
            listCarrierEarlyRoll = new PointerList<TCarrier>(selectedCarriers);
            listCarrierLateRoll = new PointerList<TCarrier>(selectedCarriers);
            listCarrierEarlyTime = new PointerList<TCarrier>(selectedCarriers);
            
            SortCarriers();
            
            // Pre-buffer

            await UniTask.SwitchToMainThread();
            
            int? overrideBufferTotalCount = bufferAllObjectsAtOnce ? selectedCarriers.Length : null;
            foreach (var pool in pools.Values) {
                pool.BufferGameObjectsUntilFull(overrideBufferTotalCount);
            }
            
            // Finish

            lastRoll = double.MinValue;
            isLoaded = true;
        }

        public async UniTask Unload() {
            await UniTask.SwitchToMainThread();
            foreach (var tuple in listObjectRealized) {
                VirtualizeObject(tuple);
            }
            isLoaded = false;
        }

        public void UpdateGraphics() {
            if (!isLoaded) return;
            
            // 补充pool内缓冲的object数量
            foreach (var pool in pools.Values) {
                pool.BufferGameObjectsForFrame();
            }
            
            // 从几个listCarrier中顺序找到经过边界位置的，将新进入展示范围的carrier加入展示列表
            if (PlayTime.GlobalRoll >= lastRoll) {
                IterateForward(listCarrierLateRoll, cachedIterateStopConditions[2], cachedRealizationAction);
                IterateForward(listCarrierEarlyRoll, cachedIterateStopConditions[0], cachedRealizationAction);
            } else {
                IterateBackward(listCarrierEarlyRoll, cachedIterateStopConditions[1], cachedRealizationAction);
                IterateBackward(listCarrierLateRoll, cachedIterateStopConditions[3], cachedRealizationAction);
            }
            IterateForward(listCarrierEarlyTime, cachedIterateStopConditions[4], cachedRealizationAction);
            
            lastRoll = PlayTime.GlobalRoll;
            
            // 遍历展示列表，更新note图形
            foreach (var tuple in listObjectRealized) {
                tuple.visual.UpdateVisual();
            }

            // 展示列表中，超过显示范围且可以移除的，放回pool
            listObjectRealized.RemoveCyclicWhere(cachedVirtualizationAction);
            
            onAfterUpdate.Invoke();
            
            // 极端情况log
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            foreach (var pool in pools.Values) {
                if (pool.IsInstantiationOverBudgetThisFrame) {
                    Debug.Log($"[ObjectLayer] Layer \"{name}\" instantiated too many objects of type \"{pool.PrefabLayerType}\" on one frame!\n" +
                        $"Budget {pool.MaxInstantiationPerFrame}, instantiated {pool.InstantiationCountThisFrame}.");
                }
            }
            #endif
        }
        
        #endregion
        
        #region Private Methods - Iteration

        private void SortCarriers() {
            // 排序后carrier会按顺序经过这些边界位置；经过这些边界位置的carrier均需测试是否需要实例化音符。
            ProcessorBasic.SortCarriersByRoll<TCarrier, TLayer>(listCarrierEarlyRoll, true, WindowDeltaRollScaledEarly);
            ProcessorBasic.SortCarriersByRoll<TCarrier, TLayer>(listCarrierLateRoll, false, WindowDeltaRollScaledLate);
            ShellSort.Hibbard(listCarrierEarlyTime, Carrier.ComparerStartTime());
        }
        
        private void IterateForward(PointerList<TCarrier> list, Func<TCarrier, bool> stopCondition, Action<TCarrier> actionOnForward) {
            while (list.IsPointerValid) {
                var carrier = list.Get();
                if (stopCondition(carrier)) break;
                actionOnForward?.Invoke(carrier);
                list.SkipToNext();
            }
        }
            
        private void IterateBackward(PointerList<TCarrier> list, Func<TCarrier, bool> stopCondition, Action<TCarrier> actionOnBackward) {
            while (list.HasLast) {
                var carrier = list.PeekLast();
                if (stopCondition(carrier)) break;
                actionOnBackward?.Invoke(carrier);
                list.SkipToLast();
            }
        }
        
        #endregion
        
        #region Private Methods - Realization

        private bool IsCarrierRealized(TCarrier carrier) => hashsetCarrierRealized.ContainsKey(carrier);

        private void RealizeCarrierIfInRange(TCarrier carrier) {
            if (IsCarrierInRange(carrier) && !IsCarrierRealized(carrier))
                RealizeObject(carrier);
        }

        private void RealizeObject(TCarrier carrier) {
            Assert.IsTrue(IsLayerTypeValid(carrier.type));
            Assert.IsFalse(IsCarrierRealized(carrier));
            
            var pool = pools[carrier.type];
            var obj = pool.GetPooledGameObject();
            
            var tuple = new ObjectTuple(carrier, obj);
            tuple.visual.StartVisual(carrier, (TLayer)this);
            
            if (tuple.visual.IsVisualFinished) {
                tuple.visual.FinishVisual();
                pool.RecycleGameObject(tuple.obj);
                return;
            }
        
            listObjectRealized.Add(tuple);
            hashsetCarrierRealized.Add(carrier, tuple.visual);

            if (logRealization) Debug.Log($"Realize {carrier.StartRoll:F2} - {carrier.EndRoll:F2} at {PlayTime.GlobalRoll:F2} (delta: {carrier.DeltaRoll(PlayTime.GlobalRoll, true):F2} - {carrier.DeltaRoll(PlayTime.GlobalRoll, false):F2})");
        }

        private void VirtualizeObject(ObjectTuple tuple) {
            Assert.IsTrue(IsLayerTypeValid(tuple.carrier.type));
            Assert.IsTrue(IsCarrierRealized(tuple.carrier));
            
            hashsetCarrierRealized.Remove(tuple.carrier);
            
            var pool = pools[tuple.carrier.type];

            tuple.visual.FinishVisual();
            pool.RecycleGameObject(tuple.obj);
            
            if (logRealization) Debug.Log($"Virtualize {tuple.carrier.StartRoll:F2} - {tuple.carrier.EndRoll:F2} at {PlayTime.GlobalRoll:F2} (delta: {tuple.carrier.DeltaRoll(PlayTime.GlobalRoll, true):F2} - {tuple.carrier.DeltaRoll(PlayTime.GlobalRoll, false):F2})");
        }

        #endregion
        
        #region Private Methods - Sanity Check

        private const double EpsilonK = 1.000001;
        
        private bool IsLayerTypeValid(DataEnum<ObjectType> type) {
            return pools.ContainsKey(type);
        }

        private bool IsCarrierInRange(TCarrier carrier) {
            return (carrier.StartTime <= PlayTime.InputTime.Seconds + windowDeltaTimeEarly && carrier.EndTime >= PlayTime.InputTime.Seconds + windowDeltaTimeLate) ||
                   (carrier.DeltaRoll(PlayTime.GlobalRoll, true) <= WindowDeltaRollScaledEarly * EpsilonK && carrier.DeltaRoll(PlayTime.GlobalRoll, false) >= WindowDeltaRollScaledLate * EpsilonK);
        }
        
        #endregion
        
        #region Unity Event Methods

        void Awake() {
            if (!doNotClearChildrenOnAwake) transform.DestroyAllChildren();
        }
        
        void Update() {
            if (updateSelf) UpdateGraphics();
        }

        void OnValidate() {
            for (int i = 0; i < prefabEntries.Length; i++) {
                if (prefabEntries[i].prefab == null) continue;
                if (prefabEntries[i].prefab!.GetComponent<IObjectVisual<TCarrier, TLayer>>() == null) {
                    Debug.LogError($"<b>[{typeof(TLayer).Name}]</b> The object prefab {i} does not contain an IDisplayObject<{typeof(TCarrier).Name}, {typeof(TLayer).Name}> component on the root object", this);
                    prefabEntries[i].prefab = null;
                }
            }
        }
        
        #endregion
    }
}