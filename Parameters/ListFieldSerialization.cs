using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using LightJson;

using Debug = UnityEngine.Debug;

namespace BGC.Parameters
{
    /// <summary>
    /// Reading and writing of List&lt;T&gt; input fields (T = int, double, string, bool).
    ///
    /// A list is written as a JSON array under its field name, always, including an empty list
    /// as []. (Omitting it would reload as the attribute's initial items, which need not be empty.)
    ///
    /// Reading accepts, in order:
    ///   - an array: each item read with the same LightJson conversion the scalar fields use;
    ///   - a string, when the field's attribute declares legacyDelimiters: split, trimmed, empty
    ///     pieces dropped, each piece parsed with the invariant culture ("" reads as []);
    ///   - any other single value of the item's kind: a one-item list. This lets a scalar field
    ///     become a list without a data version bump.
    /// Anything else is unparseable. Then the field loads as an empty list, an error is logged,
    /// and the original value is kept and written back unchanged on save until the list is edited
    /// (see <see cref="ClearUnparsedListValue"/>), so a bad battery is never silently replaced
    /// with different values. Callers can refuse to run via <see cref="GetUnparsedListFields"/>.
    ///
    /// Items are not clamped and item counts are not enforced on load, matching scalar fields,
    /// whose bounds are only enforced by the editor. A count outside the attribute's bounds is
    /// logged as a warning.
    /// </summary>
    public static class ListFieldSerialization
    {
        /// <summary>A saved value that could not be read, and the empty list loaded in its place.</summary>
        private sealed class UnparsedValue
        {
            public JsonValue original;
            public IList loadedList;
        }

        private static readonly ConditionalWeakTable<IPropertyGroup, Dictionary<string, UnparsedValue>> unparsedValues =
            new ConditionalWeakTable<IPropertyGroup, Dictionary<string, UnparsedValue>>();

        public static bool IsSupportedElementType(Type elementType) =>
            elementType == typeof(int) ||
            elementType == typeof(double) ||
            elementType == typeof(string) ||
            elementType == typeof(bool);

        /// <summary>The item type if this is a List&lt;T&gt; property, otherwise null.</summary>
        public static Type GetListElementType(this PropertyInfo property)
        {
            Type type = property.PropertyType;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                return type.GetGenericArguments()[0];
            }

            return null;
        }

        public static bool IsListField(this PropertyInfo property) =>
            IsSupportedElementType(property.GetListElementType());

        /// <summary>
        /// The list attribute governing this field, following a FieldMirrorDisplay if present.
        /// Null if there is none (the reflection contract test reports that case).
        /// </summary>
        public static ListFieldDisplayAttribute GetListFieldAttribute(
            this IPropertyGroup propertyGroup,
            string fieldName)
        {
            FieldDisplayAttribute attribute = propertyGroup.GetFieldDisplayAttribute(fieldName);

            if (attribute is FieldMirrorDisplayAttribute)
            {
                attribute = propertyGroup.SearchHierarchyForConcreteFieldAttribute(fieldName);
            }

            return attribute as ListFieldDisplayAttribute;
        }

        public static JsonValue SerializeListField(
            IPropertyGroup propertyGroup,
            PropertyInfo property,
            string fieldName)
        {
            IList list = property.GetValue(propertyGroup) as IList;

            if (TryGetPreservedValue(propertyGroup, fieldName, list, out JsonValue preserved))
            {
                return preserved;
            }

            JsonArray array = new JsonArray();

            if (list != null)
            {
                Type elementType = property.GetListElementType();
                foreach (object item in list)
                {
                    array.Add(SerializeItem(elementType, item));
                }
            }

            return array;
        }

        public static void DeserializeListField(
            IPropertyGroup propertyGroup,
            PropertyInfo property,
            string fieldName,
            JsonValue value)
        {
            Type elementType = property.GetListElementType();
            ListFieldDisplayAttribute attribute = propertyGroup.GetListFieldAttribute(fieldName);
            IList list = (IList)Activator.CreateInstance(property.PropertyType);

            ClearUnparsedListValue(propertyGroup, fieldName);

            // Without the attribute (e.g. a mirrored field read before its parent is set) a
            // non-array value cannot be read safely: its delimiters are unknown
            bool readable = (attribute != null || value.IsJsonArray) &&
                TryReadItems(value, elementType, attribute?.legacyDelimiters, list);

            if (!readable)
            {
                RecordUnparsed(propertyGroup, fieldName, value, list,
                    attribute == null ? " (its list attribute could not be found)" : "");
            }
            else
            {
                WarnIfCountInvalid(propertyGroup, fieldName, attribute, list);
            }

            property.SetValue(propertyGroup, list);
        }

