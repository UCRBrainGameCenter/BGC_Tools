using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using BGC.UI.Dialogs;

using Object = UnityEngine.Object;

namespace BGC.Parameters.View
{
    /// <summary>
    /// Editor for [PropertyGroupList] lists of property groups, in the same style as the list-field
    /// editor (StimulusView.ListFields.cs).
    ///
    /// A header row (title, item count, Add) is followed by one card per item: a row with its
    /// number, a dropdown to change its type, and buttons to move, duplicate or delete it; a row
    /// to rename it; then (unless the list shows titles only) the item's own properties.
    /// Add appends an item of the first listed type, named after the type. Changing an item's
    /// type replaces it with a new item of that type, as the dropdowns of other property groups
    /// do: its settings are not carried over. Delete asks for confirmation, since it discards
    /// the item's settings.
    ///
    /// Names follow the existing rules: not blank, unique in the list, and no '.' (names are
    /// part of the paths that controllers use).
    ///
    /// Every structural edit rebuilds this list's cards itself, so each card acts on the right
    /// item; callbacks from cards a rebuild replaced do nothing.
    /// </summary>
    public abstract partial class StimulusView
    {
        private void SpawnPropertyGroupListEditor(
            IList propertyGroupList,
            PropertyInfo property,
            IPropertyGroup propertyContainer,
            Transform parentTransform,
            bool shallow)
        {
            new PropertyGroupListEditor(this, propertyGroupList, property, propertyContainer, parentTransform, shallow)
                .Rebuild();
        }

        /// <summary>The list's title: a [PropertyGroupTitle] on the property, else its field name in words.</summary>
        private static string GetPropertyGroupListTitle(PropertyInfo property) =>
            property.GetCustomAttribute<PropertyGroupTitleAttribute>()?.title ??
            Regex.Replace(property.GetGroupListSerializationName(), "(?<=[a-z])(?=[A-Z])", " ");

        private sealed class PropertyGroupListEditor
        {
            private readonly StimulusView view;
            private readonly IList list;
            private readonly PropertyInfo property;
            private readonly IPropertyGroup owner;
            private readonly Transform parentTransform;
            private readonly bool shallow;
            private readonly GameObject header;
            private readonly Text summary;
            private readonly List<GameObject> cards = new List<GameObject>();

            // See ListFieldEditor: replaced cards stay live until the end of the frame
            private int generation;

            private WidgetFactory WidgetFactory => view.widgetFactory;

            // The type dropdown and its info button share the row's wide slot
            private const WidgetFactory.ContainerConfig TypeSelectorConfig =
                WidgetFactory.ContainerConfig.Small_Element_1 |
                WidgetFactory.ContainerConfig.Spacing_Internal |
                WidgetFactory.ContainerConfig.Height_60px |
                WidgetFactory.ContainerConfig.Smallness_5;

