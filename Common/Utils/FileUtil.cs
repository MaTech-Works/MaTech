// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;

namespace MaTech.Common.Utils {
    public static class FileUtil {
        public static bool IsIgnoredFolder(string name) {
            return name == "__macosx" || name.StartsWith(".");
        }

        public enum BinaryFormat {
            // ReSharper disable InconsistentNaming
            UNKNOWN,
            OGG,
            MP3,
            WAV,
            PNG,
            JPG,
            ZIP
            // ReSharper restore InconsistentNaming
        }

        public static BinaryFormat DetectFormat(string filename) {
            var buffer = new byte[4];
            
            try {
                using var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (stream.Read(buffer, 0, 4) < 4) return BinaryFormat.UNKNOWN;
            } catch (Exception) {
                return BinaryFormat.UNKNOWN;
            }
            
            if (buffer[0] == 'O' && buffer[1] == 'g' && buffer[2] == 'g' && buffer[3] == 'S') return BinaryFormat.OGG;
            if (buffer[0] == 0x89 && buffer[1] == 'P' && buffer[2] == 'N' && buffer[3] == 'G')  return BinaryFormat.PNG;
            if (buffer[0] == 0xff && buffer[1] == 0xd8) return BinaryFormat.JPG;
            if (buffer[0] == 'R' && buffer[1] == 'I' && buffer[2] == 'F' && buffer[3] == 'F') return BinaryFormat.WAV;
            if (buffer[0] == 'I' && buffer[1] == 'D' && buffer[2] == '3') return BinaryFormat.MP3;
            if (buffer[0] == 0x50 && buffer[1] == 0x4b && buffer[2] == 0x3 && buffer[3] == 0x4) return BinaryFormat.ZIP;

            return BinaryFormat.UNKNOWN;
        }

        public static string ToLinuxPath(this string path) {
            return path.Replace('\\', '/');
        }
        
        public static string TrimIOSRoot(string raw) {
            if (!raw.StartsWith("/var/mobile/Containers/Data/Application/")) return raw;
            if (!raw.Contains("/Documents/")) return raw;
            return raw.Substring(87);
        }
        
        public static String ReadLineUntil(this StreamReader streamReader, char delimiter, StringBuilder builder = null) {
            if (streamReader.EndOfStream) return null;

            if (builder == null) builder = new StringBuilder();

            while (!streamReader.EndOfStream) {
                char c = (char) streamReader.Read();
                if (c == delimiter) break;
                if (c == '\r' || c == '\n') {
                    if (c == '\r' && (char)streamReader.Peek() == '\n') {
                        streamReader.Read();
                    }
                    break;
                }
                builder.Append(c);
            }

            return builder.ToString();
        }
        
        public static String ReadUntil(this StreamReader streamReader, char delimiter, StringBuilder builder = null) {
            if (streamReader.EndOfStream) return null;
            
            if (builder == null) builder = new StringBuilder();
            else builder.Clear();

            while (!streamReader.EndOfStream) {
                char c = (char) streamReader.Read();
                if (c == delimiter) break;
                builder.Append(c);
            }

            return builder.ToString();
        }

        // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/using-async-for-file-access
        public static async UniTask<string> ReadFileAsString(string path) {
            await using var _ = UniTask.ReturnToCurrentSynchronizationContext();
            await UniTask.SwitchToThreadPool();
            
            const int sizeBuffer = 4096;
            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, sizeBuffer, true);
            
            var buffer = new byte[sizeBuffer];  
            var result = new StringBuilder();

            while (true)
            {  
                int sizeRead = await stream.ReadAsync(buffer, 0, sizeBuffer);
                if (sizeRead == 0)
                    return result.ToString();
                
                result.Append(Encoding.UTF8.GetString(buffer, 0, sizeRead));
            }
        }

        public static async UniTask<byte[]> ReadFileAsBytes(string path) {
            await using var _ = UniTask.ReturnToCurrentSynchronizationContext();
            await UniTask.SwitchToThreadPool();

            const int sizeBuffer = 4096;
            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, sizeBuffer, true);
            
            int sizeReadMax = (int)stream.Length;
            int sizeReadTotal = 0;
            
            var result = new byte[sizeReadMax];
            
            while (sizeReadTotal < sizeReadMax)
            {  
                int sizeRead = await stream.ReadAsync(result, sizeReadTotal, sizeReadMax);
                if (sizeRead == 0)
                    return result;
                sizeReadTotal += sizeRead;
            }
            
            return result;
        }
    }
}