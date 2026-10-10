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
    /// Editor for List&lt;T&gt; input fields (see <see cref="ListFieldDisplayAttribute"/>).
    ///
    /// A header row (title, item count with its allowed range, Add) is followed by one row per
    /// item: [index] [value] [Up] [Down] [Copy] [Delete]. Rows are ordinary 60px option rows
    /// placed directly in the property frame, so they line up with the scalar fields around them.
    ///
    /// Every structural edit (add, move, copy, delete) rebuilds this field's item rows itself,
    /// so the index each row acts on is always current. It does not depend on the property
    /// group's respawn callback, which is null inside list items, inline groups and controlled
    /// parameter templates.
    ///
    /// Bounds behave as for scalar fields: typed values are clamped to the item range, and Add,
    /// Copy and Delete are disabled at the count limits. A list loaded outside its bounds is left
    /// as-is and its count is shown in the error color.
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

            GameObject header = widgetFactory.GetContainerWidget(
                config: WidgetFactory.ContainerConfig.Config_Normal_Even,
                parent: parentTransform.gameObject,
                slots: 2);

            LayoutElement layoutElement = header.AddComponent<LayoutElement>();
            layoutElement.minHeight = 60;

            if (attribute == null)
            {
                widgetFactory.CreateLabelWidget(
                    parent: header,
                    text: "NULL",
                    alignment: TextAnchor.MiddleLeft);
                return;
            }

            if (property.GetValue(propertyGroup) == null)
            {
                property.SetValue(propertyGroup, attribute.GetInitialValue());
            }

            new ListFieldEditor(this, propertyGroup, property, attribute, header, parentTransform).Rebuild();
        }

        private sealed class ListFieldEditor
        {
            private readonly StimulusView view;
            private readonly IPropertyGroup propertyGroup;
            private readonly PropertyInfo property;
            private readonly ListFieldDisplayAttribute attribute;
            private readonly string fieldName;
            private readonly GameObject header;
            private readonly Transform parentTransform;
            private readonly Text summary;
            private readonly Color summaryColor;
            private readonly Button addButton;
            private readonly List<GameObject> rows = new List<GameObject>();

            private WidgetFactory WidgetFactory => view.widgetFactory;
            private IList List => (IList)property.GetValue(propertyGroup);

            public ListFieldEditor(
                StimulusView view,
                IPropertyGroup propertyGroup,
                PropertyInfo property,
                ListFieldDisplayAttribute attribute,
                GameObject header,
                Transform parentTransform)
            {
                this.view = view;
                this.propertyGroup = propertyGroup;
                this.property = property;
                this.attribute = attribute;
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
                    onClick: () => Edit(list => list.Add(attribute.NewItemValue)),
                    index: 1).GetComponent<Button>();
            }

            public void Rebuild()
            {
                foreach (GameObject row in rows)
                {
                    Object.Destroy(row);
                }
                rows.Clear();

                IList list = List;
                int siblingIndex = header.transform.GetSiblingIndex();

                if (propertyGroup.TryGetUnparsedListValue(fieldName, out JsonValue original))
                {
                    GameObject notice = SpawnUnparsedNotice(original);
                    notice.transform.SetSiblingIndex(++siblingIndex);
                    rows.Add(notice);
                }

                for (int i = 0; i < list.Count; i++)
                {
                    GameObject row = SpawnItemRow(list, i);
                    row.transform.SetSiblingIndex(++siblingIndex);
                    rows.Add(row);
                }

                UpdateSummary();
            }

            private GameObject SpawnUnparsedNotice(JsonValue original)
            {
                GameObject row = WidgetFactory.GetContainerWidget(
                    config: WidgetFactory.ContainerConfig.Config_Normal_Even,
                    parent: parentTransform.gameObject,
                    slots: 1);

                LayoutElement layoutElement = row.AddComponent<LayoutElement>();
                layoutElement.minHeight = 60;

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

            private GameObject SpawnItemRow(IList list, int index)
            {
                GameObject row = WidgetFactory.GetContainerWidget(
                    config: ListItemRowConfig,
                    parent: parentTransform.gameObject,
                    slots: 6);

                LayoutElement layoutElement = row.AddComponent<LayoutElement>();
                layoutElement.minHeight = 60;

                WidgetFactory.CreateLabelWidget(
                    parent: row,
                    text: $"{index + 1}",
                    alignment: TextAnchor.MiddleRight,
                    index: 0);

                SpawnItemValueWidget(row, list, index);

                SpawnRowButton(row, 2, "▲", index > 0,
                    l => Swap(l, index, index - 1));
                SpawnRowButton(row, 3, "▼", index < list.Count - 1,
                    l => Swap(l, index, index + 1));
                SpawnRowButton(row, 4, "Copy", list.Count < attribute.maximumCount,
                    l => l.Insert(index + 1, l[index]));
                SpawnRowButton(row, 5, "✕", list.Count > attribute.minimumCount,
                    l => l.RemoveAt(index),
                    WidgetFactory.ButtonFormattingOptions.Config_DeleteButton);

                return row;
            }

            private void SpawnRowButton(
                GameObject row,
                int slot,
                string text,
                bool interactable,
                Action<IList> edit,
                WidgetFactory.ButtonFormattingOptions options = WidgetFactory.ButtonFormattingOptions.Config_ListEntry)
            {
                Button button = WidgetFactory.CreateButtonWidget(
                    parent: row,
                    text: text,
                    onClick: () => Edit(edit),
                    buttonOptions: options,
                    index: slot).GetComponent<Button>();

                button.interactable = interactable;
            }

            private void SpawnItemValueWidget(GameObject row, IList list, int index)
            {
                switch (attribute)
                {
                    case IntListFieldDisplayAttribute intAttribute:
                        WidgetFactory.CreateInputFieldWidget(
                            parent: row,
                            formattingOptions: WidgetFactory.EditorWidgetFormattingOptions.Config_ValueEntry,
                            text: Convert.ToInt32(list[index]).ToString(CultureInfo.InvariantCulture),
                            postfixText: intAttribute.postfix,
                            contentType: InputField.ContentType.IntegerNumber,
                            newOnEndEdit: (input, inputField) =>
                            {
                                if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
                                {
                                    value = GeneralMath.Clamp(value, intAttribute.minimum, intAttribute.maximum);
                                    SetItem(index, value);
                                }

                                //Show what was stored: the clamped value, or the old one if unparseable
                                inputField.text = Convert.ToInt32(List[index]).ToString(CultureInfo.InvariantCulture);
                            },
                            index: 1);
                        break;

                    case DoubleListFieldDisplayAttribute doubleAttribute:
                        WidgetFactory.CreateInputFieldWidget(
                            parent: row,
                            formattingOptions: WidgetFactory.EditorWidgetFormattingOptions.Config_ValueEntry,
                            text: FormatDouble(list[index]),
                            postfixText: doubleAttribute.postfix,
                            contentType: InputField.ContentType.DecimalNumber,
                            newOnEndEdit: (input, inputField) =>
                            {
                                if (double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                                {
                                    value = GeneralMath.Clamp(value, doubleAttribute.minimum, doubleAttribute.maximum);
                                    SetItem(index, value);
                                }

                                inputField.text = FormatDouble(List[index]);
                            },
                            index: 1);
                        break;

                    case StringListFieldDisplayAttribute _:
                        WidgetFactory.CreateInputFieldWidget(
                            parent: row,
                            formattingOptions: WidgetFactory.EditorWidgetFormattingOptions.Config_ValueEntry,
                            text: list[index] as string ?? "",
                            contentType: InputField.ContentType.Standard,
                            onEndEdit: input => SetItem(index, input?.Trim()?.Replace("\r", "") ?? ""),
                            index: 1);
                        break;

                    case BoolListFieldDisplayAttribute _:
                        WidgetFactory.CreateToggleWidget(
                            parent: row,
                            value: Convert.ToBoolean(list[index]),
                            onValueChanged: value => SetItem(index, value),
                            index: 1);
                        break;

                    default:
                        Debug.LogError($"Unexpected list attribute: {attribute.GetType()}");
                        break;
                }
            }

            private void SetItem(int index, object value)
            {
                List[index] = value;
                propertyGroup.ClearUnparsedListValue(fieldName);
                UpdateSummary();
            }

            private void Edit(Action<IList> edit)
            {
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

                summary.color = unparsed || !attribute.IsListValid(list) ? view.ListFieldErrorText : summaryColor;
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
