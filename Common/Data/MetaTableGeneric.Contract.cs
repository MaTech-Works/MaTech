// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System;
using System.Data;
using UnityEngine;

namespace MaTech.Common.Data {
    public partial class MetaTableGeneric<TEnum> {
        public interface IConstraint {
            bool StopOperationIfInvalid { get; }
        }
        public interface IConstraintTypeOfKey : IConstraint {
            /// <summary> 检查类型是否合理，在实际读写时检查（如Select方法在进行具体的Get/Set前不会触发检查） </summary>
            bool IsValidTypeOfKey<T>(DataEnum<TEnum> key);
        }
        public interface IConstraintValueOfKey : IConstraint {
            /// <summary> 检查值是否合理，仅在写入时检查 </summary>
            bool IsValidValueOfKey<T>(DataEnum<TEnum> key, in T value);
        }

        public partial struct Selector<TContext, TEnum0> {
            /// <summary>
            /// 见 <see cref="MetaTableGenericGeneric{TEnum}.Constraint"/>。
            /// 赋值时会为当前选中的路径创建MetaTable，类似于Set等方法。
            /// </summary>
            public MetaTableGeneric<TEnum0>.IConstraint? Contract {
                get => GetContextTable()?.Constraint;
                set => EnsureContextTable().Constraint = value;
            }
        }

        /// <summary>
        /// 读写值时的自动检查，传入的对象实现IConstraintTypeOfKey或IConstraintValueOfKey时将启用对应的检查。
        /// 仅在编辑器内，或开启Development Build的非IL2CPP平台（即IOS平台不可用）有效。
        /// TODO: 将接口改为允许多重检查规则（类似于List&lt;IConstraint&gt;）
        /// </summary>
        public virtual IConstraint? Constraint { get; set; }

        // TODO: 泛型方法在IL2CPP环境下会被strip掉，需替换成Variant
        #if UNITY_EDITOR || (!ENABLE_IL2CPP && DEVELOPMENT_BUILD)

        private bool CheckConstraintTypeOfKey<T>(DataEnum<TEnum> keyToCheck) {
            if (Constraint is IConstraintTypeOfKey contractTypeOfKey) {
                if (!contractTypeOfKey.IsValidTypeOfKey<T>(keyToCheck)) {
                    Debug.LogError($"[MetaTableGeneric] Contract violated: type [{typeof(T).FullName}] is not allowed for key [{keyToCheck}].");
                    if (contractTypeOfKey.StopOperationIfInvalid)
                        return false;
                }
            }
            return true;
        }
        
        private bool CheckConstraintValueOfKey<T>(DataEnum<TEnum> keyToCheck, in T? valueToCheck) {
            if (Constraint is IConstraintValueOfKey contractValueOfKey) {
                if (!contractValueOfKey.IsValidValueOfKey(keyToCheck, valueToCheck)) {
                    Debug.LogError($"[MetaTableGeneric] Contract violated: value [{valueToCheck?.ToString()}] (of type [{typeof(T).FullName}]) is not allowed for key [{keyToCheck}].");
                    if (contractValueOfKey.StopOperationIfInvalid)
                        return false;
                }
            }
            return true;
        }

        #else
        private bool CheckConstraintTypeOfKey<T>(DataEnum<TEnum> keyToCheck) => true;
        private bool CheckConstraintValueOfKey<T>(DataEnum<TEnum> keyToCheck, in T valueToCheck) => true;
        #endif
    }

    public class MetaTableGenericWithContract<TEnum, TConstraint> : MetaTableGeneric<TEnum> where TEnum : unmanaged, Enum, IConvertible where TConstraint : class, MetaTableGeneric<TEnum>.IConstraint, new() {
        private static readonly TConstraint constraint = new TConstraint();
        public override IConstraint? Constraint {
            get => constraint;
            set => throw new ReadOnlyException($"[MetaTableGenericWithContract] Constraint is readonly and static, cannot be set to another instance.");
        }
    }
}