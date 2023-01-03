// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Threading;

namespace MaTech.Common.Algorithm {
    public readonly partial struct EnumEx<T> where T : struct, Enum, IConvertible {
        private static readonly Type typeEnum = typeof(T);
        
        private static readonly Dictionary<string, T> mapNameToEnum = new Dictionary<string, T>();
        private static readonly Dictionary<T, string> mapEnumToName = new Dictionary<T, string>();
        private static ulong maxEnumIndex;

        private static readonly ReaderWriterLockSlim lockMetadata = new ReaderWriterLockSlim();
        
        private struct LockRAII : IDisposable {
            private bool read;
            private bool write;

            public static LockRAII UpgradeableReadLock() {
                lockMetadata.EnterUpgradeableReadLock();
                return new LockRAII { read = true };
            }

            public void UpgradeToWriteLock() {
                if (write) return;
                lockMetadata.EnterWriteLock();
                write = true;
            }

            public void Dispose() {
                if (write) lockMetadata.ExitWriteLock();
                if (read) lockMetadata.ExitUpgradeableReadLock();
            }
        }
    }
}