            public PropertyGroupListEditor(
                StimulusView view,
                IList list,
                PropertyInfo property,
                IPropertyGroup owner,
                Transform parentTransform,
                bool shallow)
            {
                this.view = view;
                this.list = list;
                this.property = property;
                this.owner = owner;
                this.parentTransform = parentTransform;
                this.shallow = shallow;

                header = WidgetFactory.GetContainerWidget(
                    config: WidgetFactory.ContainerConfig.Config_Normal_Even,
                    parent: parentTransform.gameObject,
                    slots: 2);

                LayoutElement layoutElement = header.AddComponent<LayoutElement>();
                layoutElement.minHeight = 60;

                WidgetFactory.CreateLabelWidget(
                    parent: header,
                    text: $"{GetPropertyGroupListTitle(property)}:",
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

                WidgetFactory.CreateButtonWidget(
                    parent: controls,
                    text: "Add",
                    onClick: Add,
                    index: 1);
            }

            public void Rebuild()
            {
                generation++;

                foreach (GameObject card in cards)
                {
                    Object.Destroy(card);
                }
                cards.Clear();

                int siblingIndex = header.transform.GetSiblingIndex();
                for (int i = 0; i < list.Count; i++)
                {
                    GameObject card = SpawnCard(i);
                    card.transform.SetSiblingIndex(++siblingIndex);
                    cards.Add(card);
                }

                summary.text = list.Count == 1 ? "1 item" : $"{list.Count} items";
            }

            private GameObject SpawnCard(int index)
            {
                IPropertyGroup item = (IPropertyGroup)list[index];
                int cardGeneration = generation;

                PropertyListItemContainer card = view.SpawnPropertyListItem();
                card.transform.SetParent(parentTransform, false);

                //The type, name and info button are shown, and edited, in the rows below
                card.titleBox.gameObject.SetActive(false);

                GameObject row = WidgetFactory.GetContainerWidget(
                    config: ListItemRowConfig,
                    parent: card.propertyFrame,
                    slots: 6);

                LayoutElement layoutElement = row.AddComponent<LayoutElement>();
                layoutElement.minHeight = 60;

                WidgetFactory.CreateLabelWidget(
                    parent: row,
                    text: $"{index + 1}",
                    alignment: TextAnchor.MiddleRight,
                    index: 0);

                SpawnTypeSelector(row, item, index, cardGeneration);

                SpawnCardButton(row, cardGeneration, 2, "▲", index > 0, () => Swap(index, index - 1));
                SpawnCardButton(row, cardGeneration, 3, "▼", index < list.Count - 1, () => Swap(index, index + 1));
                SpawnCardButton(row, cardGeneration, 4, "Copy", true, () => Copy(index),
                    icon: WidgetFactory.DuplicateIcon);
                SpawnCardButton(row, cardGeneration, 5, "✕", true, () => ConfirmDelete(index),
                    WidgetFactory.ButtonFormattingOptions.Config_DeleteButton);

                GameObject nameRow = WidgetFactory.GetContainerWidget(
                    config: WidgetFactory.ContainerConfig.Config_Normal_Even,
                    parent: card.propertyFrame,
                    slots: 2);

                layoutElement = nameRow.AddComponent<LayoutElement>();
                layoutElement.minHeight = 60;

                WidgetFactory.CreateLabelWidget(
                    parent: nameRow,
                    text: "Name:",
                    alignment: TextAnchor.MiddleLeft);

                WidgetFactory.CreateInputFieldWidget(
                    parent: nameRow,
                    formattingOptions: WidgetFactory.EditorWidgetFormattingOptions.Config_StringEntry,
                    text: item.GetItemTitle(),
                    contentType: InputField.ContentType.Standard,
                    newOnEndEdit: (input, inputField) =>
                    {
                        if (cardGeneration != generation)
                        {
                            return;
                        }

                        Rename(item, input?.Trim());
                        inputField.text = item.GetItemTitle();
                    });

                if (!shallow)
                {
                    view.VisualizePropertyGroup(
                        propertyContainer: item,
                        parentTransform: card.propertyFrame.transform,
                        respawnPropertyGroupCallback: null,
                        spawningBehavior: SpawningBehavior.NestedInternal);
                }

                return card.gameObject;
            }

            /// <summary>The types an item can be: the list's addition types, plus the item's own if it isn't one</summary>
            private List<Type> GetItemTypes(IPropertyGroup item)
            {
                List<Type> types = property.GetListAdditionTypes().ToList();
                if (!types.Contains(item.GetType()))
                {
                    types.Add(item.GetType());
                }

                return types;
            }

            private void SpawnTypeSelector(GameObject row, IPropertyGroup item, int index, int cardGeneration)
            {
                List<Type> types = GetItemTypes(item);

                GameObject selector = WidgetFactory.GetContainerWidget(
                    config: TypeSelectorConfig,
                    parent: row,
                    slots: 2,
                    siblingIndex: 1,
                    parentSlots: 6);

                if (types.Count == 1)
                {
                    WidgetFactory.CreateLabelWidget(
                        parent: selector,
                        text: item.GetSelectionTitle(),
                        alignment: TextAnchor.MiddleLeft,
                        index: 0);
                }
                else
                {
                    WidgetFactory.CreateDropdownWidget(
                        parent: selector,
                        value: types.IndexOf(item.GetType()),
                        formattingOptions: WidgetFactory.EditorWidgetFormattingOptions.Config_StringEntry,
                        onValueChanged: (int value) =>
                        {
                            if (cardGeneration == generation)
                            {
                                ChangeType(index, types[value]);
                            }
                        },
                        optionList: types.Select(x => x.GetSelectionTitle()).ToList(),
                        index: 0);
                }

                string choiceInfo = item.GetType().GetChoiceInfoText();
                if (!string.IsNullOrEmpty(choiceInfo))
                {
                    WidgetFactory.CreateButtonWidget(
                        parent: selector,
                        text: "?",
                        onClick: () => ModalDialog.ShowSimpleModal(
                            mode: ModalDialog.Mode.Accept,
                            headerText: "Info",
                            bodyText: $"{item.GetSelectionTitle()}:\n{choiceInfo}"),
                        index: 1);
                }
            }

            private void SpawnCardButton(
                GameObject row,
                int cardGeneration,
                int slot,
                string text,
                bool interactable,
                Action action,
                WidgetFactory.ButtonFormattingOptions options = WidgetFactory.ButtonFormattingOptions.Config_ListEntry,
                Sprite icon = null)
            {
                Button button = WidgetFactory.CreateButtonWidget(
                    parent: row,
                    text: text,
                    onClick: () =>
                    {
                        if (cardGeneration == generation)
                        {
                            action();
                        }
                    },
                    buttonOptions: options,
                    index: slot,
                    icon: icon).GetComponent<Button>();

                button.interactable = interactable;
            }

            private void Add()
            {
                Type type = property.GetListAdditionTypes().FirstOrDefault();
                if (type == null)
                {
                    return;
                }

                type.Build(GetDefaultItemName(type), owner, list);
                Rebuild();
            }

            /// <summary>An unused name for a new item of this type: the type's title, without '.' (names cannot contain it)</summary>
            private string GetDefaultItemName(Type type, IPropertyGroup replacing = null) =>
                GetUniqueListItemName(type.GetSelectionTitle().Replace(".", ""), list, replacing);

            private void ChangeType(int index, Type type)
            {
                IPropertyGroup oldItem = (IPropertyGroup)list[index];
                if (oldItem.GetType() == type)
                {
                    return;
                }

                //An item still named after its type (as Add and duplicate name it) takes the new
                //type's name; a name the researcher chose is kept
                string oldDefaultName = oldItem.GetType().GetSelectionTitle().Replace(".", "");
                bool namedAfterType = Regex.IsMatch(
                    oldItem.GetItemTitle(),
                    $@"^{Regex.Escape(oldDefaultName)}( \(\d+\))?$");

                string name = namedAfterType ? GetDefaultItemName(type, oldItem) : oldItem.GetItemTitle();

                list.RemoveAt(index);
                IPropertyGroup newItem = type.Build(name, owner, list);
                list.RemoveAt(list.Count - 1);
                list.Insert(index, newItem);
                Rebuild();
            }

            private void Rename(IPropertyGroup item, string name)
            {
                if (name == item.GetItemTitle())
                {
                    return;
                }

                string problem =
                    string.IsNullOrEmpty(name) ? "Names cannot be blank." :
                    name.Contains(".") ? "Names cannot contain \".\"." :
                    list.Cast<IPropertyGroup>().Any(x => x != item && x.GetItemTitle() == name) ? $"\"{name}\" is already in use in this list." :
                    null;

                if (problem != null)
                {
                    ModalDialog.ShowSimpleModal(ModalDialog.Mode.Accept, headerText: "Rename", bodyText: problem);
                    return;
                }

                item.SetItemTitle(name);
            }

            private void Swap(int first, int second)
            {
                object temp = list[first];
                list[first] = list[second];
                list[second] = temp;
                Rebuild();
            }

            private void Copy(int index)
            {
                IPropertyGroup source = (IPropertyGroup)list[index];
                int requestGeneration = generation;

                // Controller assignments for controlled parameters are kept outside the item (e.g.
                // on the assessment), so a copy cannot take them along: say so before copying
                bool hasControlledParameters = source.GetAll<IControlled>().Any();

                if (!hasControlledParameters)
                {
                    InsertCopy(index, source);
                    return;
                }

                ModalDialog.ShowCustomSimpleModal(
                    headerText: "Copy",
                    bodyText: $"\"{source.GetItemTitle()}\" has controlled parameters. The copy keeps every " +
                        "setting except their controller settings, which start at their defaults: " +
                        "set them on the copy afterwards.",
                    buttonALabel: "Copy",
                    buttonBLabel: "Cancel",
                    buttonCLabel: null,
                    callback: response =>
                    {
                        if (response == ModalDialog.Response.A &&
                            requestGeneration == generation &&
                            list.Contains(source))
                        {
                            InsertCopy(list.IndexOf(source), source);
                        }
                    });
            }

            private void InsertCopy(int index, IPropertyGroup source)
            {
                IPropertyGroup copy = source.CloneOrphanListItem(property);
                if (copy == null)
                {
                    return;
                }

                copy.SetParent(owner);
                copy.SetItemTitle(GetUniqueListItemName(source.GetItemTitle(), list));
                list.Insert(index + 1, copy);
                Rebuild();
            }

            private void ConfirmDelete(int index)
            {
                IPropertyGroup item = (IPropertyGroup)list[index];
                int requestGeneration = generation;

                ModalDialog.ShowCustomSimpleModal(
                    headerText: "Delete",
                    bodyText: $"Delete \"{item.GetItemTitle()}\" and its settings?",
                    buttonALabel: "Delete",
                    buttonBLabel: "Cancel",
                    buttonCLabel: null,
                    callback: response =>
                    {
                        if (response == ModalDialog.Response.A &&
                            requestGeneration == generation &&
                            list.Contains(item))
                        {
                            list.Remove(item);
                            Rebuild();
                        }
                    });
            }
        }
    }
}
