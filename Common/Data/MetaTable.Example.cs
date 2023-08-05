// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using UnityEditor;

namespace MaTech.Common.Data {
    #if UNITY_EDITOR || DEVELOPMENT_BUILD
    internal class MetaTableExample {
        private enum Foo { Bar }
        private enum Lui { Cat }

        // TODO: make this a unit test
        #if MATECH_TEST && UNITY_EDITOR // compile check only if not for tests
        [InitializeOnLoadMethod]
        #endif
        public static void Example() {
            Foo bar0 = Foo.Bar;
            var bar1 = new EnumEx<Foo>(bar0); // prints as "Bar", underlying enum value is 0 (Foo.Bar)
            var bar2 = new EnumEx<Foo>("Bar"); // prints as "Bar", underlying enum value is 0 (Foo.Bar)
            var baz = new EnumEx<Foo>("Baz"); // prints as "Baz", underlying enum value is 1
            Foo qux = new EnumEx<Foo>("Qux"); // prints as 2; new EnumEx<Foo>(qux) prints as "Qux"

            { // Basics
                var table = new MetaTable<Foo>();
                
                table.Set(bar0, 765);
                table.Set(bar1, 5.73f);
                table.Select(bar2).Set("test");

                table.Has<int>(Foo.Bar); // --> true
                table.Has<float>(Foo.Bar); // --> true
                table.Has<string>(Foo.Bar); // --> true
                table.Has<double>(Foo.Bar); // --> false
                table.Select(Foo.Bar).Has<int>(); // --> true
                table.Select(Foo.Bar).Has<float>(); // --> true
                table.Select(Foo.Bar).Has<string>(); // --> true
                table.Select(Foo.Bar).Has<double>(); // --> false

                table.Get<int>(Foo.Bar); // --> 765
                table.Get<float>(Foo.Bar); // --> 5.73f
                table.Get<string>(Foo.Bar); // --> "test"
                table.Get<double>(Foo.Bar); // --> 0.0 (default of double)
                table.Get<object>(Foo.Bar); // --> null
                table.GetNullable<double>(Foo.Bar); // --> null
            }

            { // GetOrSet
                var table = new MetaTable<Foo>();
                table.GetOrSet(baz, 1); // --> 1
                table.GetOrSet(baz, 2); // --> 1
                table.Get<int>(baz); // --> 1
                table.Remove<int>(baz);
                table.GetOrSet(baz, 3); // --> 3
                table.Get<int>(baz); // --> 3
                table.Set(baz, 4); // --> 4
                table.Get<int>(baz); // --> 4
            }

            { // Collect
                var table = new MetaTable<Foo>();
                table.Set(bar0, 0);
                table.Set(bar1, "test bar1");
                table.Set(bar2, "test bar2");
                table.Set(baz, baz);
                table.Set(qux, "test qux");
                    
                var list = new List<string>();
                table.Collect(list); // --> ["test bar2", "test qux"]
            }

            { // Selector Methods
                var table = new MetaTable<Foo>();
                table.Select(qux).Select(Lui.Cat).EnsureContextTable();
                table.Select(qux).Select(Lui.Cat).IsValid(); // --> true
                table.Select(qux).Select(Foo.Bar).Set(Lui.Cat);
                table.Select(qux).Select(Foo.Bar).Get<Lui>(); // --> Cat
            }
            { // Without Selector Methods
                var table = new MetaTable<Foo>();
                table.Set(qux, new MetaTable<Lui>());
                table.Has<MetaTable<Lui>>(qux); // --> true
                table.Set(qux, new MetaTable<Foo>()).Set(Foo.Bar, Lui.Cat);
                table.Get<MetaTable<Foo>>(qux)!.Get<Lui>(Foo.Bar); // --> Cat
            }
            { // Selector Objects
                var table = new MetaTable<Foo>();
                var selector1 = table.Select(qux).Select(Lui.Cat);
                var selector2 = table.Select(qux).Select(Foo.Bar);
                selector1.EnsureContextTable();
                selector1.IsValid(); // --> true
                selector2.Set(Lui.Cat);
                selector2.Get<Lui>(); // --> Cat
            }
        }
    }
    #endif
}