        #region Tuple lists

        /// <summary>Whether this is a List of a 2- to 4-item value tuple of int, double, string or bool.</summary>
        public static bool IsTupleListField(this PropertyInfo property) =>
            TupleListLayout.GetTupleRowType(property) != null;

        /// <summary>Each row is written as an object keyed by its columns' names.</summary>
        public static JsonValue SerializeTupleListField(
            IPropertyGroup propertyGroup,
            PropertyInfo property,
            string fieldName)
        {
            IList list = property.GetValue(propertyGroup) as IList;

            if (TryGetPreservedValue(propertyGroup, fieldName, list, out JsonValue preserved))
            {
                return preserved;
            }

            JsonArray array = new JsonArray();
            TupleListLayout layout = TupleListLayout.Get(propertyGroup, property);

            if (layout == null)
            {
                Debug.LogError(
                    $"Tuple list field \"{fieldName}\" of {propertyGroup.GetType().Name} has missing or " +
                    $"inconsistent column attributes and cannot be saved; see GetListFieldDeclarationErrors.");
                return array;
            }

            if (list != null)
            {
                foreach (object row in list)
                {
                    JsonObject rowData = new JsonObject();
                    for (int i = 0; i < layout.Columns.Count; i++)
                    {
                        rowData.Add(
                            layout.Columns[i].columnName,
                            SerializeItem(layout.Columns[i].ElementType, layout.GetItem(row, i)));
                    }
                    array.Add(rowData);
                }
            }

            return array;
        }

        /// <summary>
        /// Reads rows from an array of objects keyed by column name (a missing or unknown key makes
        /// the value unreadable, so adding, renaming or removing a column never silently changes
        /// or drops data), or from a legacy string split into rows on
        /// legacyDelimiters and into columns on legacyColumnDelimiters (every row must have every
        /// column). Unreadable values are preserved as for other list fields.
        /// </summary>
        public static void DeserializeTupleListField(
            IPropertyGroup propertyGroup,
            PropertyInfo property,
            string fieldName,
            JsonValue value)
        {
            TupleListLayout layout = TupleListLayout.Get(propertyGroup, property);
            IList list = (IList)Activator.CreateInstance(property.PropertyType);

            ClearUnparsedListValue(propertyGroup, fieldName);

            bool readable = layout != null && TryReadRows(value, layout, list);

            if (!readable)
            {
                RecordUnparsed(propertyGroup, fieldName, value, list,
                    layout == null ? " (its tuple column attributes are missing or inconsistent)" : "");
            }
            else
            {
                WarnIfCountInvalid(propertyGroup, fieldName, layout.Attribute, list);
            }

            property.SetValue(propertyGroup, list);
        }

        private static bool TryReadRows(JsonValue value, TupleListLayout layout, IList list)
        {
            int columnCount = layout.Columns.Count;

            if (value.IsJsonArray)
            {
                foreach (JsonValue rowValue in value.AsJsonArray)
                {
                    JsonObject rowData = rowValue.AsJsonObject;
                    if (rowData == null ||
                        ((IEnumerable<KeyValuePair<string, JsonValue>>)rowData)
                            .Any(entry => layout.Columns.All(column => column.columnName != entry.Key)))
                    {
                        return false;
                    }

                    object[] items = new object[columnCount];
                    for (int i = 0; i < columnCount; i++)
                    {
                        TupleColumnAttribute column = layout.Columns[i];

                        // A missing column is not filled in: that would run with a value nobody
                        // chose. A new column needs an explicit upgrade of the saved rows.
                        if (rowData.ContainsKey(column.columnName) &&
                            IsItemKind(rowData[column.columnName], column.ElementType))
                        {
                            items[i] = ReadItem(rowData[column.columnName], column.ElementType);
                        }
                        else
                        {
                            return false;
                        }
                    }

                    list.Add(layout.CreateRow(items));
                }

                return true;
            }

            char[] rowDelimiters = layout.Attribute.legacyDelimiters;
            char[] columnDelimiters = layout.Attribute.legacyColumnDelimiters;

            if (value.IsString && rowDelimiters != null && columnDelimiters != null)
            {
                foreach (string rowText in SplitLegacyString(value.AsString, rowDelimiters))
                {
                    string[] pieces = rowText.Split(columnDelimiters).Select(x => x.Trim()).ToArray();
                    if (pieces.Length != columnCount)
                    {
                        return false;
                    }

                    object[] items = new object[columnCount];
                    for (int i = 0; i < columnCount; i++)
                    {
                        if (!TryParsePiece(pieces[i], layout.Columns[i].ElementType, out items[i]))
                        {
                            return false;
                        }
                    }

                    list.Add(layout.CreateRow(items));
                }

                return true;
            }

            return false;
        }

