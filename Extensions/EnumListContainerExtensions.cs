﻿using System.Collections.Generic;
using LightJson;
using BGC.DataStructures;
using BGC.Utility;

namespace BGC.Extensions
{
    public static class EnumListContainerExtensions
    {
        public static JsonArray EnumListContainerListToJsonArray<T>(this List<EnumListContainer<T>> enumList)
        {
            return enumList.ConvertToJsonArray((EnumListContainer<T> val) =>
            {
                return val.JsonArray;
            });
        }

        public static List<EnumListContainer<T>> JsonArrayToEnumListContainerList<T>(this JsonArray arr, EnumSerialization serialization)
        {
            return arr.JsonArrayToList((JsonValue val) =>
            {
                return new EnumListContainer<T>(val, serialization);
            });
        }
    }
}
