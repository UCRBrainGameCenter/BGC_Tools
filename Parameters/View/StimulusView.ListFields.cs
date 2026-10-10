using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using LightJson;
using BGC.Mathematics;

using Object = UnityEngine.Object;

namespace BGC.Parameters.View
{
    /// <summary>
    /// Editor for list input fields: List&lt;T&gt; (see <see cref="ListFieldDisplayAttribute"/>) and
    /// tuple lists, List&lt;(T1, T2[, T3[, T4]])&gt; (see <see cref="TupleListFieldDisplayAttribute"/>).
    ///
    /// A header row (title, item count with its allowed range, Add) is followed by one row per
    /// item: [index] [value] [Up] [Down] [Copy] [Delete]. A tuple row puts one value per column in
    /// the value slot, under a row of column titles. Rows are ordinary 60px option rows placed
    /// directly in the property frame, so they line up with the scalar fields around them.
    ///
    /// Every structural edit (add, move, copy, delete) rebuilds this field's item rows itself,
    /// so the index each row acts on is always current. It does not depend on the property
    /// group's respawn callback, which is null inside list items, inline groups and controlled
    /// parameter templates.
    ///
    /// Bounds behave as for scalar fields: typed values are clamped to the item (or column)
    /// range, and Add, Copy and Delete are disabled at the count limits. A list loaded outside its
    /// bounds is left as-is and its count is shown in the error color.
    /// </summary>
    public abstract partial class StimulusView
    {
        protected virtual Color ListFieldErrorText { get; } = new Color32(0xC0, 0x1C, 0x1C, 0xFF);

        // Index, Up, Down, Copy and Delete are narrow; the value (slot 1) takes the rest
        private const WidgetFactory.ContainerConfig ListItemRowConfig =
            WidgetFactory.ContainerConfig.Small_Element_0 |
            WidgetFactory.ContainerConfig.Small_Element_2 |
            WidgetFactory.ContainerConfig.Small_Element_3 |
            WidgetFactory.ContainerConfig.Small_Element_4 |
            WidgetFactory.ContainerConfig.Small_Element_5 |
            WidgetFactory.ContainerConfig.Spacing_Internal |
            WidgetFactory.ContainerConfig.Height_60px |
            WidgetFactory.ContainerConfig.Smallness_5;

        private void SpawnListField(
            IPropertyGroup propertyGroup,
            PropertyInfo property,
            Transform parentTransform)
        {
            ListFieldDisplayAttribute attribute =
                propertyGroup.GetListFieldAttribute(property.GetInputFieldName());
            TupleListLayout layout = property.IsTupleListField()
                ? TupleListLayout.Get(propertyGroup, property)
                : null;

            GameObject header = widgetFactory.GetContainerWidget(
                config: WidgetFactory.ContainerConfig.Config_Normal_Even,
                parent: parentTransform.gameObject,
                slots: 2);

            LayoutElement layoutElement = header.AddComponent<LayoutElement>();
            layoutElement.minHeight = 60;

            if (attribute == null || (property.IsTupleListField() && layout == null))
            {
                widgetFactory.CreateLabelWidget(
                    parent: header,
                    text: "NULL",
                    alignment: TextAnchor.MiddleLeft);
                return;
            }

            if (property.GetValue(propertyGroup) == null)
            {
                property.SetValue(propertyGroup, layout != null ? layout.CreateInitialList() : attribute.GetInitialValue());
            }

            new ListFieldEditor(this, propertyGroup, property, attribute, layout, header, parentTransform).Rebuild();
        }

