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
    /// A header row (title, item count) is followed by one panel per item, then an add bar.
    /// Each panel has a darker header bar with the item's number, a dropdown to change its type,
    /// its type's info button, and buttons to move it up or down, duplicate it or (set apart)
    /// delete it; below that, indented, a Name row and (unless the list shows titles only) the
    /// item's own properties. The add bar is where a new item will appear: a type dropdown and
    /// "+ Add", or "+ Add <type>" for a list with one type. A new item is named after its type.
    /// Changing an item's type replaces it with a new item of that type, as the dropdowns of
    /// other property groups do: its settings are not carried over. Delete asks for
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

        // Each item is a panel, lighter than the group around it, with a dark header bar (number,
        // type, item buttons) over its settings
        protected virtual Color ListItemPanelBG { get; } = new Color32(0xA8, 0xA8, 0xA8, 0xFF);
        protected virtual Color ListItemHeaderBG { get; } = new Color32(0x50, 0x50, 0x50, 0xFF);
        protected virtual Color ListItemHeaderText { get; } = Color.white;

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
            private readonly GameObject addBar;
            private readonly Dropdown addTypeDropdown;
            private readonly List<Type> addTypes;
            private readonly List<GameObject> cards = new List<GameObject>();

            // See ListFieldEditor: replaced cards stay live until the end of the frame
            private int generation;

            private WidgetFactory WidgetFactory => view.widgetFactory;

            // Bar layout, in canvas units. Bar widgets are as tall as the editor's other rows (60):
            // shorter, and the widgets' 35pt captions no longer fit and are not drawn.
            private const float WidgetHeight = 60f;
            private const int BarVerticalPadding = 5;
            private const float BarHeight = WidgetHeight + 2 * BarVerticalPadding;
            private const float NumberWidth = 50f;
            private const float InfoWidth = 40f;
            private const float ItemButtonWidth = 120f;
            private const float AddButtonWidth = 200f;

            // The type (in the header bars and the add bar) has a fixed width, so the item buttons
            // line up at the right of every header
            private const float TypeWidth = 600f;

            // The item's settings are inset from its number by the nested-group step (40)
            private const int ContentsLeftPadding = 20 + 40;
            private const int ContentsRightPadding = 10;

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

                summary = WidgetFactory.CreateLabelWidget(
                    parent: header,
                    alignment: TextAnchor.MiddleLeft).GetComponent<Text>();

                //Add sits below the items, where the new item appears, beside the type it adds
                addTypes = property.GetListAdditionTypes().ToList();
                addBar = SpawnBar("AddBar", parentTransform, new RectOffset(0, 0, BarVerticalPadding, BarVerticalPadding));

                //It reads "+ Add [type]"
                if (addTypes.Count == 1)
                {
                    Size(WidgetFactory.CreateButtonWidget(
                            parent: addBar,
                            text: $"+ Add {addTypes[0].GetSelectionTitle()}",
                            onClick: () => Add(addTypes[0])),
                        AddButtonWidth + 10f + TypeWidth);
                }
                else if (addTypes.Count > 1)
                {
                    Size(WidgetFactory.CreateButtonWidget(
                            parent: addBar,
                            text: "+ Add",
                            onClick: () => Add(addTypes[addTypeDropdown.value])),
                        AddButtonWidth);

                    addTypeDropdown = Size(WidgetFactory.CreateDropdownWidget(
                            parent: addBar,
                            formattingOptions: WidgetFactory.EditorWidgetFormattingOptions.Config_StringEntry,
                            optionList: addTypes.Select(x => x.GetSelectionTitle()).ToList()),
                        TypeWidth).GetComponent<Dropdown>();
                }
                else
                {
                    addBar.SetActive(false);
                }
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

                addBar.transform.SetSiblingIndex(++siblingIndex);

                summary.text = list.Count == 1 ? "1 item" : $"{list.Count} items";
            }

            private GameObject SpawnCard(int index)
            {
                IPropertyGroup item = (IPropertyGroup)list[index];
                int cardGeneration = generation;

                PropertyListItemContainer card = view.SpawnPropertyListItem();
                card.transform.SetParent(parentTransform, false);

                Image panel = card.GetComponent<Image>();
                panel.color = view.ListItemPanelBG;

                //The type and name are shown, and edited, in the header bar and the Name row
                card.titleBox.gameObject.SetActive(false);

                VerticalLayoutGroup frameLayout = card.propertyFrame.GetComponent<VerticalLayoutGroup>();
                frameLayout.padding = new RectOffset(ContentsLeftPadding, ContentsRightPadding, 5, 10);

                SpawnHeaderBar(card, panel.sprite, item, index, cardGeneration);

                GameObject nameRow = WidgetFactory.GetContainerWidget(
                    config: WidgetFactory.ContainerConfig.Config_Normal_Even,
                    parent: card.propertyFrame,
                    slots: 2);

                LayoutElement layoutElement = nameRow.AddComponent<LayoutElement>();
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

            /// <summary>
            /// The card's header bar: number, type (a dropdown, or a label if the list has one type),
            /// the type's info button, then move up, move down, duplicate and, set apart, delete.
            /// </summary>
            private void SpawnHeaderBar(
                PropertyListItemContainer card,
                Sprite sprite,
                IPropertyGroup item,
                int index,
                int cardGeneration)
            {
                //The left padding clears the panel's rounded corner
                GameObject bar = SpawnBar("ItemHeader", card.transform,
                    new RectOffset(20, 10, BarVerticalPadding, BarVerticalPadding));
                bar.transform.SetSiblingIndex(0);

                Image background = bar.AddComponent<Image>();
                background.sprite = sprite;
                background.type = Image.Type.Sliced;
                background.color = view.ListItemHeaderBG;
                background.raycastTarget = false;

                Size(WidgetFactory.CreateLabelWidget(
                        parent: bar,
                        text: $"{index + 1}",
                        alignment: TextAnchor.MiddleLeft),
                    NumberWidth).GetComponent<Text>().color = view.ListItemHeaderText;

                List<Type> types = GetItemTypes(item);
                if (types.Count == 1)
                {
                    Size(WidgetFactory.CreateLabelWidget(
                            parent: bar,
                            text: item.GetSelectionTitle(),
                            alignment: TextAnchor.MiddleLeft),
                        TypeWidth).GetComponent<Text>().color = view.ListItemHeaderText;
                }
                else
                {
                    Size(WidgetFactory.CreateDropdownWidget(
                            parent: bar,
                            value: types.IndexOf(item.GetType()),
                            formattingOptions: WidgetFactory.EditorWidgetFormattingOptions.Config_StringEntry,
                            onValueChanged: (int value) =>
                            {
                                if (cardGeneration == generation)
                                {
                                    ChangeType(index, types[value]);
                                }
                            },
                            optionList: types.Select(x => x.GetSelectionTitle()).ToList()),
                        TypeWidth);
                }

                //The card's own (round) info button moves beside the type; it shows itself only for
                //documented types, and its message is titled with the type label. Its slot is kept
                //either way so the types line up from card to card.
                card.typeLabel.text = $"{item.GetSelectionTitle()}:";
                card.ChoiceInfoText = item.GetType().GetChoiceInfoText();

                GameObject infoSlot = Size(new GameObject("InfoSlot", typeof(RectTransform)), InfoWidth);
                infoSlot.transform.SetParent(bar.transform, false);

                RectTransform infoButton = (RectTransform)card.infoButton.transform;
                infoButton.SetParent(infoSlot.transform, false);
                infoButton.anchorMin = new Vector2(0.5f, 0.5f);
                infoButton.anchorMax = new Vector2(0.5f, 0.5f);
                infoButton.pivot = new Vector2(0.5f, 0.5f);
                infoButton.anchoredPosition = Vector2.zero;
                infoButton.sizeDelta = new Vector2(InfoWidth, InfoWidth);

                //The item buttons sit at the right
                Size(new GameObject("Spacer", typeof(RectTransform)), 0f, flexible: true)
                    .transform.SetParent(bar.transform, false);

                SpawnItemButton(bar, cardGeneration, "▲", index > 0, () => Swap(index, index - 1));
                SpawnItemButton(bar, cardGeneration, "▼", index < list.Count - 1, () => Swap(index, index + 1));
                SpawnItemButton(bar, cardGeneration, "Copy", true, () => Copy(index),
                    icon: WidgetFactory.DuplicateIcon);

                //Delete is set apart from the others
                Size(new GameObject("Gap", typeof(RectTransform)), 20f).transform.SetParent(bar.transform, false);

                SpawnItemButton(bar, cardGeneration, "✕", true, () => ConfirmDelete(index),
                    WidgetFactory.ButtonFormattingOptions.Config_DeleteButton);
            }

            /// <summary>A row that lays out its children left to right at the sizes Size gives them</summary>
            private static GameObject SpawnBar(string name, Transform parent, RectOffset padding)
            {
                GameObject bar = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup));
                bar.transform.SetParent(parent, false);

                HorizontalLayoutGroup layout = bar.GetComponent<HorizontalLayoutGroup>();
                layout.padding = padding;
                layout.spacing = 10;
                layout.childAlignment = TextAnchor.MiddleLeft;
                layout.childControlWidth = true;
                layout.childControlHeight = true;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;

                LayoutElement layoutElement = bar.AddComponent<LayoutElement>();
                layoutElement.minHeight = BarHeight;
                layoutElement.preferredHeight = BarHeight;

                return bar;
            }

            /// <summary>Fix a bar child's width (or let it take the spare width) and height</summary>
            private static GameObject Size(GameObject widget, float width, bool flexible = false)
            {
                if (!widget.TryGetComponent(out LayoutElement layoutElement))
                {
                    layoutElement = widget.AddComponent<LayoutElement>();
                }

                layoutElement.minWidth = width;
                layoutElement.preferredWidth = width;
                layoutElement.flexibleWidth = flexible ? 1f : 0f;
                layoutElement.minHeight = WidgetHeight;
                layoutElement.preferredHeight = WidgetHeight;

                return widget;
            }

            /// <summary>The types an item can be: the list's addition types, plus the item's own if it isn't one</summary>
            private List<Type> GetItemTypes(IPropertyGroup item)
            {
                List<Type> types = addTypes.ToList();
                if (!types.Contains(item.GetType()))
                {
                    types.Add(item.GetType());
                }

                return types;
            }

            private void SpawnItemButton(
                GameObject bar,
                int cardGeneration,
                string text,
                bool interactable,
                Action action,
                WidgetFactory.ButtonFormattingOptions options = WidgetFactory.ButtonFormattingOptions.Config_ListEntry,
                Sprite icon = null)
            {
                Button button = Size(WidgetFactory.CreateButtonWidget(
                        parent: bar,
                        text: text,
                        onClick: () =>
                        {
                            if (cardGeneration == generation)
                            {
                                action();
                            }
                        },
                        buttonOptions: options,
                        icon: icon),
                    ItemButtonWidth).GetComponent<Button>();

                button.interactable = interactable;

                //The button's own disabled tint is translucent, which barely shows on the dark header
                if (!interactable)
                {
                    button.gameObject.AddComponent<CanvasGroup>().alpha = 0.3f;
                }
            }

            private void Add(Type type)
            {
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
