using System;
using System.Collections;
using System.Collections.Generic;
using LightJson;
using BGC.Users;

namespace BGC.Scripting
{
    public class GetUserListFunction : IValueGetter
    {
        private readonly IValueGetter keyArg;
        private readonly IValueGetter defaultArg;
        private readonly Type getType;

        public static GetUserListFunction Create(
            IValueGetter[] args,
            Type itemType,
            Token source)
        {
            if (args.Length == 1)
            {
                return new GetUserListFunction(
                    keyArg: args[0],
                    defaultArg: null,
                    itemType: itemType,
                    source: source);
            }
            else if (args.Length == 2)
            {
                return new GetUserListFunction(
                    keyArg: args[0],
                    defaultArg: args[1],
                    itemType: itemType,
                    source: source);
            }
            else
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Expected 1 or 2 Arguments to User.GetList, found: {args.Length}");
            }
        }

        public GetUserListFunction(
            IValueGetter keyArg,
            IValueGetter defaultArg,
            Type itemType,
            Token source)
        {
            this.keyArg = keyArg;
            this.defaultArg = defaultArg;
            getType = typeof(List<>).MakeGenericType(itemType);

            if (keyArg.GetValueType() != typeof(string))
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Key argument of User.GetList must be a string: type {keyArg.GetValueType().Name}");
            }

            if (defaultArg != null && !getType.AssignableFromType(defaultArg.GetValueType()))
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Default Value argument of User.GetList must be of type List: type {defaultArg.GetValueType().Name}");
            }
        }

        public T GetAs<T>(RuntimeContext context)
        {
            Type returnType = typeof(T);

            if (!returnType.AssignableFromType(getType))
            {
                throw new ScriptRuntimeException($"Tried to retrieve result of User.GetList as type {returnType.Name}");
            }

            string key = keyArg.GetAs<string>(context);

            if (PlayerData.HasKey(key))
            {
                return (T)DeserializeList(PlayerData.GetJsonValue(key));
            }
            else
            {
                return (T)(defaultArg?.GetAs<object>(context) ?? Activator.CreateInstance(getType));
            }
        }

        public static IList DeserializeList(JsonObject listData)
        {
            if (!listData.ContainsKey("ItemType") || !listData.ContainsKey("Items"))
            {
                throw new ScriptRuntimeException(
                    $"List Serialization missing elements: {listData.ToString()}");
            }

            string itemType = listData["ItemType"];
            JsonArray items = listData["Items"].AsJsonArray;

            Type newContainerType = typeof(List<>).MakeGenericType(GetItemType(itemType));

            IList list = (IList)Activator.CreateInstance(newContainerType);

            switch (itemType)
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
                        $"Container item type not recognized by deserialization: {itemType}");
            }

            return list;
        }

        public static JsonObject SerializeList(IList list)
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

            return containerSerialization;
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

        private static string GetItemType(object container)
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

        public Type GetValueType() => getType;
    }
}
