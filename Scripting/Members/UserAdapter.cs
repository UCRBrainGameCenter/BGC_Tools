using System;
using System.Collections;
using System.Collections.Generic;
using LightJson;
using BGC.Users;

namespace BGC.Scripting.Members
{
    public class UserAdapter
    {
        public static Reports.ReportElement currentReport = null;

        public static string GetUserName() => PlayerData.UserName;
        public static bool HasData(string key) => PlayerData.HasKey(key);
        public static void ClearData(string key) => PlayerData.RemoveKey(key);

        public static bool GetBool(string key) => PlayerData.GetBool(key);
        public static bool GetBool(string key, bool defaultValue) => PlayerData.GetBool(key, defaultValue);
        public static int GetInt(string key) => PlayerData.GetInt(key);
        public static int GetInt(string key, int defaultValue) => PlayerData.GetInt(key, defaultValue);
        public static string GetString(string key) => PlayerData.GetString(key);
        public static string GetString(string key, string defaultValue) => PlayerData.GetString(key, defaultValue);
        public static double GetDouble(string key) => PlayerData.GetDouble(key);
        public static double GetDouble(string key, double defaultValue) => PlayerData.GetDouble(key, defaultValue);

        public static List<T> GetList<T>(string key) => GetList<T>(key, null);

        public static List<T> GetList<T>(string key, List<T> defaultValue)
        {
            if (!PlayerData.HasKey(key))
            {
                return defaultValue;
            }

            JsonValue listDataValue = PlayerData.GetJsonValue(key);

            if (!listDataValue.IsJsonObject)
            {
                return defaultValue;
            }

            JsonObject listData = listDataValue.AsJsonObject;

            if (!listData.ContainsKey("ItemType") || !listData.ContainsKey("Items"))
            {
                throw new ScriptRuntimeException(
                    $"List Serialization missing elements: {listData}");
            }

            string itemTypeName = listData["ItemType"];
            JsonArray items = listData["Items"].AsJsonArray;

            Type itemType = GetItemType(itemTypeName);

            if (typeof(T) != itemType)
            {
                throw new ScriptRuntimeException(
                    $"List Serialization type mismatch. Requested {typeof(T).Name} but received {itemType.Name}");
            }

            IList list = new List<T>();

            switch (itemTypeName)
            {
                case "double":
                    foreach (JsonValue item in items)
                    {
                        list.Add(item.AsNumber);
                    }
                    break;

                case "int":
                    foreach (JsonValue item in items)
                    {
                        list.Add(item.AsInteger);
                    }
                    break;

                case "bool":
                    foreach (JsonValue item in items)
                    {
                        list.Add(item.AsBoolean);
                    }
                    break;

                case "string":
                    foreach (JsonValue item in items)
                    {
                        list.Add(item.AsString);
                    }
                    break;

                default:
                    throw new ScriptRuntimeException(
                        $"Container item type not recognized by deserialization: {itemTypeName}");
            }

            return (List<T>)list;
        }

        public static void SetBool(string key, bool value) => PlayerData.SetBool(key, value);
        public static void SetInt(string key, int value) => PlayerData.SetInt(key, value);
        public static void SetString(string key, string value) => PlayerData.SetString(key, value);
        public static void SetDouble(string key, double value) => PlayerData.SetDouble(key, value);
        public static void SetList(string key, IList list)
        {
            JsonArray items = new JsonArray();
            foreach (object item in list)
            {
                items.Add(SerializeItem(item));
            }

            JsonObject containerSerialization = new JsonObject()
            {
                { "ItemType", GetItemType(list) },
                { "Items", items }
            };

            PlayerData.SetJsonValue(key, containerSerialization);
        }

        private static JsonValue SerializeItem(object item)
        {
            switch (item)
            {
                case double value: return new JsonValue(value);
                case int value: return new JsonValue(value);
                case string value: return new JsonValue(value);
                case bool value: return new JsonValue(value);

                default:
                    throw new ScriptRuntimeException(
                        $"Unable to serialize item of type: {item.GetType().Name}");
            }
        }

        private static string GetItemType(IList container)
        {
            if (!container.GetType().IsGenericType)
            {
                throw new ScriptRuntimeException(
                    $"Container was not generic type: {container.GetType().Name}");
            }

            Type itemType = container.GetType().GetGenericArguments()[0];

            if (itemType == typeof(double))
            {
                return "double";
            }
            else if (itemType == typeof(int))
            {
                return "int";
            }
            else if (itemType == typeof(bool))
            {
                return "bool";
            }
            else if (itemType == typeof(string))
            {
                return "string";
            }

            throw new ScriptRuntimeException(
                $"Container item type not supported by serialization: {itemType.Name}");
        }

        private static Type GetItemType(string itemType)
        {
            switch (itemType)
            {
                case "double": return typeof(double);
                case "int": return typeof(int);
                case "bool": return typeof(bool);
                case "string": return typeof(string);

                default:
                    throw new ScriptRuntimeException(
                        $"Container item type not recognized by deserialization: {itemType}");
            }
        }

        public static void Save() => PlayerData.Save();

        public static void AddToReport(string key, string value) => currentReport?.AddData(key, value);

    }
}
