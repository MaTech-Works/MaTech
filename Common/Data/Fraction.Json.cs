// Copyright (c) 2025, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System;
using System.Collections.Generic;
using MaTech.Common.Utils;
using Newtonsoft.Json;
using UnityEngine.Assertions;

namespace MaTech.Common.Data {
    // todo: move implementation to a separate IMeta serialization module and make Json.net optional
    public class FractionJsonConverter : JsonConverter {
        public override bool CanConvert(Type objectType) => objectType == typeof(Fraction) || objectType == typeof(FractionSimple);

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
            switch (value) {
            case FractionSimple fs:
                serializer.Serialize(writer, fs.ToArray());
                break;
            case Fraction f:
                serializer.Serialize(writer, f.ToArray());
                break;
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) {
            if (reader.TokenType == JsonToken.Null) {
                return FractionOfType(objectType, FractionSimple.invalid);
            }

            var arrInt = ReadIntArrayForFraction(reader);
            if (arrInt.Count == 2) {
                var value = new FractionSimple(arrInt[0], arrInt[1]);
                return FractionOfType(objectType, value);
            }
            if (arrInt.Count == 3) {
                var value = new Fraction(arrInt[0], arrInt[1], arrInt[2]);
                return FractionOfType(objectType, value);
            }

            throw reader is IJsonLineInfo lineInfo ?
                new JsonSerializationException($"Cannot read beat value '{reader.Path}'. It needs to be an array of 2 or 3 integers.", reader.Path, lineInfo.LineNumber, lineInfo.LinePosition, null) :
                new JsonSerializationException($"Cannot read beat value '{reader.Path}'. It needs to be an array of 2 or 3 integers.");
        }

        private object FractionOfType(Type type, Fraction value) => type == typeof(Fraction) ? value : value.Improper;
        private object FractionOfType(Type type, FractionSimple value) => type == typeof(Fraction) ? (Fraction)value : value;

        public static List<int> ReadIntArrayForFraction(JsonReader reader) {
            List<int> arrInt = new List<int>(3);

            reader.AssumeToken(JsonToken.StartArray);

            reader.ReadAndAssumeToken(JsonToken.Integer);
            Assert.IsNotNull(reader.Value);
            arrInt.Add((int)(long)reader.Value);

            reader.ReadAndAssumeToken(JsonToken.Integer);
            Assert.IsNotNull(reader.Value);
            arrInt.Add((int)(long)reader.Value);

            reader.ReadAndAssertSuccess();

            if (reader.TokenType == JsonToken.Integer) {
                Assert.IsNotNull(reader.Value);
                arrInt.Add((int)(long)reader.Value);
                reader.ReadAndAssumeToken(JsonToken.EndArray);
            } else {
                reader.AssumeToken(JsonToken.EndArray);
            }

            return arrInt;
        }
    }
}