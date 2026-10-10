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
    /// A header row (title, item count, Add) is followed by one card per item: its type and name,
    /// a row to rename, move, copy or delete it, and (unless the list shows titles only) the
    /// item's own properties. Add asks for the type and name, as before. Delete asks for
    /// confirmation, since it discards the item's settings.
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
                card.typeLabel.text = $"{item.GetSelectionTitle()}:";
                card.ChoiceInfoText = item.GetType().GetChoiceInfoText();

                //The name is shown, and edited, in the row below
                card.nameLabel.gameObject.SetActive(false);

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

                WidgetFactory.CreateInputFieldWidget(
                    parent: row,
                    formattingOptions: WidgetFactory.EditorWidgetFormattingOptions.Config_ValueEntry,
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
                    },
                    index: 1);

                SpawnCardButton(row, cardGeneration, 2, "▲", index > 0, () => Swap(index, index - 1));
                SpawnCardButton(row, cardGeneration, 3, "▼", index < list.Count - 1, () => Swap(index, index + 1));
                SpawnCardButton(row, cardGeneration, 4, "Copy", true, () => Copy(index));
                SpawnCardButton(row, cardGeneration, 5, "✕", true, () => ConfirmDelete(index),
                    WidgetFactory.ButtonFormattingOptions.Config_DeleteButton);

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

            private void SpawnCardButton(
                GameObject row,
                int cardGeneration,
                int slot,
                string text,
                bool interactable,
                Action action,
                WidgetFactory.ButtonFormattingOptions options = WidgetFactory.ButtonFormattingOptions.Config_ListEntry)
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
                    index: slot).GetComponent<Button>();

                button.interactable = interactable;
            }

            private void Add()
            {
                List<Type> types = property.GetListAdditionTypes().ToList();

                ModalDialog.ShowDropdownInputModal(
                    headerText: "Add",
                    primaryBodyText: "Select Item to Add",
                    secondaryBodyText: "Name of item to Add",
                    dropdownOptions: types.Select(x => x.GetSelectionTitle()),
                    inputCallback: (ModalDialog.Response response, int selectionIndex, string title) =>
                    {
                        if (response != ModalDialog.Response.Confirm)
                        {
                            return;
                        }

                        if (string.IsNullOrEmpty(title))
                        {
                            title = types[selectionIndex].GetSelectionTitle();
                        }

                        title = GetUniqueListItemName(title.Replace(".", ""), list);
                        types[selectionIndex].Build(title, owner, list);
                        Rebuild();
                    },
                    inputType: InputField.ContentType.Standard);
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
