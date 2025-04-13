// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#if MATECH_USE_NEWTONSOFT_JSON

using System;
using Newtonsoft.Json;

namespace MaTech.Common.Data {
    public partial struct Variant {
        public class JsonConverter : Newtonsoft.Json.JsonConverter {
            public override bool CanConvert(Type objectType) => objectType == typeof(Variant);

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
                var variant = (Variant)value;
                switch (variant.Type) {
                case VariantType.None:
                    writer.WriteNull();
                    break;
                case VariantType.Bool:
                    writer.WriteValue(variant.Bool);
                    break;
                case VariantType.Int:
                    writer.WriteValue(variant.Int);
                    break;
                case VariantType.Float:
                    writer.WriteValue(variant.Float);
                    break;
                case VariantType.Double:
                    writer.WriteValue(variant.Double);
                    break;
                case VariantType.Fraction:
                    serializer.Serialize(writer, variant.Fraction);
                    break;
                case VariantType.FractionSimple:
                    serializer.Serialize(writer, variant.FractionSimple);
                    break;
                case VariantType.String:
                    writer.WriteValue(variant.String);
                    break;
                case VariantType.Object:
                    writer.WriteValue(variant.Object);
                    break;
                default:
                    writer.WriteNull();
                    break;
                }
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
                var variant = (Variant?)existingValue ?? Variant.None;
                switch (reader.TokenType) {
                case JsonToken.Boolean:
                    variant = serializer.Deserialize<bool>(reader);
                    break;
                case JsonToken.Integer:
                    variant = serializer.Deserialize<int>(reader);
                    break;
                case JsonToken.Float:
                    variant = serializer.Deserialize<double>(reader);
                    break; // unfortunately we cannot distinguish between f32 & f64
                case JsonToken.String:
                    variant = serializer.Deserialize<string>(reader);
                    break;

                case JsonToken.Raw:
                case JsonToken.Date:
                case JsonToken.Bytes:
                    variant = Variant.From(serializer.Deserialize(reader));
                    break;

                case JsonToken.StartArray:
                    var arr = FractionJsonConverter.ReadIntArrayForFraction(reader);
                    switch (arr.Count) {
                    case 2:
                        variant = new FractionSimple(arr[0], arr[1]);
                        break;
                    case 3:
                        variant = new FractionMixed(arr[0], arr[1], arr[2]);
                        break;
                    default:
                        variant = Variant.None;
                        break;
                    }
                    break;
                }
                return variant;
            }
        }
    }
}

#endif // MATECH_USE_NEWTONSOFT_JSON