        /// <summary>What a value cell edits: its type, its range and its postfix.</summary>
        private readonly struct ItemSpec
        {
            public readonly Type type;
            public readonly double minimum;
            public readonly double maximum;
            public readonly string postfix;

            private ItemSpec(Type type, double minimum, double maximum, string postfix)
            {
                this.type = type;
                this.minimum = minimum;
                this.maximum = maximum;
                this.postfix = postfix ?? "";
            }

            public static ItemSpec From(ListFieldDisplayAttribute attribute)
            {
                switch (attribute)
                {
                    case IntListFieldDisplayAttribute intAttribute:
                        return new ItemSpec(typeof(int), intAttribute.minimum, intAttribute.maximum, intAttribute.postfix);
                    case DoubleListFieldDisplayAttribute doubleAttribute:
                        return new ItemSpec(typeof(double), doubleAttribute.minimum, doubleAttribute.maximum, doubleAttribute.postfix);
                    default:
                        return new ItemSpec(attribute.ElementType, double.MinValue, double.MaxValue, "");
                }
            }

            public static ItemSpec From(TupleColumnAttribute column)
            {
                switch (column)
                {
                    case IntTupleColumnAttribute intColumn:
                        return new ItemSpec(typeof(int), intColumn.minimum, intColumn.maximum, intColumn.postfix);
                    case DoubleTupleColumnAttribute doubleColumn:
                        return new ItemSpec(typeof(double), doubleColumn.minimum, doubleColumn.maximum, doubleColumn.postfix);
                    default:
                        return new ItemSpec(column.ElementType, double.MinValue, double.MaxValue, column.Postfix);
                }
            }
        }

        private sealed class ListFieldEditor
        {
            private readonly StimulusView view;
            private readonly IPropertyGroup propertyGroup;
            private readonly PropertyInfo property;
            private readonly ListFieldDisplayAttribute attribute;
            private readonly TupleListLayout layout;
            private readonly string fieldName;
            private readonly GameObject header;
            private readonly Transform parentTransform;
            private readonly Text summary;
            private readonly Color summaryColor;
            private readonly Button addButton;
            private readonly List<GameObject> rows = new List<GameObject>();

            // Destroy() is deferred to the end of the frame, so replaced rows stay live until then.
            // Each row's callbacks carry the generation they were built in and do nothing once
            // a rebuild has replaced them, so a late event cannot act on a stale index.
            private int generation;

            private WidgetFactory WidgetFactory => view.widgetFactory;
            private IList List => (IList)property.GetValue(propertyGroup);

            public ListFieldEditor(
                StimulusView view,
                IPropertyGroup propertyGroup,
                PropertyInfo property,
                ListFieldDisplayAttribute attribute,
                TupleListLayout layout,
                GameObject header,
                Transform parentTransform)
            {
                this.view = view;
                this.propertyGroup = propertyGroup;
                this.property = property;
                this.attribute = attribute;
                this.layout = layout;
                this.header = header;
                this.parentTransform = parentTransform;
                fieldName = property.GetInputFieldName();

                WidgetFactory.CreateLabelWidget(
                    parent: header,
                    text: $"{attribute.displayTitle}:",
                    alignment: TextAnchor.MiddleLeft);

                GameObject controls = WidgetFactory.GetContainerWidget(
                    config: WidgetFactory.ContainerConfig.Config_Normal_Even,
                    parent: header,
                    slots: 2,
                    siblingIndex: 1,
                    parentSlots: 2);

                summary = WidgetFactory.CreateLabelWidget(
                    parent: controls,
                    alignment: TextAnchor.MiddleLeft,
                    index: 0).GetComponent<Text>();
                summaryColor = summary.color;

                addButton = WidgetFactory.CreateButtonWidget(
                    parent: controls,
                    text: "Add",
                    onClick: () => Edit(generation, list => list.Add(CreateNewItem())),
                    index: 1).GetComponent<Button>();
            }

            public void Rebuild()
            {
                generation++;

                foreach (GameObject row in rows)
                {
                    Object.Destroy(row);
                }
                rows.Clear();

                IList list = List;
                int siblingIndex = header.transform.GetSiblingIndex();

                void Place(GameObject row)
                {
                    row.transform.SetSiblingIndex(++siblingIndex);
                    rows.Add(row);
                }

                if (propertyGroup.TryGetUnparsedListValue(fieldName, out JsonValue original))
                {
                    Place(SpawnUnparsedNotice(original));
                }

                if (layout != null && list.Count > 0)
                {
                    Place(SpawnColumnTitles());
                }

                for (int i = 0; i < list.Count; i++)
                {
                    Place(SpawnItemRow(list, i));
                }

                UpdateSummary();
            }

            private object CreateNewItem() => layout != null ? layout.CreateNewRow() : attribute.NewItemValue;

            private bool IsListValid(IList list) =>
                layout != null ? layout.IsListValid(list) : attribute.IsListValid(list);