        #endregion Tuple lists

        /// <summary>Field names of lists on this group (not its children) whose saved value could not be read.</summary>
        public static IEnumerable<string> GetUnparsedListFields(this IPropertyGroup propertyGroup) =>
            unparsedValues.TryGetValue(propertyGroup, out Dictionary<string, UnparsedValue> table)
                ? table.Keys.ToList()
                : Enumerable.Empty<string>();

        public static bool TryGetUnparsedListValue(
            this IPropertyGroup propertyGroup,
            string fieldName,
            out JsonValue value)
        {
            value = JsonValue.Null;

            if (unparsedValues.TryGetValue(propertyGroup, out Dictionary<string, UnparsedValue> table) &&
                table.TryGetValue(fieldName, out UnparsedValue unparsed))
            {
                value = unparsed.original;
                return true;
            }

            return false;
        }

        /// <summary>Call when the user edits the list, so the edited list is saved instead of the original value.</summary>
        public static void ClearUnparsedListValue(this IPropertyGroup propertyGroup, string fieldName)
        {
            if (unparsedValues.TryGetValue(propertyGroup, out Dictionary<string, UnparsedValue> table))
            {
                table.Remove(fieldName);
            }
        }

        /// <summary>
        /// Forget every unreadable value recorded for this group. Called at the start of each
        /// deserialization, so a reused group never carries a value from earlier data.
        /// </summary>
        public static void ClearUnparsedListValues(this IPropertyGroup propertyGroup) =>
            unparsedValues.Remove(propertyGroup);

        /// <summary>
        /// Declaration errors for list fields on a property-group type: a List&lt;T&gt; input field
        /// must have a supported item type, a list attribute of the same item type (or a mirror),
        /// no input key (script variables are scalars), and a setter. A list attribute
        /// must not be attached to a non-list property. Empty when the type is well formed.
        /// </summary>
        public static IEnumerable<string> GetListFieldDeclarationErrors(Type propertyGroupType)
        {
            List<FieldDisplayAttribute> displayAttributes =
                propertyGroupType.GetCustomAttributes_Deep<FieldDisplayAttribute>().ToList();
            HashSet<string> keyedFields = new HashSet<string>(propertyGroupType.GetProperties()
                .Select(x => x.GetCustomAttribute<DisplayInputFieldKeyAttribute>()?.fieldName)
                .Where(x => x != null));

            foreach (PropertyInfo property in propertyGroupType.GetProperties())
            {
                string fieldName = property.GetCustomAttribute<DisplayInputFieldAttribute>()?.fieldName;
                if (fieldName == null)
                {
                    continue;
                }

                string location = $"{propertyGroupType.FullName}.{property.Name}";
                FieldDisplayAttribute attribute = displayAttributes.FirstOrDefault(x => x.fieldName == fieldName);
                Type elementType = property.GetListElementType();

                if (elementType == null)
                {
                    if (attribute is ListFieldDisplayAttribute)
                    {
                        yield return $"{location}: list attribute on a non-List property ({property.PropertyType.Name}).";
                    }
                    continue;
                }

                if (TupleListLayout.IsValueTuple(elementType))
                {
                    foreach (string error in GetTupleListDeclarationErrors(
                        propertyGroupType, property, fieldName, attribute, location))
                    {
                        yield return error;
                    }
                }
                else if (!IsSupportedElementType(elementType))
                {
                    yield return $"{location}: List<{elementType.Name}> is not a supported list field (int, double, string, bool).";
                    continue;
                }
                else if (attribute is TupleListFieldDisplayAttribute)
                {
                    yield return $"{location}: a tuple list attribute needs a List of value tuples.";
                }

                if (attribute == null)
                {
                    yield return $"{location}: no FieldDisplay attribute for field \"{fieldName}\".";
                }
                else if (TupleListLayout.IsValueTuple(elementType) || attribute is TupleListFieldDisplayAttribute)
                {
                    // Reported above
                }
                else if (attribute is ListFieldDisplayAttribute listAttribute)
                {
                    if (listAttribute.ElementType != elementType)
                    {
                        yield return $"{location}: List<{elementType.Name}> but its attribute is for {listAttribute.ElementType.Name} items.";
                    }
                }
                else if (!(attribute is FieldMirrorDisplayAttribute))
                {
                    yield return $"{location}: List<{elementType.Name}> needs a list attribute, not {attribute.GetType().Name}.";
                }

                if (keyedFields.Contains(fieldName))
                {
                    yield return $"{location}: list fields cannot have a [DisplayInputFieldKey]; script variables are scalars.";
                }

                if (!property.CanWrite)
                {
                    yield return $"{location}: list field needs a setter.";
                }
            }

            HashSet<string> tupleFields = new HashSet<string>(propertyGroupType.GetProperties()
                .Where(x => x.GetCustomAttribute<DisplayInputFieldAttribute>() != null &&
                    x.GetListElementType() is Type rowType && TupleListLayout.IsValueTuple(rowType))
                .Select(x => x.GetInputFieldName()));

            foreach (string orphan in propertyGroupType.GetCustomAttributes_Deep<TupleColumnAttribute>()
                .Select(x => x.fieldName)
                .Distinct()
                .Where(x => !tupleFields.Contains(x)))
            {
                yield return $"{propertyGroupType.FullName}: tuple columns for \"{orphan}\", which is not a tuple list field.";
            }
        }

