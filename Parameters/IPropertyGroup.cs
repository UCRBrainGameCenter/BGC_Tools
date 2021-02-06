using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LightJson;
using BGC.Scripting;

using Debug = UnityEngine.Debug;

namespace BGC.Parameters
{
#pragma warning disable IDE0019 // Use pattern matching

    public interface IPropertyGroup
    {
        IPropertyGroup GetParent();
        void SetParent(IPropertyGroup parent);

        JsonObject Serialize();
        void Deserialize(JsonObject data);
    }

    public static class PropertyGroupExtensions
    {
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

        public static JsonObject Internal_GetSerializedData(this IPropertyGroup propertyGroup)
        {
            JsonObject propertyGroupData = new JsonObject();
            JsonObject keyData = new JsonObject();

            propertyGroupData.Add("Type", propertyGroup.GetSelectionSerializationName());

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

                switch (property.PropertyType.Name)
                {
                    case "Single":
                        propertyGroupData.Add(att.fieldName, Convert.ToSingle(property.GetValue(propertyGroup)));
                        break;

                    case "Double":
                        propertyGroupData.Add(att.fieldName, Convert.ToDouble(property.GetValue(propertyGroup)));
                        break;

                    case "Int32":
                        propertyGroupData.Add(att.fieldName, Convert.ToInt32(property.GetValue(propertyGroup)));
                        break;

                    case "Int64":
                        propertyGroupData.Add(att.fieldName, Convert.ToInt64(property.GetValue(propertyGroup)));
                        break;

                    case "String":
                        propertyGroupData.Add(att.fieldName, property.GetValue(propertyGroup) as string);
                        break;

                    case "Boolean":
                        propertyGroupData.Add(att.fieldName, Convert.ToBoolean(property.GetValue(propertyGroup)));
                        break;

                    default:
                        if (property.PropertyType.IsEnum)
                        {
                            propertyGroupData.Add(att.fieldName, property.GetValue(propertyGroup).ToString());
                            break;
                        }
                        Debug.LogError($"Unsupported datatype ({property.PropertyType.Name}) for variable {property.Name}");
                        break;
                }
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
                        value: innerPropertyGroup.Serialize());
                }
            }

            foreach (PropertyInfo innerList in propertyGroup.GetPropertyGroupListProperties())
            {
                if (innerList.GetValue(propertyGroup) is IList innerPropertyList)
                {
                    JsonArray propertyListValues = new JsonArray();

                    foreach (IPropertyGroup listElement in innerPropertyList)
                    {
                        propertyListValues.Add(listElement.Serialize());
                    }

                    if (propertyListValues.Count > 0)
                    {
                        propertyGroupData.Add(
                            key: innerList.GetGroupListSerializationName(),
                            value: propertyListValues);
                    }
                }
            }

            if (keyData.Count > 0)
            {
                propertyGroupData.Add("Keys", keyData);
            }

            return propertyGroupData;
        }

        public static void Internal_RawDeserialize(this IPropertyGroup container, JsonObject propertyGroupData)
        {
            //Deserialize Property Groups
            foreach (PropertyInfo property in container.GetPropertyGroupProperties())
            {
                string propertyGroupName = property.GetGroupSerializationName();

                if (!propertyGroupData.ContainsKey(propertyGroupName))
                {
                    Debug.Log($"Data not found for group {propertyGroupName}.  Creating default.");
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
                    switch (property.PropertyType.Name)
                    {
                        case "Single":
                            property.SetValue(container, (float)propertyGroupData[fieldName].AsNumber);
                            break;

                        case "Double":
                            property.SetValue(container, propertyGroupData[fieldName].AsNumber);
                            break;

                        case "Int32":
                            property.SetValue(container, propertyGroupData[fieldName].AsInteger);
                            break;

                        case "Int64":
                            property.SetValue(container, propertyGroupData[fieldName].AsInteger);
                            break;

                        case "String":
                            property.SetValue(container, propertyGroupData[fieldName].AsString);
                            break;

                        case "Boolean":
                            property.SetValue(container, propertyGroupData[fieldName].AsBoolean);
                            break;

                        default:
                            if (property.PropertyType.IsEnum)
                            {
                                property.SetValue(container,
                                    Enum.Parse(property.PropertyType, propertyGroupData[fieldName].AsString));
                                break;
                            }
                            Debug.LogError($"Unsupported datatype ({property.PropertyType.Name}) for variable {property.Name}");
                            break;
                    }
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
            PropertyInfo property)
        {
            JsonObject serializedData = source.Serialize();

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
                        throw new Exception($"State dictionary did not contain key: {keyValue}");
                    }

                    valueProperty.SetValue(propertyGroup, scriptContext.GetRawValue(keyValue));
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