            private GameObject SpawnRow(WidgetFactory.ContainerConfig config, int slots)
            {
                GameObject row = WidgetFactory.GetContainerWidget(
                    config: config,
                    parent: parentTransform.gameObject,
                    slots: slots);

                LayoutElement layoutElement = row.AddComponent<LayoutElement>();
                layoutElement.minHeight = 60;

                return row;
            }

            private GameObject SpawnUnparsedNotice(JsonValue original)
            {
                GameObject row = SpawnRow(WidgetFactory.ContainerConfig.Config_Normal_Even, 1);

                string originalText = original.ToString();
                if (originalText.Length > 60)
                {
                    originalText = originalText.Substring(0, 57) + "...";
                }

                Text notice = WidgetFactory.CreateLabelWidget(
                    parent: row,
                    text: $"The saved value {originalText} could not be read. It is kept unchanged until this list is edited.",
                    alignment: TextAnchor.MiddleLeft).GetComponent<Text>();
                notice.color = view.ListFieldErrorText;

                return row;
            }

            // The value slot of an item row, split into one slot per tuple column
            private GameObject SpawnCellContainer(GameObject row) =>
                WidgetFactory.GetContainerWidget(
                    config: WidgetFactory.ContainerConfig.Config_Normal_Even,
                    parent: row,
                    slots: layout.Columns.Count,
                    siblingIndex: 1);

            private GameObject SpawnColumnTitles()
            {
                GameObject row = SpawnRow(ListItemRowConfig, 6);
                GameObject cells = SpawnCellContainer(row);

                for (int i = 0; i < layout.Columns.Count; i++)
                {
                    WidgetFactory.CreateLabelWidget(
                        parent: cells,
                        text: layout.Columns[i].displayTitle,
                        alignment: TextAnchor.MiddleLeft,
                        index: i);
                }

                return row;
            }

            private GameObject SpawnItemRow(IList list, int index)
            {
                GameObject row = SpawnRow(ListItemRowConfig, 6);

                WidgetFactory.CreateLabelWidget(
                    parent: row,
                    text: $"{index + 1}",
                    alignment: TextAnchor.MiddleRight,
                    index: 0);

                int rowGeneration = generation;

                if (layout == null)
                {
                    SpawnValueCell(row, 1, ItemSpec.From(attribute), rowGeneration,
                        get: () => List[index],
                        set: value => List[index] = value);
                }
                else
                {
                    GameObject cells = SpawnCellContainer(row);
                    for (int column = 0; column < layout.Columns.Count; column++)
                    {
                        int c = column;
                        SpawnValueCell(cells, c, ItemSpec.From(layout.Columns[c]), rowGeneration,
                            get: () => layout.GetItem(List[index], c),
                            set: value => List[index] = layout.WithItem(List[index], c, value));
                    }
                }

                SpawnRowButton(row, rowGeneration, 2, "▲", index > 0,
                    l => Swap(l, index, index - 1));
                SpawnRowButton(row, rowGeneration, 3, "▼", index < list.Count - 1,
                    l => Swap(l, index, index + 1));
                SpawnRowButton(row, rowGeneration, 4, "Copy", list.Count < attribute.maximumCount,
                    l => l.Insert(index + 1, l[index]));
                SpawnRowButton(row, rowGeneration, 5, "✕", list.Count > attribute.minimumCount,
                    l => l.RemoveAt(index),
                    WidgetFactory.ButtonFormattingOptions.Config_DeleteButton);

                return row;
            }

            private void SpawnRowButton(
                GameObject row,
                int rowGeneration,
                int slot,
                string text,
                bool interactable,
                Action<IList> edit,
                WidgetFactory.ButtonFormattingOptions options = WidgetFactory.ButtonFormattingOptions.Config_ListEntry)
            {
                Button button = WidgetFactory.CreateButtonWidget(
                    parent: row,
                    text: text,
                    onClick: () => Edit(rowGeneration, edit),
                    buttonOptions: options,
                    index: slot).GetComponent<Button>();

                button.interactable = interactable;
            }