        private static IEnumerable<string> GetTupleListDeclarationErrors(
            Type propertyGroupType,
            PropertyInfo property,
            string fieldName,
            FieldDisplayAttribute attribute,
            string location)
        {
            Type rowType = TupleListLayout.GetTupleRowType(property);
            if (rowType == null)
            {
                yield return $"{location}: a tuple list needs {TupleListLayout.MinimumColumns} to " +
                    $"{TupleListLayout.MaximumColumns} items of int, double, string or bool.";
                yield break;
            }

            if (attribute == null)
            {
                // Reported by the caller
                yield break;
            }

            if (!(attribute is TupleListFieldDisplayAttribute tupleAttribute))
            {
                yield return $"{location}: a tuple list needs a TupleListFieldDisplay attribute, " +
                    $"not {attribute.GetType().Name} (mirrors are not supported).";
                yield break;
            }

            if ((tupleAttribute.legacyDelimiters == null) != (tupleAttribute.legacyColumnDelimiters == null))
            {
                yield return $"{location}: set both legacyDelimiters and legacyColumnDelimiters, or neither.";
            }

            Type[] itemTypes = rowType.GetGenericArguments();
            TupleColumnAttribute[] columns = TupleListLayout.GetColumns(propertyGroupType, fieldName);

            for (int i = 0; i < itemTypes.Length; i++)
            {
                TupleColumnAttribute column = columns.FirstOrDefault(x => x.index == i);
                if (column == null)
                {
                    yield return $"{location}: no tuple column {i}.";
                }
                else if (column.ElementType != itemTypes[i])
                {
                    yield return $"{location}: column {i} is {column.GetType().Name} but item {i + 1} is {itemTypes[i].Name}.";
                }
            }

            foreach (TupleColumnAttribute column in columns.Where(x => x.index < 0 || x.index >= itemTypes.Length))
            {
                yield return $"{location}: column index {column.index} is outside the tuple's {itemTypes.Length} items.";
            }

            if (columns.Any(x => string.IsNullOrEmpty(x.columnName)) ||
                columns.Select(x => x.columnName).Distinct().Count() != columns.Length)
            {
                yield return $"{location}: column names must be non-empty and unique.";
            }

            if (columns.Select(x => x.InitialValues.Length).Distinct().Count() > 1)
            {
                yield return $"{location}: every column needs the same number of initial values.";
            }
        }

        /// <summary>
        /// True when the list is still the untouched one loaded in place of an unreadable value;
        /// then the original is saved exactly. Editing or replacing the list ends this.
        /// </summary>
        private static bool TryGetPreservedValue(
            IPropertyGroup propertyGroup,
            string fieldName,
            IList list,
            out JsonValue original)
        {
            original = JsonValue.Null;

            if (unparsedValues.TryGetValue(propertyGroup, out Dictionary<string, UnparsedValue> table) &&
                table.TryGetValue(fieldName, out UnparsedValue unparsed) &&
                ReferenceEquals(list, unparsed.loadedList) &&
                list.Count == 0)
            {
                original = unparsed.original;
                return true;
            }

            return false;
        }

