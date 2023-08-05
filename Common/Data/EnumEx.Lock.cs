// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System;
using System.Threading;

namespace MaTech.Common.Data {
    public partial struct EnumEx<T> {
        private static readonly ReaderWriterLockSlim lockMetadata = new ReaderWriterLockSlim();
        
        private struct ReaderLockRAII : IDisposable {
            private bool read;

            public static ReaderLockRAII EnterRead() {
                lockMetadata.EnterReadLock();
                return new ReaderLockRAII { read = true };
            }

            public void Dispose() {
                if (read) lockMetadata.ExitReadLock();
            }
        }

        private struct WriterLockRAII : IDisposable {
            private bool read;
            private bool write;

            public static WriterLockRAII EnterRead() {
                lockMetadata.EnterUpgradeableReadLock();
                return new WriterLockRAII { read = true };
            }

            public void EnterWrite() {
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