            private void SpawnValueCell(
                GameObject parent,
                int slot,
                ItemSpec spec,
                int rowGeneration,
                Func<object> get,
                Action<object> set)
            {
                void Set(object value)
                {
                    if (rowGeneration != generation)
                    {
                        return;
                    }

                    set(value);
                    propertyGroup.ClearUnparsedListValue(fieldName);
                    UpdateSummary();
                }

                if (spec.type == typeof(int))
                {
                    WidgetFactory.CreateInputFieldWidget(
                        parent: parent,
                        formattingOptions: WidgetFactory.EditorWidgetFormattingOptions.Config_ValueEntry,
                        text: FormatInt(get()),
                        postfixText: spec.postfix,
                        contentType: InputField.ContentType.IntegerNumber,
                        newOnEndEdit: (input, inputField) =>
                        {
                            if (rowGeneration != generation)
                            {
                                return;
                            }

                            if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
                            {
                                Set((int)GeneralMath.Clamp(value, spec.minimum, spec.maximum));
                            }

                            //Show what was stored: the clamped value, or the old one if unparseable
                            inputField.text = FormatInt(get());
                        },
                        index: slot);
                }
                else if (spec.type == typeof(double))
                {
                    WidgetFactory.CreateInputFieldWidget(
                        parent: parent,
                        formattingOptions: WidgetFactory.EditorWidgetFormattingOptions.Config_ValueEntry,
                        text: FormatDouble(get()),
                        postfixText: spec.postfix,
                        contentType: InputField.ContentType.DecimalNumber,
                        newOnEndEdit: (input, inputField) =>
                        {
                            if (rowGeneration != generation)
                            {
                                return;
                            }

                            if (double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                            {
                                Set(GeneralMath.Clamp(value, spec.minimum, spec.maximum));
                            }

                            inputField.text = FormatDouble(get());
                        },
                        index: slot);
                }
                else if (spec.type == typeof(string))
                {
                    WidgetFactory.CreateInputFieldWidget(
                        parent: parent,
                        formattingOptions: WidgetFactory.EditorWidgetFormattingOptions.Config_ValueEntry,
                        text: get() as string ?? "",
                        contentType: InputField.ContentType.Standard,
                        onEndEdit: input => Set(input?.Trim()?.Replace("\r", "") ?? ""),
                        index: slot);
                }
                else if (spec.type == typeof(bool))
                {
                    WidgetFactory.CreateToggleWidget(
                        parent: parent,
                        value: Convert.ToBoolean(get()),
                        onValueChanged: value => Set(value),
                        index: slot);
                }
                else
                {
                    Debug.LogError($"Unexpected list item type: {spec.type}");
                }
            }

            private void Edit(int rowGeneration, Action<IList> edit)
            {
                if (rowGeneration != generation)
                {
                    return;
                }

                edit(List);
                propertyGroup.ClearUnparsedListValue(fieldName);
                Rebuild();
            }

            private void UpdateSummary()
            {
                IList list = List;
                bool unparsed = propertyGroup.TryGetUnparsedListValue(fieldName, out JsonValue _);

                summary.text = unparsed
                    ? "Unreadable"
                    : (list.Count == 1 ? "1 item" : $"{list.Count} items") + GetCountRangeText();

                summary.color = unparsed || !IsListValid(list) ? view.ListFieldErrorText : summaryColor;
                addButton.interactable = list.Count < attribute.maximumCount;
            }

            private string GetCountRangeText()
            {
                bool hasMinimum = attribute.minimumCount > 0;
                bool hasMaximum = attribute.maximumCount < int.MaxValue;

                if (hasMinimum && hasMaximum)
                {
                    return attribute.minimumCount == attribute.maximumCount
                        ? $" (exactly {attribute.minimumCount})"
                        : $" ({attribute.minimumCount} to {attribute.maximumCount})";
                }

                if (hasMinimum)
                {
                    return $" (at least {attribute.minimumCount})";
                }

                if (hasMaximum)
                {
                    return $" (at most {attribute.maximumCount})";
                }

                return "";
            }

            private static string FormatInt(object value) =>
                Convert.ToInt32(value).ToString(CultureInfo.InvariantCulture);

            private static string FormatDouble(object value) =>
                Convert.ToDouble(value).ToString("R", CultureInfo.InvariantCulture);

            private static void Swap(IList list, int first, int second)
            {
                object temp = list[first];
                list[first] = list[second];
                list[second] = temp;
            }
        }
    }
}