        private static void RecordUnparsed(
            IPropertyGroup propertyGroup,
            string fieldName,
            JsonValue value,
            IList list,
            string reason)
        {
            Debug.LogError(
                $"Unable to read list field \"{fieldName}\" of {propertyGroup.GetType().Name} " +
                $"from {value}{reason}. It is loaded as an empty list; the original value is kept and " +
                "saved unchanged until the list is edited.");

            list.Clear();
            GetUnparsedTable(propertyGroup)[fieldName] = new UnparsedValue
            {
                original = value,
                loadedList = list
            };
        }

        private static void WarnIfCountInvalid(
            IPropertyGroup propertyGroup,
            string fieldName,
            ListFieldDisplayAttribute attribute,
            IList list)
        {
            if (attribute != null && !attribute.IsCountValid(list.Count))
            {
                Debug.LogWarning(
                    $"List field \"{fieldName}\" of {propertyGroup.GetType().Name} has {list.Count} " +
                    $"items, outside its allowed {attribute.minimumCount} to {attribute.maximumCount}. " +
                    $"Loaded as-is.");
            }
        }

        private static Dictionary<string, UnparsedValue> GetUnparsedTable(IPropertyGroup propertyGroup) =>
            unparsedValues.GetValue(propertyGroup, _ => new Dictionary<string, UnparsedValue>());

        private static JsonValue SerializeItem(Type elementType, object item)
        {
            if (elementType == typeof(int))
            {
                return Convert.ToInt32(item);
            }

            if (elementType == typeof(double))
            {
                return Convert.ToDouble(item);
            }

            if (elementType == typeof(bool))
            {
                return Convert.ToBoolean(item);
            }

            return item as string ?? "";
        }

        private static bool TryReadItems(
            JsonValue value,
            Type elementType,
            char[] legacyDelimiters,
            IList list)
        {
            if (value.IsJsonArray)
            {
                foreach (JsonValue item in value.AsJsonArray)
                {
                    if (!IsItemKind(item, elementType))
                    {
                        return false;
                    }

                    list.Add(ReadItem(item, elementType));
                }

                return true;
            }

            if (value.IsString && (legacyDelimiters != null || elementType != typeof(string)))
            {
                // A delimited string (legacy format), or a single number/bool written as a string
                foreach (string piece in SplitLegacyString(value.AsString, legacyDelimiters))
                {
                    if (!TryParsePiece(piece, elementType, out object item))
                    {
                        return false;
                    }

                    list.Add(item);
                }

                return true;
            }

            if (IsItemKind(value, elementType))
            {
                // A former scalar field
                list.Add(ReadItem(value, elementType));
                return true;
            }

            return false;
        }

        private static bool IsItemKind(JsonValue value, Type elementType)
        {
            if (elementType == typeof(int))
            {
                // Not IsNumber: AsInteger would silently truncate 1.5 or clamp 2147483648
                return value.IsInteger;
            }

            if (elementType == typeof(double))
            {
                return value.IsNumber;
            }

            if (elementType == typeof(bool))
            {
                return value.IsBoolean;
            }

            return value.IsString;
        }

        // Same LightJson conversions the scalar fields use in Internal_RawDeserialize
        private static object ReadItem(JsonValue value, Type elementType)
        {
            if (elementType == typeof(int))
            {
                return value.AsInteger;
            }

            if (elementType == typeof(double))
            {
                return value.AsNumber;
            }

            if (elementType == typeof(bool))
            {
                return value.AsBoolean;
            }

            return value.AsString;
        }

        private static IEnumerable<string> SplitLegacyString(string text, char[] delimiters)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return Enumerable.Empty<string>();
            }

            if (delimiters == null || delimiters.Length == 0)
            {
                return new[] { text.Trim() };
            }

            if (Array.IndexOf(delimiters, '\n') >= 0)
            {
                text = text.Replace("\r\n", "\n").Replace('\r', '\n');
            }

            return text
                .Split(delimiters)
                .Select(x => x.Trim())
                .Where(x => x.Length > 0);
        }

        private static bool TryParsePiece(string piece, Type elementType, out object item)
        {
            if (elementType == typeof(int))
            {
                bool parsed = int.TryParse(piece, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue);
                item = intValue;
                return parsed;
            }

            if (elementType == typeof(double))
            {
                bool parsed = double.TryParse(piece, NumberStyles.Float, CultureInfo.InvariantCulture, out double doubleValue);
                item = doubleValue;
                return parsed;
            }

            if (elementType == typeof(bool))
            {
                bool parsed = bool.TryParse(piece, out bool boolValue);
                item = boolValue;
                return parsed;
            }

            item = piece;
            return true;
        }
    }
}
