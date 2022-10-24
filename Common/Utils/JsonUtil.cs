// Copyright (c) 2022, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Newtonsoft.Json;

namespace MaTech.Common.Utils {
    public static class JsonUtil {
        public static void AssumeToken(this JsonReader reader, JsonToken token) {
            if (reader.NonCommentToken() != token)
                throw new JsonReaderException($"Unexpected token type {reader.TokenType}, expected {token}.");
            // throw JsonReaderException.Create(reader, $"Unexpected token type {reader.TokenType}, expected {token}.");
        }

        public static void ReadAndAssertSuccess(this JsonReader reader) {
            if (!reader.Read()) throw new JsonSerializationException("Unexpected end when reading JSON.");
            // throw JsonSerializationException.Create(reader, "Unexpected end when reading JSON.");
        }

        public static void ReadAndAssumeToken(this JsonReader reader, JsonToken token) {
            reader.ReadAndAssertSuccess();
            reader.AssumeToken(token);
        }

        public static void AssumeTokenAndRead(this JsonReader reader, JsonToken token) {
            reader.AssumeToken(token);
            reader.ReadAndAssertSuccess();
        }

        public static JsonToken NonCommentToken(this JsonReader reader) {
            while (reader.ReadOnToken(JsonToken.None, JsonToken.Comment)) { }
            return reader.TokenType;
        }

        public static bool ReadOnToken(this JsonReader reader, JsonToken token) {
            if (reader.TokenType == token) {
                reader.ReadAndAssertSuccess();
                return true;
            }
            return false;
        }

        public static bool ReadOnToken(this JsonReader reader, params JsonToken[] tokens) {
            foreach (var token in tokens) {
                if (reader.ReadOnToken(token)) {
                    return true;
                }
            }
            return false;
        }
    }
}