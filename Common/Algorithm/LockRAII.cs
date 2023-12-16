// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;

namespace MaTech.Common.Algorithm {
    public struct ReaderLockRAII : IDisposable {
        private ReaderWriterLockSlim target;
        private bool read;

        public static ReaderLockRAII EnterRead(ReaderWriterLockSlim target) {
            target.EnterReadLock();
            return new ReaderLockRAII { target = target, read = true };
        }

        public void Dispose() {
            if (read) target.ExitReadLock();
        }
    }

    public struct WriterLockRAII : IDisposable {
        private ReaderWriterLockSlim target;
        private bool read;
        private bool write;

        public static WriterLockRAII EnterRead(ReaderWriterLockSlim target) {
            target.EnterUpgradeableReadLock();
            return new WriterLockRAII { target = target, read = true };
        }

        public static WriterLockRAII EnterWrite(ReaderWriterLockSlim target) {
            target.EnterWriteLock();
            return new WriterLockRAII { target = target, write = true };
        }
        
        public void EnterWriteFromRead() {
            if (write) return;
            target.EnterWriteLock();
            write = true;
        }

        public void Dispose() {
            if (write) target.ExitWriteLock();
            if (read) target.ExitUpgradeableReadLock();
        }
    }
}