// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;

#nullable enable

namespace MaTech.Common.Data {
    public readonly partial struct EnumEx<T> {
        private static readonly ReaderWriterLockSlim lockMetadata = new ReaderWriterLockSlim();
        
        private struct ReaderLockRAII : IDisposable {
            private bool read;

            public static ReaderLockRAII Read() {
                lockMetadata.EnterReadLock();
                return new ReaderLockRAII { read = true };
            }

            public void Dispose() {
                if (read) lockMetadata.ExitUpgradeableReadLock();
            }
        }

        private struct WriterLockRAII : IDisposable {
            private bool read;
            private bool write;

            public static WriterLockRAII Read() {
                lockMetadata.EnterUpgradeableReadLock();
                return new WriterLockRAII { read = true };
            }

            public void Write() {
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