using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LightJson;
using BGC.Scripting;
using BGC.Utility;
using Debug = UnityEngine.Debug;

namespace BGC.Parameters
{
#pragma warning disable IDE0019 // Use pattern matching

    public interface IPropertyGroup
    {
        /// <summary>
        /// Version of the property group. Used in serialization and deserialization of __STATE__ property.
        /// </summary>
        int __VERSION__ { get; }
        
        IPropertyGroup GetParent();
        void SetParent(IPropertyGroup parent);

        /// <summary>Serializes the property group to JSON</summary>
        /// <param name="includeState">
        /// If TRUE, then a __STATE__ object will be included at the root level of the serialized JSON.
        /// This object will contain state data from all properties decorated with <see cref="SerializableStateAttribute"/>
        /// </param>
        JsonObject Serialize(bool includeState = false);
        void Deserialize(JsonObject data);

        /// <summary>
        /// Attempts to upgrade a serialized property group to a current schema. No op if no upgrades available.
        /// </summary>
        /// <returns>TRUE if an upgrade was successful OR nothing was done, FALSE otherwise.</returns>
        bool TryUpgradeVersion(JsonObject serializedData);
    }

    public static class PropertyGroupExtensions
    {
        private const string StateKey = "__STATE__";
        private const string VersionKey = nameof(IPropertyGroup.__VERSION__);

        public static IEnumerable<PropertyInfo> GetPropertyGroupListProperties(this IPropertyGroup container)
        {
            foreach (PropertyInfo property in container.GetType().GetProperties())
            {
                if (typeof(IList).IsAssignableFrom(property.PropertyType) &&
                    property.GetCustomAttribute<PropertyGroupListAttribute>() is PropertyGroupListAttribute propertyGroupList &&
                    propertyGroupList.autoSerialize)
                {
                    yield return property;
                }
            }
        }

        public static IEnumerable<PropertyInfo> GetPropertyGroupProperties(this IPropertyGroup container)
        {
            foreach (PropertyInfo property in container.GetType().GetProperties())
            {
                if (!typeof(IPropertyGroup).IsAssignableFrom(property.PropertyType))
                {
                    continue;
                }

                yield return property;
            }
        }
        
        public static IEnumerable<IPropertyGroup> GetAllPropertyGroups(this IPropertyGroup container)
        {
            foreach (PropertyInfo property in container.GetType().GetProperties(
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                //The first check prevents the evaluation of all properties
                if (typeof(IPropertyGroup).IsAssignableFrom(property.PropertyType) &&
                    property.GetValue(container) is IPropertyGroup propertyGroup)
                {
                    //Standard PropertyGroup
                    yield return propertyGroup;
                }
                else if (typeof(IList).IsAssignableFrom(property.PropertyType) &&
                    property.GetValue(container) is IList propertyGroupList &&
                    property.GetCustomAttribute<PropertyGroupListAttribute>() != null)
                {
                    //PropertyGroupList
                    foreach (IPropertyGroup propertyGroupListItem in propertyGroupList)
                    {
                        yield return propertyGroupListItem;
                    }
                }
            }
        }

        public static IEnumerable<IPropertyGroup> GetAllPropertyGroups(this IPropertyGroup container, Func<PropertyInfo, bool> predicate)
        {
            foreach (PropertyInfo property in container.GetType().GetProperties(
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                //The first check prevents the evaluation of all properties
                if (typeof(IPropertyGroup).IsAssignableFrom(property.PropertyType) &&
                    property.GetValue(container) is IPropertyGroup propertyGroup)
                {
                    //Standard PropertyGroup
                    if (predicate(property))
                    {
                        yield return propertyGroup;
                    }
                }
                else if (typeof(IList).IsAssignableFrom(property.PropertyType) &&
                    property.GetValue(container) is IList propertyGroupList &&
                    property.GetCustomAttribute<PropertyGroupListAttribute>() != null)
                {
                    if (predicate(property))
                    {
                        //PropertyGroupList
                        foreach (IPropertyGroup propertyGroupListItem in propertyGroupList)
                        {
                            yield return propertyGroupListItem;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates a __STATE__ JSON object containing data for fields decorated with
        /// <see cref="SerializableStateAttribute"/>
        /// </summary>
        public static JsonObject Internal_GetSerializedStateData(this IPropertyGroup propertyGroup)
        {
            JsonObject state = new JsonObject();

            IEnumerable<MemberInfo> properties = propertyGroup.GetSerializableStateProperties();
            foreach (MemberInfo statePropInfo in properties)
            {
                SerializableStateAttribute parsed = statePropInfo.GetCustomAttribute<SerializableStateAttribute>();
                
                SerializeValueToJsonObject(
                    propertyGroup,
                    statePropInfo,
                    state,
                    parsed.FieldName);
            }

            return state;
        }
        
        public static JsonObject Internal_GetSerializedData(this IPropertyGroup propertyGroup, bool includeState = false)
        {
            JsonObject propertyGroupData = new JsonObject();
            JsonObject keyData = new JsonObject();

            propertyGroupData.Add("Type", propertyGroup.GetSelectionSerializationName());
            propertyGroupData.Add(VersionKey, propertyGroup.__VERSION__);

            //Grab the key fields
            foreach (PropertyInfo property in propertyGroup.GetKeyProperties())
            {
                string keyValue = property.GetValue(propertyGroup) as string;

                if (!string.IsNullOrEmpty(keyValue))
                {
                    keyData.Add(
                        key: property.GetCustomAttribute<DisplayInputFieldKeyAttribute>().fieldName,
                        value: keyValue);
                }
            }

            //Grab the value fields
            foreach (PropertyInfo property in propertyGroup.GetInputFieldProperties())
            {
                DisplayInputFieldAttribute att = property.GetCustomAttribute<DisplayInputFieldAttribute>();
                if (keyData.ContainsKey(att.fieldName))
                {
                    //Skip values being serialized by key
                    continue;
                }

                SerializeValueToJsonObject(
                    propertyGroup: propertyGroup,
                    memberInfo: property,
                    data: propertyGroupData,
                    fieldName: att.fieldName);
            }

            //Serialize Item Title
            foreach (PropertyInfo property in propertyGroup.GetTitleFieldProperty())
            {
                propertyGroupData.Add(
                    key: property.GetItemTitleSerializationName(),
                    value: property.GetValue(propertyGroup) as string);
            }

            //Grab the outputkey fields
            foreach (PropertyInfo property in propertyGroup.GetOutputKeyProperties())
            {
                string keyValue = property.GetValue(propertyGroup) as string;

                if (!string.IsNullOrEmpty(keyValue))
                {
                    propertyGroupData.Add(
                        key: property.GetCustomAttribute<DisplayOutputFieldKeyAttribute>().fieldName,
                        value: keyValue);
                }
            }

            foreach (PropertyInfo innerGroup in propertyGroup.GetPropertyGroupProperties())
            {
                if (innerGroup.GetValue(propertyGroup) is IPropertyGroup innerPropertyGroup)
                {
                    propertyGroupData.Add(
                        key: innerGroup.GetGroupSerializationName(),
                        value: innerPropertyGroup.Serialize(includeState));
                }
            }

            foreach (PropertyInfo innerList in propertyGroup.GetPropertyGroupListProperties())
            {
                if (innerList.GetValue(propertyGroup) is IList innerPropertyList)
                {
                    JsonArray propertyListValues = new JsonArray();

                    foreach (IPropertyGroup listElement in innerPropertyList)
                    {
                        propertyListValues.Add(listElement.Serialize(includeState));
                    }

                    if (propertyListValues.Count > 0)
                    {
                        propertyGroupData.Add(
                            key: innerList.GetGroupListSerializationName(),
                            value: propertyListValues);
                    }
                }
            }

            if (includeState)
            {
                propertyGroupData.Add(StateKey, propertyGroup.Internal_GetSerializedStateData());
            }

            if (keyData.Count > 0)
            {
                propertyGroupData.Add("Keys", keyData);
            }

            return propertyGroupData;
        }

        /// <summary>
        /// Parses a reflection-based property into a JSON value and saves it into the provided JSON object.
        /// </summary>
        private static void SerializeValueToJsonObject(
            IPropertyGroup propertyGroup,
            MemberInfo memberInfo,
            JsonObject data,
            string fieldName)
        {
            if (memberInfo is PropertyInfo property)
            {
                string propertyTypeName = property.PropertyType.Name;
                switch (propertyTypeName)
                {
                    case "Single":
                        data.Add(fieldName, Convert.ToSingle(property.GetValue(propertyGroup)));
                        break;

                    case "Double":
                        data.Add(fieldName, Convert.ToDouble(property.GetValue(propertyGroup)));
                        break;

                    case "Int32":
                        data.Add(fieldName, Convert.ToInt32(property.GetValue(propertyGroup)));
                        break;

                    case "Int64":
                        data.Add(fieldName, Convert.ToInt64(property.GetValue(propertyGroup)));
                        break;

                    case "String":
                        data.Add(fieldName, property.GetValue(propertyGroup) as string);
                        break;

                    case "Boolean":
                        data.Add(fieldName, Convert.ToBoolean(property.GetValue(propertyGroup)));
                        break;

                    default:
                        if (property.PropertyType.IsEnum)
                        {
                            data.Add(fieldName, property.GetValue(propertyGroup).ToString());
                            break;
                        }
                        if(typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                        {
                            // enumerable
                            SerializeEnumerable(
                                propertyGroup,
                                memberInfo,
                                data,
                                fieldName);
                            
                            break;
                        }
                        
                        Debug.LogError($"Unsupported datatype ({propertyTypeName}) for variable {property.Name}");
                        break;
                }
            }
            else if (memberInfo is FieldInfo field)
            {
                string fieldType = field.FieldType.Name;
                switch (fieldType)
                {
                    case "Single":
                        data.Add(fieldName, Convert.ToSingle(field.GetValue(propertyGroup)));
                        break;

                    case "Double":
                        data.Add(fieldName, Convert.ToDouble(field.GetValue(propertyGroup)));
                        break;

                    case "Int32":
                        data.Add(fieldName, Convert.ToInt32(field.GetValue(propertyGroup)));
                        break;

                    case "Int64":
                        data.Add(fieldName, Convert.ToInt64(field.GetValue(propertyGroup)));
                        break;

                    case "String":
                        data.Add(fieldName, field.GetValue(propertyGroup) as string);
                        break;

                    case "Boolean":
                        data.Add(fieldName, Convert.ToBoolean(field.GetValue(propertyGroup)));
                        break;

                    default:
                        if (field.FieldType.IsEnum)
                        {
                            data.Add(fieldName, field.GetValue(propertyGroup).ToString());
                            break;
                        }
                        if(typeof(IEnumerable).IsAssignableFrom(field.FieldType))
                        {
                            // enumerable
                            SerializeEnumerable(
                                propertyGroup,
                                memberInfo,
                                data,
                                fieldName);
                            
                            break;
                        }
                        
                        Debug.LogError($"Unsupported datatype ({fieldType}) for variable {field.Name}");
                        break;
                }
            }
        }
        
        /// <summary>
        /// Parses a reflection-based member info into a JSON value and saves it into the provided JSON object.
        /// </summary>
        private static void DeserializeValueFromJson(
            IPropertyGroup propertyGroup,
            MemberInfo memberInfo,
            JsonObject serializedData,
            string fieldName)
        {
            if (memberInfo is PropertyInfo property)
            {
                string propertyInfoName = property.PropertyType.Name;
                switch (propertyInfoName)
                {
                    case "Single":
                        property.SetValue(propertyGroup, (float)serializedData[fieldName].AsNumber);
                        break;

                    case "Double":
                        property.SetValue(propertyGroup, serializedData[fieldName].AsNumber);
                        break;

                    case "Int32":
                        property.SetValue(propertyGroup, serializedData[fieldName].AsInteger);
                        break;

                    case "Int64":
                        property.SetValue(propertyGroup, serializedData[fieldName].AsInteger);
                        break;

                    case "String":
                        property.SetValue(propertyGroup, serializedData[fieldName].AsString);
                        break;

                    case "Boolean":
                        property.SetValue(propertyGroup, serializedData[fieldName].AsBoolean);
                        break;

                    default:
                        if (property.PropertyType.IsEnum)
                        {
                            property.SetValue(propertyGroup,
                                Enum.Parse(property.PropertyType, serializedData[fieldName].AsString));
                            break;
                        }
                        if(typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                        {
                            DeserializeEnumerable(
                                propertyGroup,
                                memberInfo,
                                serializedData,
                                fieldName);

                            break;
                        }
                        Debug.LogError($"Unsupported datatype ({propertyInfoName}) for variable {property.Name}");
                        break;
                }
            }
            else if (memberInfo is FieldInfo field)
            {
                string fieldInfoName = field.FieldType.Name;
                switch (fieldInfoName)
                {
                    case "Single":
                        field.SetValue(propertyGroup, (float)serializedData[fieldName].AsNumber);
                        break;

                    case "Double":
                        field.SetValue(propertyGroup, serializedData[fieldName].AsNumber);
                        break;

                    case "Int32":
                        field.SetValue(propertyGroup, serializedData[fieldName].AsInteger);
                        break;

                    case "Int64":
                        field.SetValue(propertyGroup, serializedData[fieldName].AsInteger);
                        break;

                    case "String":
                        field.SetValue(propertyGroup, serializedData[fieldName].AsString);
                        break;

                    case "Boolean":
                        field.SetValue(propertyGroup, serializedData[fieldName].AsBoolean);
                        break;

                    default:
                        if (field.FieldType.IsEnum)
                        {
                            field.SetValue(propertyGroup,
                                Enum.Parse(field.FieldType, serializedData[fieldName].AsString));
                            break;
                        }
                        if(typeof(IEnumerable).IsAssignableFrom(field.FieldType))
                        {
                            DeserializeEnumerable(
                                propertyGroup,
                                memberInfo,
                                serializedData,
                                fieldName);

                            break;
                        }
                        
                        Debug.LogError($"Unsupported datatype ({fieldInfoName}) for variable {field.Name}");
                        break;
                }
            }
        }
        
        private static void SerializeEnumerable(
            IPropertyGroup propertyGroup,
            MemberInfo memberInfo,
            JsonObject serializedData,
            string fieldName)
        {
            if (memberInfo is PropertyInfo property)
            {
                Type propertyType = property.PropertyType;
                Type elementType = propertyType.GetGenericArguments()[0];
                
                switch (elementType.Name)
                {
                    case "Single":
                        IEnumerable<double> parsedSingle = (IEnumerable<double>) property.GetValue(propertyGroup);
                        serializedData.Add(
                            fieldName,
                            new JsonArray(parsedSingle.Select(x => new JsonValue(x))));
                        break;

                    case "Double":
                        IEnumerable<double> parsedDouble = (IEnumerable<double>) property.GetValue(propertyGroup);
                        serializedData.Add(
                            fieldName,
                            new JsonArray(parsedDouble.Select(x => new JsonValue(x))));
                        break;

                    case "Int32":
                        IEnumerable<int> parsedInt32 = (IEnumerable<int>) property.GetValue(propertyGroup);
                        serializedData.Add(
                            fieldName,
                            new JsonArray(parsedInt32.Select(x => new JsonValue(x))));
                        break;

                    case "Int64":
                        IEnumerable<int> parsedInt64 = (IEnumerable<int>) property.GetValue(propertyGroup);
                        serializedData.Add(
                            fieldName,
                            new JsonArray(parsedInt64.Select(x => new JsonValue(x))));
                        break;

                    case "String":
                        IEnumerable<string> parsedString = (IEnumerable<string>) property.GetValue(propertyGroup);
                        serializedData.Add(
                            fieldName,
                            new JsonArray(parsedString.Select(x => new JsonValue(x))));
                        break;

                    case "Boolean":
                        IEnumerable<bool> parsedBool = (IEnumerable<bool>) property.GetValue(propertyGroup);
                        serializedData.Add(
                            fieldName,
                            new JsonArray(parsedBool.Select(x => new JsonValue(x))));
                        break; 
                    default:
                        if (propertyType.IsEnum)
                        {
                            IEnumerable<string> parsedStringEnum = (IEnumerable<string>) property.GetValue(propertyGroup);
                            serializedData.Add(
                                fieldName,
                                new JsonArray(parsedStringEnum.Select(x => new JsonValue(x))));
                            
                            break;
                        }
                        
                        Debug.LogError($"Unsupported datatype ({propertyType}) for variable {property.Name}");
                        break;
                }
            }
            if (memberInfo is FieldInfo field)
            {
                Type fieldType = field.FieldType;
                Type elementType = fieldType.GetGenericArguments()[0];
                
                switch (elementType.Name)
                {
                    case "Single":
                        IEnumerable<double> parsedSingle = (IEnumerable<double>) field.GetValue(propertyGroup);
                        serializedData.Add(
                            fieldName,
                            new JsonArray(parsedSingle.Select(x => new JsonValue(x))));
                        break;

                    case "Double":
                        IEnumerable<double> parsedDouble = (IEnumerable<double>) field.GetValue(propertyGroup);
                        serializedData.Add(
                            fieldName,
                            new JsonArray(parsedDouble.Select(x => new JsonValue(x))));
                        break;

                    case "Int32":
                        IEnumerable<int> parsedInt32 = (IEnumerable<int>) field.GetValue(propertyGroup);
                        serializedData.Add(
                            fieldName,
                            new JsonArray(parsedInt32.Select(x => new JsonValue(x))));
                        break;

                    case "Int64":
                        IEnumerable<int> parsedInt64 = (IEnumerable<int>) field.GetValue(propertyGroup);
                        serializedData.Add(
                            fieldName,
                            new JsonArray(parsedInt64.Select(x => new JsonValue(x))));
                        break;

                    case "String":
                        IEnumerable<string> parsedString = (IEnumerable<string>) field.GetValue(propertyGroup);
                        serializedData.Add(
                            fieldName,
                            new JsonArray(parsedString.Select(x => new JsonValue(x))));
                        break;

                    case "Boolean":
                        IEnumerable<bool> parsedBool = (IEnumerable<bool>) field.GetValue(propertyGroup);
                        serializedData.Add(
                            fieldName,
                            new JsonArray(parsedBool.Select(x => new JsonValue(x))));
                        break; 
                    default:
                        if (fieldType.IsEnum)
                        {
                            IEnumerable<string> parsedStringEnum = (IEnumerable<string>) field.GetValue(propertyGroup);
                            serializedData.Add(
                                fieldName,
                                new JsonArray(parsedStringEnum.Select(x => new JsonValue(x))));
                            
                            break;
                        }
                        
                        Debug.LogError($"Unsupported datatype ({fieldType}) for variable {field.Name}");
                        break;
                }
            }
        }

        private static void DeserializeEnumerable(
            IPropertyGroup propertyGroup,
            MemberInfo memberInfo,
            JsonObject serializedData,
            string fieldName)
        {
            if (memberInfo is PropertyInfo property)
            {
                Type propertyType = property.PropertyType;
                Type elementType = propertyType.GetGenericArguments()[0];
                JsonArray serializedArray = serializedData[fieldName].AsJsonArray;

                // Create an array and populate it
                switch (elementType.Name)
                {
                    case "Single":
                    case "Double":
                        property.SetValue(
                            propertyGroup, 
                            propertyType.IsArray 
                                ? serializedArray.ToArray<double>()
                                : serializedArray.ToList<double>(), 
                            null);
                        break;

                    case "Int32":
                    case "Int64":
                        property.SetValue(
                            propertyGroup,
                            propertyType.IsArray 
                                ? serializedArray.ToArray<int>()
                                : serializedArray.ToList<int>(), 
                            null);
                        break;

                    case "String":
                        property.SetValue(
                            propertyGroup,
                            propertyType.IsArray
                                ? serializedArray.ToArray<string>()
                                : serializedArray.ToList<string>(), 
                            null);
                        break;

                    case "Boolean":
                        property.SetValue(
                            propertyGroup, 
                            propertyType.IsArray
                                ? serializedArray.ToArray<bool>()
                                : serializedArray.ToList<bool>(), 
                            null);
                        break;
                    default:
                        if (elementType.IsEnum)
                        {
                            property.SetValue(
                                propertyGroup,
                                propertyType.IsArray
                                    ? serializedArray.ToArray<string>()
                                    : serializedArray.ToList<string>(), 
                                null);
                            break;
                        }
                    
                        Debug.LogError($"Unsupported datatype ({elementType}) for variable {fieldName}");
                        break;
                }
            }
            if (memberInfo is FieldInfo field)
            {
                Type fieldType = field.FieldType;
                Type elementType = fieldType.GetGenericArguments()[0];
                JsonArray serializedArray = serializedData[fieldName].AsJsonArray;

                // Create an array and populate it
                switch (elementType.Name)
                {
                    case "Single":
                    case "Double":
                        field.SetValue(
                            propertyGroup, 
                            fieldType.IsArray 
                                ? serializedArray.ToArray<double>()
                                : serializedArray.ToList<double>());
                        break;

                    case "Int32":
                    case "Int64":
                        field.SetValue(
                            propertyGroup,
                            fieldType.IsArray 
                                ? serializedArray.ToArray<int>()
                                : serializedArray.ToList<int>());
                        break;

                    case "String":
                        field.SetValue(
                            propertyGroup,
                            fieldType.IsArray
                                ? serializedArray.ToArray<string>()
                                : serializedArray.ToList<string>());
                        break;

                    case "Boolean":
                        field.SetValue(
                            propertyGroup, 
                            fieldType.IsArray
                                ? serializedArray.ToArray<bool>()
                                : serializedArray.ToList<bool>());
                        break;
                    default:
                        if (elementType.IsEnum)
                        {
                            field.SetValue(
                                propertyGroup,
                                fieldType.IsArray
                                    ? serializedArray.ToArray<string>()
                                    : serializedArray.ToList<string>());
                            break;
                        }
                    
                        Debug.LogError($"Unsupported datatype ({elementType}) for variable {fieldName}");
                        break;
                }
            }
        }
        
        public static void Internal_RawDeserialize(this IPropertyGroup container, JsonObject propertyGroupData)
        {
            // Upgrade property group version, if applicable.
            bool didUpgrade = container.TryUpgradeVersion(propertyGroupData);

            if (!didUpgrade)
            {
                throw new Exception(
                    $"Unable to upgrade property group to current version. Please update the version number.");
            }
            
            //Deserialize Property Groups
            foreach (PropertyInfo property in container.GetPropertyGroupProperties())
            {
                string propertyGroupName = property.GetGroupSerializationName();

                if (!propertyGroupData.ContainsKey(propertyGroupName))
                {
                    container.ConstructNewInternalPropertyGroup(property);
                    continue;
                }
                
                container.DeserializeInternalPropertyGroup(property, propertyGroupData[propertyGroupName]);
            }

            //Deserialize Property Group Lists
            foreach (PropertyInfo property in container.GetPropertyGroupListProperties())
            {
                string propertyGroupListName = property.GetGroupListSerializationName();

                //Instantiate Container (List)
                property.SetValue(container, Activator.CreateInstance(property.PropertyType));

                if (!propertyGroupData.ContainsKey(propertyGroupListName))
                {
                    //No list saved
                    continue;
                }

                IList groupList = property.GetValue(container) as IList;

                foreach (JsonObject listElement in propertyGroupData[propertyGroupListName].AsJsonArray)
                {
                    DeserializeListItem(
                        container: container,
                        property: property,
                        groupList: groupList,
                        listElement: listElement);
                }
            }

            //Deserialize Values
            JsonObject keyObject = null;

            if (propertyGroupData.ContainsKey("Keys"))
            {
                keyObject = propertyGroupData["Keys"].AsJsonObject;
            }

            //Deserialize Values
            foreach (PropertyInfo property in container.GetInputFieldProperties())
            {
                string fieldName = property.GetInputFieldName();
                if (propertyGroupData.ContainsKey(fieldName))
                {
                    DeserializeValueFromJson(container, property, propertyGroupData, fieldName);
                }
            }

            //Deserialize Item Title
            foreach (PropertyInfo property in container.GetTitleFieldProperty())
            {
                string serializationName = property.GetItemTitleSerializationName();
                if (propertyGroupData.ContainsKey(serializationName))
                {
                    property.SetValue(container, propertyGroupData[serializationName].AsString);
                }
                else
                {
                    Debug.LogError($"No Item Title Found with key: {serializationName}.  Creating Guid.");
                    property.SetValue(container, Guid.NewGuid().ToString());
                }
            }

            //Deserialize output keys
            foreach (PropertyInfo property in container.GetOutputKeyProperties())
            {
                string fieldName = property.GetOutputKeyFieldName();
                if (propertyGroupData.ContainsKey(fieldName))
                {
                    property.SetValue(container, propertyGroupData[fieldName].AsString);
                }
                else
                {
                    property.SetValue(container, string.Empty);
                }
            }

            // deserialize the state data.
            if (propertyGroupData.ContainsKey(StateKey))
            {
                container.Internal_DeserializeStateData(propertyGroupData[StateKey].AsJsonObject);
                // propertyGroupData.Add(StateKey, propertyGroup.Internal_GetStateData());
            }

            //Deserialize Keys
            if (keyObject != null)
            {
                foreach (PropertyInfo property in container.GetKeyProperties())
                {
                    string fieldName = property.GetKeyFieldName();
                    if (keyObject.ContainsKey(fieldName))
                    {
                        property.SetValue(container, keyObject[fieldName].AsString);
                    }
                    else
                    {
                        property.SetValue(container, string.Empty);
                    }
                }
            }
        }

        /// <summary>Deserializes the state data for a property group.</summary>
        /// <param name="propertyGroup">The property group to deserialize the state for.</param>
        /// <param name="data">The serialized data to deserialize.</param>
        public static void Internal_DeserializeStateData(this IPropertyGroup propertyGroup, JsonObject data)
        {
            JsonObject state = new JsonObject();
            foreach (MemberInfo statePropInfo in propertyGroup.GetSerializableStateProperties())
            {
                SerializableStateAttribute parsed = statePropInfo.GetCustomAttribute<SerializableStateAttribute>();

                if (data.ContainsKey(parsed.FieldName))
                {
                    DeserializeValueFromJson(
                        propertyGroup,
                        statePropInfo,
                        data,
                        parsed.FieldName);
                }
                else
                {
                    // use default value
                    state.Add(parsed.FieldName, parsed.DefaultValue);
                }
            }
        }

        public static void ConstructNewInternalPropertyGroup(
            this IPropertyGroup container,
            PropertyInfo property) => property.GetDefaultSelectionType().Build(container, property);

        public static void DeserializeInternalPropertyGroup(
            this IPropertyGroup container,
            PropertyInfo property,
            JsonObject internalPropertyGroupData)
        {
            Type matchingType = property.FindMatchingSelectionType(internalPropertyGroupData["Type"].AsString);

            if (matchingType == null)
            {
                Debug.LogError($"Missing Matching Type for construction: {internalPropertyGroupData["Type"].AsString}.  Proceeding with default");
                matchingType = property.GetDefaultSelectionType();
            }

            matchingType.Build(container, property)
                .Deserialize(internalPropertyGroupData);
        }

        public static void DeserializeListItem(
            this IPropertyGroup container,
            PropertyInfo property,
            IList groupList,
            JsonObject listElement,
            int index = -1)
        {
            Type matchingType = property.FindMatchingListType(listElement["Type"].AsString);

            if (matchingType == null)
            {
                Debug.LogError($"Requested type not recognized for list: {listElement["Type"].AsString}");
                return;
            }

            IPropertyGroup propertyGroup = matchingType.Build();

            if (index == -1)
            {
                groupList.Add(propertyGroup);
            }
            else if (index < 0 || index > groupList.Count)
            {
                Debug.LogError($"Tried to insert PropertyGroup {container.GetGroupPath()} in invalid index {index}");
                groupList.Add(propertyGroup);
            }
            else
            {
                groupList.Insert(index, propertyGroup);
            }

            propertyGroup.SetParent(container);
            propertyGroup.InitializeProperties();
            propertyGroup.Deserialize(listElement);
        }

        public static IPropertyGroup CloneOrphanListItem(
            this IPropertyGroup source,
            PropertyInfo property,
            bool includePropertyGroupState = false)
        {
            JsonObject serializedData = source.Serialize(includePropertyGroupState);

            Type matchingType = property.FindMatchingListType(serializedData["Type"].AsString);

            if (matchingType == null)
            {
                Debug.LogError($"Requested type not recognized for list: {serializedData["Type"].AsString}");
                return null;
            }

            IPropertyGroup propertyGroup = matchingType.Build();

            propertyGroup.InitializeProperties();
            propertyGroup.Deserialize(serializedData);

            return propertyGroup;
        }

        public static void Internal_Deserialize(this IPropertyGroup container, JsonObject data)
        {
            if (!data.ContainsKey("Type"))
            {
                Debug.LogError("Type data not present!");
            }
            else if (data["Type"] != container.GetSelectionSerializationName())
            {
                Debug.LogError(
                    $"Type data is not matching. " +
                    $"Expected {container.GetSelectionSerializationName()}. " +
                    $"Received {data["Type"]}.");
            }

            container.Internal_RawDeserialize(data);
        }

        public static PropertyGroupTitleAttribute GetGroupAttribute(this Type type) =>
            type.GetCustomAttributes_Deep<PropertyGroupTitleAttribute>().FirstOrDefault() ??
            throw new ArgumentException($"PropertyGroupTitleAttribute not found for Type: {type}");

        public static PropertyGroupInfoAttribute GetGroupInfoAttribute(this Type type) =>
            type.GetCustomAttributes_Deep<PropertyGroupInfoAttribute>().FirstOrDefault();

        public static PropertyChoiceTitleAttribute GetSelectionAttribute(this Type type) =>
            type.GetCustomAttribute<PropertyChoiceTitleAttribute>() ??
            throw new ArgumentException($"PropertyChoiceTitleAttribute not found for Type: {type}");

        public static string GetChoiceInfoText(this Type type) =>
            type.GetCustomAttribute<PropertyChoiceInfoAttribute>()?.text;

        public static bool IsTaskGroupTerminator(this Type type) =>
            type.GetCustomAttribute<TaskGroupTerminatorAttribute>() != null;

        public static PropertyGroupTitleAttribute GetGroupAttribute(this IPropertyGroup propertyGroup)
        {
            IPropertyGroup parent = propertyGroup.GetParent();

            if (parent == null)
            {
                Debug.LogError($"Unexpected null parent: {propertyGroup}");
                return null;
            }

            foreach (PropertyInfo propertyInfo in parent.GetPropertyGroupProperties())
            {
                IPropertyGroup childProperty = propertyInfo.GetValue(parent) as IPropertyGroup;
                if (childProperty == propertyGroup)
                {
                    if (propertyInfo.GetCustomAttribute<PropertyGroupTitleAttribute>() is
                        PropertyGroupTitleAttribute groupTitleAttribute)
                    {
                        //Return the group title attached to the property
                        return groupTitleAttribute;
                    }

                    //Otherwise, return the group property of the instance, as a fallback
                    return propertyInfo.PropertyType.GetGroupAttribute();
                }
            }

            Debug.LogError($"Failed to find PropertyGroupTitle pedantically: {propertyGroup.GetType()}");


            foreach (PropertyGroupTitleAttribute att in
                propertyGroup.GetType().GetCustomAttributes_Deep<PropertyGroupTitleAttribute>())
            {
                return att;
            }

            Debug.LogError($"PropertyGroupTitleAttribute not found for Type: {propertyGroup.GetType()}");
            return null;
        }

        public static PropertyLabelAttribute GetSelectionAttribute(this IPropertyGroup propertyGroup)
        {
            //Search for extractors first
            if (propertyGroup.GetType().GetCustomAttribute<ExtractPropertyGroupTitleAttribute>() != null)
            {
                ControlledParameterTemplate template = propertyGroup as ControlledParameterTemplate;
                if (template?.controlledParameter is IPropertyGroup adaptive)
                {
                    return adaptive.GetGroupAttribute();
                }
            }

            if (propertyGroup.GetType().GetCustomAttribute<PropertyChoiceTitleAttribute>() is
                PropertyChoiceTitleAttribute att)
            {
                return att;
            }

            Debug.LogError($"PropertyChoiceTitleAttribute not found for Type: {propertyGroup.GetType()}");
            return null;
        }

        /// <summary>
        /// First, check for local title override
        /// Then fallback on interface default title attribute
        /// </summary>
        public static PropertyGroupTitleAttribute GetGroupAttribute(this PropertyInfo propertyInfo) =>
            propertyInfo.GetCustomAttribute<PropertyGroupTitleAttribute>() ??
            propertyInfo.PropertyType.GetGroupAttribute();

        /// <summary>
        /// First, check for local info attribute override
        /// Then fallback on interface default info attribute
        /// </summary>
        public static PropertyGroupInfoAttribute GetGroupInfoAttribute(this PropertyInfo propertyInfo) =>
            propertyInfo.GetCustomAttribute<PropertyGroupInfoAttribute>() ??
            propertyInfo.PropertyType.GetGroupInfoAttribute();

        public static PropertyGroupListAttribute GetGroupListAttribute(this PropertyInfo propertyInfo) =>
            propertyInfo.GetCustomAttribute<PropertyGroupListAttribute>();

        public static string GetGroupTitle(this PropertyInfo propertyInfo) =>
            propertyInfo.GetGroupAttribute().title;

        public static string GetGroupSerializationName(this PropertyInfo propertyInfo) =>
            propertyInfo.GetGroupAttribute().serializationString;

        public static string GetGroupListSerializationName(this PropertyInfo propertyInfo) =>
            propertyInfo.GetGroupListAttribute().fieldName;

        public static string GetSelectionSerializationName(this Type type) =>
            type.GetSelectionAttribute().serializationString;

        public static string GetGroupInfoText(this PropertyInfo propertyInfo) =>
            propertyInfo.GetGroupInfoAttribute()?.text ?? null;

        public static string GetItemTitleSerializationName(this PropertyInfo propertyInfo) =>
            propertyInfo.GetCustomAttribute<PropertyGroupItemTitleAttribute>()?.serializationName ?? "";

        public static string GetSelectionTitle(this Type type) => type.GetSelectionAttribute().title;

        public static ChoiceRenderingModifier GetSelectionChoiceRenderingModifier(this Type type) =>
            type.GetSelectionAttribute().renderingModifier;

        public static string GetGroupPath(
            this IPropertyGroup propertyGroup,
            bool taskGroupOnly = false)
        {
            List<string> pathComponents = new List<string>();

            IPropertyGroup child = propertyGroup;
            IPropertyGroup parent;

            while ((parent = child.GetParent()) != null)
            {
                if (taskGroupOnly && child.GetType().IsTaskGroupTerminator())
                {
                    //End at the specified Terminating Type
                    break;
                }

                bool found = false;

                //Search properties
                foreach (PropertyInfo propertyInfo in parent.GetPropertyGroupProperties())
                {
                    if (propertyInfo.GetValue(parent) == child)
                    {
                        pathComponents.Add(propertyInfo.GetGroupSerializationName());
                        found = true;
                    }
                }

                if (!found)
                {
                    //Search lists
                    foreach (PropertyInfo propertyInfo in parent.GetPropertyGroupListProperties())
                    {
                        if (propertyInfo.GetValue(parent) is IList propertyGroupList)
                        {
                            foreach (IPropertyGroup internalPropertyGroup in propertyGroupList)
                            {
                                if (internalPropertyGroup == child)
                                {
                                    pathComponents.Add($"*{internalPropertyGroup.GetItemTitle()}");
                                    found = true;
                                }
                            }
                        }
                    }
                }

                if (!found)
                {
                    throw new Exception($"Unable to trace path for {propertyGroup}");
                }

                //Navigate up the hierarchy
                child = parent;
            }

            pathComponents.Reverse();
            return string.Join(".", pathComponents);
        }

        public static string GetParentListItemTitle(this IPropertyGroup propertyGroup)
        {
            IPropertyGroup child = propertyGroup;
            IPropertyGroup parent;

            while ((parent = child.GetParent()) != null)
            {
                bool found = false;

                //Search properties
                foreach (PropertyInfo propertyInfo in parent.GetPropertyGroupProperties())
                {
                    if (propertyInfo.GetValue(parent) == child)
                    {
                        found = true;
                    }
                }

                if (!found)
                {
                    //Search lists
                    foreach (PropertyInfo propertyInfo in parent.GetPropertyGroupListProperties())
                    {
                        if (propertyInfo.GetValue(parent) is IList propertyGroupList)
                        {
                            foreach (IPropertyGroup internalPropertyGroup in propertyGroupList)
                            {
                                if (internalPropertyGroup == child)
                                {
                                    return internalPropertyGroup.GetItemTitle();
                                }
                            }
                        }
                    }
                }

                if (!found)
                {
                    throw new Exception($"Unable to trace path for {propertyGroup}");
                }

                //Navigate up the hierarchy
                child = parent;
            }

            return null;
        }
        private static bool AllowsKeySearch(PropertyInfo propertyInfo) =>
            propertyInfo.GetCustomAttribute<KeySearchTerminatorAttribute>() == null;

        public static IEnumerable<KeyInfo> GetKeyInfos(this IPropertyGroup propertyGroup)
        {
            foreach (PropertyInfo property in propertyGroup.GetKeyProperties())
            {
                string key = (string)property.GetValue(propertyGroup);

                if (!string.IsNullOrEmpty(key))
                {
                    string fieldName = property.GetKeyFieldName();

                    PropertyInfo matchingProperty = propertyGroup.GetMatchingInputFieldProperty(fieldName);

                    if (matchingProperty == null)
                    {
                        throw new Exception($"Unable to find Property field matching Key: {key}");
                    }

                    yield return new KeyInfo(
                        valueType: matchingProperty.PropertyType,
                        key: key);
                }
            }

            foreach (IPropertyGroup internalGroups in propertyGroup.GetAllPropertyGroups(AllowsKeySearch))
            {
                foreach (KeyInfo keyInfo in internalGroups.GetKeyInfos())
                {
                    yield return keyInfo;
                }
            }
        }


        public static IEnumerable<KeyInfo> GetOutputKeyInfos(this IPropertyGroup propertyGroup)
        {
            foreach (PropertyInfo property in propertyGroup.GetOutputKeyProperties())
            {
                string key = (string)property.GetValue(propertyGroup);

                if (!string.IsNullOrEmpty(key))
                {
                    string fieldName = property.GetOutputKeyFieldName();
                    PropertyInfo matchingProperty = propertyGroup.GetMatchingOutputFieldProperty(fieldName);

                    if (matchingProperty == null)
                    {
                        throw new Exception($"Unable to find Property field matching fieldName: {fieldName}");
                    }

                    yield return new KeyInfo(
                        valueType: matchingProperty.PropertyType,
                        key: key);
                }
            }

            foreach (IPropertyGroup internalGroups in propertyGroup.GetAllPropertyGroups(AllowsKeySearch))
            {
                foreach (KeyInfo keyInfo in internalGroups.GetOutputKeyInfos())
                {
                    yield return keyInfo;
                }
            }
        }

        public static string GetSelectionSerializationName(this IPropertyGroup propertyGroup) =>
            propertyGroup.GetSelectionAttribute().serializationString;

        public static string GetSelectionTitle(this IPropertyGroup propertyGroup) =>
            propertyGroup.GetSelectionAttribute().title;

        public static string GetAdaptiveTemplateTitle(this ControlledParameterTemplate template)
        {
            string titleBase = template.controlledParameter.GetParentListItemTitle();

            if (string.IsNullOrEmpty(titleBase))
            {
                return template.GetSelectionTitle();
            }

            return $"{titleBase} - {template.GetSelectionTitle()}";
        }

        public static string GetItemTitle(this IPropertyGroup propertyGroup)
        {
            foreach (PropertyInfo property in propertyGroup.GetTitleFieldProperty())
            {
                return property.GetValue(propertyGroup) as string;
            }

            Debug.LogError($"Item Title not found for {propertyGroup}");
            return "";
        }

        public static void SetItemTitle(this IPropertyGroup propertyGroup, string title)
        {
            foreach (PropertyInfo property in propertyGroup.GetTitleFieldProperty())
            {
                property.SetValue(propertyGroup, title);
                return;
            }

            Debug.LogError($"Item Title not found for {propertyGroup}");
        }

        public static PropertyInfo GetMatchingKeyProperty(
            this IPropertyGroup propertyGroup,
            PropertyInfo valueProperty)
        {
            foreach (PropertyInfo property in propertyGroup.GetType().GetProperties())
            {
                if (property.GetCustomAttribute<DisplayInputFieldKeyAttribute>() is
                        DisplayInputFieldKeyAttribute attribute &&
                    attribute.fieldName == valueProperty.GetInputFieldName())
                {
                    return property;
                }
            }

            return null;
        }

        public static IEnumerable<PropertyInfo> GetKeyProperties(this IPropertyGroup propertyGroup)
        {
            foreach (PropertyInfo property in propertyGroup.GetType().GetProperties())
            {
                if (property.GetCustomAttribute<DisplayInputFieldKeyAttribute>() != null)
                {
                    yield return property;
                }
            }
        }

        public static IEnumerable<PropertyInfo> GetInitializeableFieldProperties(this IPropertyGroup propertyGroup)
        {
            foreach (PropertyInfo property in propertyGroup.GetType().GetProperties())
            {
                if (property.GetCustomAttribute<DisplayInputFieldAttribute>() != null)
                {
                    yield return property;
                }
                else if (property.GetCustomAttribute<DisplayOutputFieldKeyAttribute>() != null)
                {
                    yield return property;
                }
            }
        }

        public static void InitializeProperties(this IPropertyGroup propertyGroup)
        {
            foreach (PropertyInfo property in propertyGroup.GetInitializeableFieldProperties())
            {
                propertyGroup.InitializeProperty(property);
            }
        }

        public static IEnumerable<PropertyInfo> GetOutputKeyProperties(this IPropertyGroup propertyGroup)
        {
            foreach (PropertyInfo property in propertyGroup.GetType().GetProperties())
            {
                if (property.GetCustomAttribute<DisplayOutputFieldKeyAttribute>() != null)
                {
                    yield return property;
                }
            }
        }

        public static IEnumerable<PropertyInfo> GetInputFieldProperties(this IPropertyGroup propertyGroup)
        {
            foreach (PropertyInfo property in propertyGroup.GetType().GetProperties())
            {
                if (property.GetCustomAttribute<DisplayInputFieldAttribute>() != null)
                {
                    yield return property;
                }
            }
        }
        
        public static IEnumerable<MemberInfo> GetSerializableStateProperties(this IPropertyGroup propertyGroup)
        {
            // Get all properties and fields of the class
            Type type = propertyGroup.GetType();
            PropertyInfo[] propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            IEnumerable<MemberInfo> members = propertyInfos.Cast<MemberInfo>().Concat(fieldInfos);
            
            foreach (MemberInfo member in members)
            {
                // Check if the member has the MyAttribute
                bool hasAttribute = Attribute.IsDefined(member, typeof(SerializableStateAttribute));

                if (hasAttribute)
                {
                    // Get the attribute
                    yield return member;
                }
            }
        }

        public static IEnumerable<PropertyInfo> GetTitleFieldProperty(this IPropertyGroup propertyGroup)
        {
            //In general there should be 0 or 1 of these...
            foreach (PropertyInfo property in propertyGroup.GetType().GetProperties())
            {
                if (property.GetCustomAttribute<PropertyGroupItemTitleAttribute>() != null)
                {
                    yield return property;
                }
            }
        }

        public static PropertyInfo GetMatchingInputFieldProperty(
            this IPropertyGroup propertyGroup,
            string fieldName)
        {
            foreach (PropertyInfo property in propertyGroup.GetType().GetProperties())
            {
                if (property.GetCustomAttribute<DisplayInputFieldAttribute>() is
                        DisplayInputFieldAttribute attribute &&
                    attribute.fieldName == fieldName)
                {
                    return property;
                }
            }

            return null;
        }

        public static PropertyInfo GetMatchingOutputFieldKeyProperty(
            this IPropertyGroup propertyGroup,
            string fieldName)
        {
            foreach (PropertyInfo property in propertyGroup.GetType().GetProperties())
            {
                if (property.GetCustomAttribute<DisplayOutputFieldKeyAttribute>() is
                        DisplayOutputFieldKeyAttribute attribute &&
                    attribute.fieldName == fieldName)
                {
                    return property;
                }
            }

            return null;
        }

        public static PropertyInfo GetMatchingOutputFieldProperty(
            this IPropertyGroup propertyGroup,
            string fieldName)
        {
            foreach (PropertyInfo property in propertyGroup.GetType().GetProperties())
            {
                if (property.GetCustomAttribute<OutputFieldAttribute>() is
                        OutputFieldAttribute attribute &&
                    attribute.fieldName == fieldName)
                {
                    return property;
                }
            }

            return null;
        }

        public static Type GetDefaultSelectionType(this PropertyInfo info)
        {
            foreach (AppendSelectionAttribute selectionAttribute in
                info.GetCustomAttributes<AppendSelectionAttribute>())
            {
                foreach (Type type in selectionAttribute.selectionTypes)
                {
                    return type;
                }
            }

            throw new Exception($"Failed to locate a Selection for property: {info}");
        }

        /// <summary>
        /// Returns the valid Types for this PropertyGroup.
        /// </summary>
        public static IEnumerable<Type> GetListAdditionTypes(this PropertyInfo propertyInfo)
        {
            foreach (AppendAdditionAttribute additionAttribute in
                propertyInfo.GetCustomAttributes<AppendAdditionAttribute>())
            {
                foreach (Type type in additionAttribute.additionTypes)
                {
                    yield return type;
                }
            }
        }

        /// <summary>
        /// Returns the valid Types for this PropertyGroup.
        /// </summary>
        public static IEnumerable<Type> GetSelectionTypes(this PropertyInfo propertyInfo)
        {
            foreach (AppendSelectionAttribute selectionAttribute in
                propertyInfo.GetCustomAttributes<AppendSelectionAttribute>())
            {
                foreach (Type type in selectionAttribute.selectionTypes)
                {
                    yield return type;
                }
            }
        }

        /// <summary>
        /// Used when deserializing to determine which object to construct.
        /// </summary>
        public static Type FindMatchingSelectionType(this PropertyInfo propertyInfo, string typeName)
        {
            foreach (Type type in propertyInfo.GetSelectionTypes())
            {
                if (type.GetSelectionSerializationName() == typeName)
                {
                    return type;
                }
            }

            Debug.LogError($"Failed to locate type {typeName} for property {propertyInfo}");
            return null;
        }

        /// <summary>
        /// Used when deserializing to determine which object to construct.
        /// </summary>
        public static Type FindMatchingListType(this PropertyInfo propertyInfo, string typeName)
        {
            foreach (Type type in propertyInfo.GetListAdditionTypes())
            {
                if (type.GetSelectionSerializationName() == typeName)
                {
                    return type;
                }
            }

            Debug.LogError($"Failed to locate type {typeName} for property {propertyInfo}");
            return null;
        }

        public static IPropertyGroup Build(this Type type) =>
            Activator.CreateInstance(type) as IPropertyGroup;

        /// <summary>
        /// Build the specified type, and assign it to the specified propertyInfo in the parentGroup.
        /// </summary>
        public static IPropertyGroup Build(this Type type, IPropertyGroup parentGroup, PropertyInfo propertyInfo)
        {
            IPropertyGroup newPropertyGroup = Activator.CreateInstance(type) as IPropertyGroup;
            propertyInfo.SetValue(parentGroup, newPropertyGroup);
            newPropertyGroup.SetParent(parentGroup);

            newPropertyGroup.InitializeProperties();

            return newPropertyGroup;
        }

        /// <summary>
        /// Build the specified type, and Add it to the specified List in the parentGroup.
        /// </summary>
        public static IPropertyGroup Build(this Type type, string itemTitle, IPropertyGroup parentGroup, IList propertyList)
        {
            IPropertyGroup newPropertyGroup = Activator.CreateInstance(type) as IPropertyGroup;
            propertyList.Add(newPropertyGroup);
            newPropertyGroup.SetParent(parentGroup);
            newPropertyGroup.SetItemTitle(itemTitle);

            newPropertyGroup.InitializeProperties();
            newPropertyGroup.BuildPartialPropertyGroup();

            return newPropertyGroup;
        }

        public static string GetInitializableFieldName(this PropertyInfo propertyInfo) =>
            propertyInfo.GetCustomAttribute<DisplayInputFieldAttribute>()?.fieldName ??
            propertyInfo.GetCustomAttribute<DisplayOutputFieldKeyAttribute>()?.fieldName ?? "";

        public static string GetInputFieldName(this PropertyInfo propertyInfo) =>
            propertyInfo.GetCustomAttribute<DisplayInputFieldAttribute>()?.fieldName ?? "";

        public static string GetOutputKeyFieldName(this PropertyInfo propertyInfo) =>
            propertyInfo.GetCustomAttribute<DisplayOutputFieldKeyAttribute>()?.fieldName ?? "";

        public static string GetOutputFieldName(this PropertyInfo propertyInfo) =>
            propertyInfo.GetCustomAttribute<OutputFieldAttribute>()?.fieldName ?? "";

        public static string GetKeyFieldName(this PropertyInfo propertyInfo) =>
            propertyInfo.GetCustomAttribute<DisplayInputFieldKeyAttribute>()?.fieldName ?? "";

        public static FieldDisplayAttribute GetFieldDisplayAttribute(
            this IPropertyGroup propertyGroup,
            string fieldName)
        {
            foreach (FieldDisplayAttribute attribute in
                propertyGroup.GetType().GetCustomAttributes_Deep<FieldDisplayAttribute>())
            {
                if (attribute.fieldName == fieldName)
                {
                    return attribute;
                }
            }

            return null;
        }

        public static void InitializeProperty(this IPropertyGroup propertyGroup, PropertyInfo property)
        {
            FieldDisplayAttribute attribute = propertyGroup.GetFieldDisplayAttribute(
                fieldName: property.GetInitializableFieldName());

            if (attribute == null)
            {
                Debug.LogError(
                    $"Unable to find FieldDisplayAttribute for Property: {property.Name}");
                return;
            }

            object initialValue = attribute.GetInitialValue();

            if (attribute is FieldMirrorDisplayAttribute mirrored)
            {
                initialValue = propertyGroup
                    .SearchHierarchyForConcreteFieldAttribute(mirrored.mirroredFieldName)
                    ?.GetInitialValue();
            }

            property.SetValue(propertyGroup, initialValue);
        }

        public static void BuildPartialPropertyGroup(this IPropertyGroup propertyGroup)
        {
            foreach (PropertyInfo property in propertyGroup.GetPropertyGroupProperties())
            {
                IPropertyGroup internalPropertyGroup = property.GetValue(propertyGroup) as IPropertyGroup;
                if (internalPropertyGroup == null)
                {
                    //Construct a new instance from first selection
                    internalPropertyGroup = property.GetDefaultSelectionType().Build(propertyGroup, property);
                }

                internalPropertyGroup.BuildPartialPropertyGroup();
            }

            foreach (PropertyInfo property in propertyGroup.GetPropertyGroupListProperties())
            {
                IList internalPropertyGroupList = property.GetValue(propertyGroup) as IList;
                if (internalPropertyGroupList == null)
                {
                    internalPropertyGroupList = Activator.CreateInstance(property.PropertyType) as IList;
                    property.SetValue(propertyGroup, internalPropertyGroupList);
                }

                foreach (IPropertyGroup internalPropertyGroup in internalPropertyGroupList)
                {
                    internalPropertyGroup.BuildPartialPropertyGroup();
                }
            }
        }

        public static FieldDisplayAttribute SearchHierarchyForConcreteFieldAttribute(
            this IPropertyGroup propertyGroup,
            string fieldName)
        {
            (IPropertyGroup _, FieldDisplayAttribute attribute) =
                propertyGroup.SearchHierarchyForConcreteFieldAttributeAndPropertyGroup(fieldName);

            return attribute;
        }

        public static (IPropertyGroup, FieldDisplayAttribute) SearchHierarchyForConcreteFieldAttributeAndPropertyGroup(
            this IPropertyGroup propertyGroup,
            string fieldName)
        {
            IPropertyGroup current = propertyGroup;
            FieldDisplayAttribute attribute = null;

            while (current != null)
            {
                attribute = current.GetFieldDisplayAttribute(fieldName);
                if (attribute != null)
                {
                    if (attribute is FieldMirrorDisplayAttribute mirrored)
                    {
                        fieldName = mirrored.mirroredFieldName;
                    }
                    else if (attribute is ControlledExtractionAttribute extraction)
                    {
                        if (current is ControlledParameterTemplate algorithm)
                        {
                            if (algorithm.controlledParameter is IPropertyGroup adaptivePropertyGroup)
                            {
                                current = adaptivePropertyGroup;
                                fieldName = extraction.extractionFieldName;
                                //Skip to top of loop so we don't fail to test this PropertyGroup
                                continue;
                            }
                        }

                        throw new Exception("Invalid use of Extraction attribute");
                    }
                    else
                    {
                        break;
                    }
                }

                current = current.GetParent();
            }

            return (current, attribute);
        }

        public static void InitializeRandomizers(
            this IPropertyGroup propertyGroup,
            Func<Random> randomizerGetter)
        {
            foreach (IRandomizer randomizer in propertyGroup.GetAll<IRandomizer>())
            {
                randomizer.AssignRandomizer(randomizerGetter);
            }
        }

        public static IEnumerable<T> GetAll<T>(this IPropertyGroup propertyGroup)
        {
            if (propertyGroup is T typedPropertyGroup)
            {
                yield return typedPropertyGroup;
            }

            foreach (IPropertyGroup internalGroup in propertyGroup.GetAllPropertyGroups())
            {
                foreach (T internalClass in internalGroup.GetAll<T>())
                {
                    yield return internalClass;
                }
            }
        }

        public static void InitializeParameters(this IPropertyGroup propertyGroup,
            GlobalRuntimeContext scriptContext)
        {
            foreach (PropertyInfo property in propertyGroup.GetKeyProperties())
            {
                string keyValue = property.GetValue(propertyGroup) as string;

                if (!string.IsNullOrEmpty(keyValue))
                {
                    string fieldName = property.GetKeyFieldName();

                    PropertyInfo valueProperty = propertyGroup.GetMatchingInputFieldProperty(fieldName);

                    if (valueProperty == null)
                    {
                        throw new Exception($"Unable to find matching parameter: {fieldName}");
                    }

                    if (!scriptContext.VariableExists(keyValue))
                    {
                        Debug.LogWarning($"State dictionary did not contain key: \"{keyValue}\". Using default value.");
                        valueProperty.SetValue(propertyGroup, valueProperty.PropertyType.GetDefaultValue());
                    }
                    else
                    {
                        valueProperty.SetValue(propertyGroup, scriptContext.GetRawValue(keyValue));
                    }
                }
            }

            foreach (IPropertyGroup innerPropertyGroup in propertyGroup.GetAllPropertyGroups())
            {
                innerPropertyGroup.InitializeParameters(scriptContext);
            }
        }

        public static void PopulateScriptContextOutputs(this IPropertyGroup propertyGroup,
            GlobalRuntimeContext scriptContext)
        {
            foreach (PropertyInfo property in propertyGroup.GetOutputKeyProperties())
            {
                string keyValue = property.GetValue(propertyGroup) as string;

                if (!string.IsNullOrEmpty(keyValue))
                {
                    string fieldName = property.GetOutputKeyFieldName();

                    PropertyInfo matchingProperty = propertyGroup.GetMatchingOutputFieldProperty(fieldName);

                    scriptContext.AddOrSetValue(
                        key: keyValue,
                        type: matchingProperty.PropertyType,
                        value: matchingProperty.GetValue(propertyGroup));
                }
            }

            foreach (IPropertyGroup innerPropertyGroup in propertyGroup.GetAllPropertyGroups())
            {
                innerPropertyGroup.PopulateScriptContextOutputs(scriptContext);
            }
        }

        public static IPropertyGroup GetRoot(this IPropertyGroup propertyGroup, bool taskGroupOnly)
        {
            while (propertyGroup.GetParent() is IPropertyGroup parent)
            {
                if (taskGroupOnly && propertyGroup.GetType().IsTaskGroupTerminator())
                {
                    break;
                }

                propertyGroup = parent;
            }

            return propertyGroup;
        }

        public static IEnumerable<PropertyInfo> GetSortedProperties(this Type type)
        {
            Dictionary<Type, int> orderLookup = new Dictionary<Type, int>();
            Type iteratingType = type;
            int index = 0;

            do
            {
                orderLookup.Add(iteratingType, index);
                iteratingType = iteratingType.BaseType;
                index++;
            }
            while (iteratingType != null);

            return type.GetProperties().OrderBy(GetPropertyOrderValue).ThenByDescending(x => orderLookup[x.DeclaringType]);
        }

        /// <summary>
        /// Returns the ordering value for this PropertyInfo
        /// </summary>
        public static int GetPropertyOrderValue(this PropertyInfo propertyInfo)
        {
            if (propertyInfo.GetCustomAttribute<OverrideDefaultOrderingAttribute>() is
                OverrideDefaultOrderingAttribute attr)
            {
                return attr.orderPriority;
            }

            return 0;
        }
    }

#pragma warning restore IDE0019 // Use pattern matching
}
