using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using BGC.Mathematics;
using BGC.UI.Dialogs;

namespace BGC.Parameters.View
{
#pragma warning disable IDE0019 // Use pattern matching
    public enum SpawningBehavior
    {
        PropertyFrame = 0,
        NestedInternal,
        ShallowPropertyFrame,
        ShallowInternal,
        FlatInternal,
        ListTitlesOnly,
        NonRendered,
        MAX
    }

    public abstract class StimulusView : MonoBehaviour
    {
        [Header("Engine References")]
        [SerializeField]
        protected WidgetFactory widgetFactory = null;

        [Header("Prefabs")]
        [SerializeField]
        private GameObject propertyGroupPrefab = null;
        [SerializeField]
        private GameObject nestedPropertyGroupPrefab = null;
        [SerializeField]
        private GameObject flatPropertyGroupPrefab = null;
        [SerializeField]
        private GameObject nestedPropertyListItemPrefab = null;

        protected virtual Type FallbackSetupMethodsClass => null;

        #region Interface Colors

        protected virtual Color PropertyGroupDropdownNormalBG { get; } = Color.white;
        protected virtual Color PropertyGroupDropdownNormalText { get; } = Color.black;
        protected virtual Color PropertyGroupDropdownControlledBG { get; } = new Color32(0x19, 0x00, 0x48, 0xFF);
        protected virtual Color PropertyGroupDropdownControlledText { get; } = Color.white;

        #endregion Interface Colors

        protected void VisualizeGroupListTitles(
            IPropertyGroup propertyContainer,
            PropertyGroupContainer container)
        {

            foreach (Transform t in container.propertyFrame.transform)
            {
                if (container.titleBox != t)
                {
                    GameObject.Destroy(t.gameObject);
                }
            }

            foreach (PropertyInfo property in propertyContainer.GetType().GetSortedProperties())
            {
                if (property.GetCustomAttribute<PropertyGroupListAttribute>() != null)
                {
                    //Internal Property Group List

                    IList propertyGroupList = property.GetValue(propertyContainer) as IList;

                    if (propertyGroupList == null)
                    {
                        //Construct a new instance of the List
                        propertyGroupList = Activator.CreateInstance(property.PropertyType) as IList;
                        property.SetValue(propertyContainer, propertyGroupList);
                    }

                    if (propertyGroupList == null)
                    {
                        Debug.LogError($"Unable to construct PropertyGroupList: {property}");
                        continue;
                    }

                    SpawnPropertyGroupList(
                        propertyGroupList: propertyGroupList,
                        property: property,
                        propertyContainer: propertyContainer,
                        parentTransform: container.propertyFrame.transform,
                        spawningBehavior: SpawningBehavior.ShallowInternal);
                }
            }
        }

        protected void VisualizePropertyGroup(
            IPropertyGroup propertyContainer,
            Transform parentTransform,
            Action respawnPropertyGroupCallback,
            SpawningBehavior spawningBehavior = SpawningBehavior.PropertyFrame)
        {
            foreach (PropertyInfo property in propertyContainer.GetType().GetSortedProperties())
            {
                if (property.GetCustomAttribute<DisplayInputFieldAttribute>() != null)
                {
                    //Input field
                    GameObject baseWidget = widgetFactory.GetContainerWidget(
                        config: WidgetFactory.ContainerConfig.Config_Normal_Even,
                        parent: parentTransform.gameObject,
                        slots: 2);

                    LayoutElement layoutElement = baseWidget.AddComponent<LayoutElement>();
                    layoutElement.minHeight = 60;

                    PropertyInfo keyProperty = propertyContainer.GetMatchingKeyProperty(property);

                    if (keyProperty == null)
                    {
                        SpawnPropertyWidget(
                            propertyGroup: propertyContainer,
                            property: property,
                            baseWidget: baseWidget,
                            respawnPropertyGroupCallback: respawnPropertyGroupCallback);
                    }
                    else
                    {
                        SpawnKeyedPropertyWidget(
                            propertyGroup: propertyContainer,
                            valueProperty: property,
                            keyProperty: keyProperty,
                            baseWidget: baseWidget);
                    }
                }
                else if (property.GetCustomAttribute<DisplayOutputFieldKeyAttribute>() != null)
                {
                    //Output Field
                    GameObject baseWidget = widgetFactory.GetContainerWidget(
                        config: WidgetFactory.ContainerConfig.Config_Normal_Even,
                        parent: parentTransform.gameObject,
                        slots: 2);

                    LayoutElement layoutElement = baseWidget.AddComponent<LayoutElement>();
                    layoutElement.minHeight = 60;

                    SpawnKeyWidget(
                        propertyGroup: propertyContainer,
                        property: property,
                        baseWidget: baseWidget);
                }
                else if (property.GetCustomAttribute<PropertyGroupListAttribute>() != null)
                {
                    //Internal Property Group List
                    IList propertyGroupList = property.GetValue(propertyContainer) as IList;

                    if (propertyGroupList == null)
                    {
                        //Construct a new instance of the List
                        propertyGroupList = Activator.CreateInstance(property.PropertyType) as IList;
                        property.SetValue(propertyContainer, propertyGroupList);
                    }

                    if (propertyGroupList == null)
                    {
                        Debug.LogError($"Unable to construct PropertyGroupList: {property}");
                        continue;
                    }

                    SpawnPropertyGroupList(
                        propertyGroupList: propertyGroupList,
                        property: property,
                        propertyContainer: propertyContainer,
                        parentTransform: parentTransform,
                        spawningBehavior: spawningBehavior);
                }
                else if (typeof(IPropertyGroup).IsAssignableFrom(property.PropertyType))
                {
                    //Internal PropertyGroup

                    IPropertyGroup propertyGroup = property.GetValue(propertyContainer) as IPropertyGroup;

                    if (propertyGroup == null)
                    {
                        //Construct a new instance from first selection
                        propertyGroup = property.GetDefaultSelectionType().Build(propertyContainer, property);
                    }

                    if (propertyGroup == null)
                    {
                        Debug.LogError($"Unable to resolve PropertyGroup: {property}");
                        continue;
                    }

                    SpawnPropertyGroup(
                        propertyGroup: propertyGroup,
                        property: property,
                        propertyContainer: propertyContainer,
                        parentTransform: parentTransform,
                        spawningBehavior: spawningBehavior);
                }
            }
        }

        protected void VisualizePropertyGroupSeparated(
            IPropertyGroup propertyContainer,
            PropertyInfo property,
            Dropdown dropdown,
            Transform parentTransform)
        {
            dropdown.onValueChanged.RemoveAllListeners();
            dropdown.ClearOptions();

            if (!typeof(IPropertyGroup).IsAssignableFrom(property.PropertyType))
            {
                Debug.LogError($"Unable to assign IPropertyGroup to {property.PropertyType}");
                return;
            }

            IPropertyGroup propertyGroup = property.GetValue(propertyContainer) as IPropertyGroup;

            if (propertyGroup == null)
            {
                //Construct a new instance from first selection
                propertyGroup = property.GetDefaultSelectionType().Build(propertyContainer, property);
            }

            if (propertyGroup == null)
            {
                Debug.LogError($"Unable to resolve PropertyGroup: {property}");
            }


            List<Dropdown.OptionData> propertyGroupChoices = new List<Dropdown.OptionData>();
            Type[] propertyGroupChoiceTypes = property.GetSelectionTypes().ToArray();

            int currentDataIndex = 0;

            foreach (Type type in propertyGroupChoiceTypes)
            {
                propertyGroupChoices.Add(new Dropdown.OptionData(type.GetSelectionTitle()));

                if (type == propertyGroup.GetType())
                {
                    currentDataIndex = propertyGroupChoices.Count - 1;
                }
            }

            string title = property.GetGroupTitle();

            dropdown.options = propertyGroupChoices;
            dropdown.value = currentDataIndex;
            dropdown.RefreshShownValue();

            dropdown.onValueChanged.AddListener((int index) =>
            {
                IPropertyGroup newPropertyGroup = propertyGroupChoiceTypes[index].Build(propertyContainer, property);
                SpawnPropertyWidgets(
                    propertyGroup: newPropertyGroup,
                    container: parentTransform,
                    protectedWidget: null,
                    spawningBehavior: SpawningBehavior.PropertyFrame);
            });

            SpawnPropertyWidgets(
                propertyGroup: propertyGroup,
                container: parentTransform,
                protectedWidget: null,
                spawningBehavior: SpawningBehavior.PropertyFrame);
        }

        private void SpawnPropertyGroupList(
            IList propertyGroupList,
            PropertyInfo property,
            IPropertyGroup propertyContainer,
            Transform parentTransform,
            SpawningBehavior spawningBehavior)
        {
            bool shallow = false;
            bool rendered = true;

            switch (spawningBehavior)
            {
                case SpawningBehavior.PropertyFrame:
                case SpawningBehavior.NestedInternal:
                case SpawningBehavior.FlatInternal:
                    break;

                case SpawningBehavior.ListTitlesOnly:
                case SpawningBehavior.ShallowInternal:
                    shallow = true;
                    break;

                case SpawningBehavior.NonRendered:
                    rendered = false;
                    break;

                case SpawningBehavior.ShallowPropertyFrame:
                default:
                    Debug.LogError($"Unexpected SpawningBehavior: {spawningBehavior}");
                    return;
            }

            if (rendered)
            {
                //Display Add and Edit buttons at the top

                //Render list of FIXED children
                //If it's Shallow, print only names and Add/Edit Buttons
                //If it's Full, Print names (No class Dropdown) and all internal properties
                GameObject baseWidget = widgetFactory.GetContainerWidget(
                    config: WidgetFactory.ContainerConfig.Config_Normal_Even,
                    parent: parentTransform.gameObject,
                    slots: 4);

                LayoutElement layoutElement = baseWidget.AddComponent<LayoutElement>();
                layoutElement.minHeight = 60;

                widgetFactory.CreateButtonWidget(
                    parent: baseWidget,
                    text: "Add",
                    onClick: () =>
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

                                title = GetUniqueListItemName(title, propertyGroupList);

                                IPropertyGroup newPropertyGroup = types[selectionIndex].Build(title, propertyContainer, propertyGroupList);

                                if (shallow)
                                {
                                    PropertyListItemContainer propertyListItemContainer = SpawnPropertyListItem();

                                    propertyListItemContainer.transform.SetParent(parentTransform, false);
                                    propertyListItemContainer.typeLabel.text = $"{newPropertyGroup.GetSelectionTitle()}:";
                                    propertyListItemContainer.nameLabel.text = newPropertyGroup.GetItemTitle();
                                    propertyListItemContainer.ChoiceInfoText = newPropertyGroup.GetType().GetChoiceInfoText();
                                }
                                else
                                {
                                    PropertyListItemContainer propertyListItemContainer = SpawnPropertyListItem();

                                    propertyListItemContainer.transform.SetParent(parentTransform, false);
                                    propertyListItemContainer.typeLabel.text = $"{newPropertyGroup.GetSelectionTitle()}:";
                                    propertyListItemContainer.nameLabel.text = newPropertyGroup.GetItemTitle();
                                    propertyListItemContainer.ChoiceInfoText = newPropertyGroup.GetType().GetChoiceInfoText();

                                    VisualizePropertyGroup(
                                        propertyContainer: newPropertyGroup,
                                        parentTransform: propertyListItemContainer.propertyFrame.transform,
                                        respawnPropertyGroupCallback: null,
                                        spawningBehavior: SpawningBehavior.NestedInternal);
                                }
                            },
                            inputType: InputField.ContentType.Standard);
                    },
                    index: 2);

                widgetFactory.CreateButtonWidget(
                    parent: baseWidget,
                    text: "Edit",
                    onClick: () => ModalListDialog.ShowListEditModal(
                        headerText: "Edit",
                        itemList: propertyGroupList,
                        nameTranslator: ListItemNameTranslator,
                        nameValidator: ListItemNameValidator,
                        nameUpdater: ListItemNameUpdater,
                        callback: _ =>
                        {
                            //Refresh items
                            foreach (Transform t in parentTransform)
                            {
                                if (t.gameObject.GetComponent<PropertyListItemContainer>() == null)
                                {
                                    //Only destroy PropertyListItems
                                    continue;
                                }

                                GameObject.Destroy(t.gameObject);
                            }

                            RenderAllListItems(
                                shallow: shallow,
                                propertyGroupList: propertyGroupList,
                                parentTransform: parentTransform);
                        },
                        inputType: InputField.ContentType.Standard),
                    index: 3);

                RenderAllListItems(
                    shallow: shallow,
                    propertyGroupList: propertyGroupList,
                    parentTransform: parentTransform);
            }
        }

        private void RenderAllListItems(
            bool shallow,
            IList propertyGroupList,
            Transform parentTransform)
        {
            if (shallow)
            {
                //Shallow renderings only go one layer deep
                foreach (IPropertyGroup propertyGroup in propertyGroupList)
                {
                    PropertyListItemContainer propertyListItemContainer = SpawnPropertyListItem();

                    propertyListItemContainer.transform.SetParent(parentTransform, false);
                    propertyListItemContainer.typeLabel.text = $"{propertyGroup.GetSelectionTitle()}:";
                    propertyListItemContainer.nameLabel.text = propertyGroup.GetItemTitle();
                    propertyListItemContainer.ChoiceInfoText = propertyGroup.GetType().GetChoiceInfoText();
                }
            }
            else
            {
                //Full renderings recursively render children
                foreach (IPropertyGroup propertyGroup in propertyGroupList)
                {
                    PropertyListItemContainer propertyListItemContainer = SpawnPropertyListItem();

                    propertyListItemContainer.transform.SetParent(parentTransform, false);
                    propertyListItemContainer.typeLabel.text = $"{propertyGroup.GetSelectionTitle()}:";
                    propertyListItemContainer.nameLabel.text = propertyGroup.GetItemTitle();
                    propertyListItemContainer.ChoiceInfoText = propertyGroup.GetType().GetChoiceInfoText();

                    VisualizePropertyGroup(
                        propertyContainer: propertyGroup,
                        parentTransform: propertyListItemContainer.propertyFrame.transform,
                        respawnPropertyGroupCallback: null,
                        spawningBehavior: SpawningBehavior.NestedInternal);
                }
            }
        }

        protected void SpawnPropertyGroup(
            IPropertyGroup propertyGroup,
            PropertyInfo property,
            IPropertyGroup propertyContainer,
            Transform parentTransform,
            SpawningBehavior spawningBehavior)
        {
            PropertyGroupContainer propertyGroupContainer = null;
            string titlePostFix = "";
            SpawningBehavior childSpawningBehavior;
            bool shallow = false;
            bool rendered = true;

            switch (spawningBehavior)
            {
                case SpawningBehavior.PropertyFrame:
                    propertyGroupContainer = SpawnPropertyGroupContainer();
                    childSpawningBehavior = SpawningBehavior.NestedInternal;
                    break;

                case SpawningBehavior.ShallowPropertyFrame:
                    shallow = true;
                    propertyGroupContainer = SpawnPropertyGroupContainer();
                    childSpawningBehavior = SpawningBehavior.ListTitlesOnly;
                    break;

                case SpawningBehavior.NestedInternal:
                    titlePostFix = ":";
                    propertyGroupContainer = SpawnNestedPropertyGroupContainer();
                    childSpawningBehavior = SpawningBehavior.NestedInternal;
                    break;

                case SpawningBehavior.ShallowInternal:
                    shallow = true;
                    titlePostFix = ":";
                    propertyGroupContainer = SpawnNestedPropertyGroupContainer();
                    childSpawningBehavior = SpawningBehavior.NonRendered;
                    break;

                case SpawningBehavior.FlatInternal:
                    propertyGroupContainer = SpawnFlatPropertyGroupContainer();
                    childSpawningBehavior = SpawningBehavior.FlatInternal;
                    break;

                case SpawningBehavior.NonRendered:
                    rendered = false;
                    childSpawningBehavior = SpawningBehavior.NonRendered;
                    break;

                case SpawningBehavior.ListTitlesOnly:
                default:
                    Debug.LogError($"Unexpected SpawningBehavior: {spawningBehavior}");
                    return;
            }

            if (rendered)
            {
                //Build class-selection dropdown

                List<Dropdown.OptionData> propertyGroupChoices = new List<Dropdown.OptionData>();
                Type[] propertyGroupChoiceTypes = property.GetSelectionTypes().ToArray();

                int currentDataIndex = 0;

                foreach (Type type in propertyGroupChoiceTypes)
                {
                    propertyGroupChoices.Add(new Dropdown.OptionData(type.GetSelectionTitle()));

                    if (type == propertyGroup.GetType())
                    {
                        currentDataIndex = propertyGroupChoices.Count - 1;
                    }
                }

                string title = property.GetGroupTitle();

                propertyGroupContainer.transform.SetParent(parentTransform, false);
                propertyGroupContainer.label.text = $"{title}{titlePostFix}";
                propertyGroupContainer.GroupInfoText = property.GetGroupInfoText();
                propertyGroupContainer.ChoiceInfoText = propertyGroup.GetType().GetChoiceInfoText();
                propertyGroupContainer.options.options = propertyGroupChoices;
                propertyGroupContainer.options.value = currentDataIndex;
                propertyGroupContainer.options.RefreshShownValue();

                switch (propertyGroup.GetType().GetSelectionChoiceRenderingModifier())
                {
                    case ChoiceRenderingModifier.Normal:
                        propertyGroupContainer.options.GetComponent<Image>().color = PropertyGroupDropdownNormalBG;
                        propertyGroupContainer.options.captionText.color = PropertyGroupDropdownNormalText;
                        break;

                    case ChoiceRenderingModifier.Controlled:
                        propertyGroupContainer.options.GetComponent<Image>().color = PropertyGroupDropdownControlledBG;
                        propertyGroupContainer.options.captionText.color = PropertyGroupDropdownControlledText;
                        break;

                    default:
                        break;
                }

                propertyGroupContainer.options.onValueChanged.AddListener((int index) =>
                {
                    IPropertyGroup newPropertyGroup = propertyGroupChoiceTypes[index].Build(propertyContainer, property);

                    if (shallow)
                    {
                        newPropertyGroup.BuildPartialPropertyGroup();
                    }
                    else
                    {
                        SpawnPropertyWidgets(
                            propertyGroup: newPropertyGroup,
                            container: propertyGroupContainer.propertyFrame.transform,
                            protectedWidget: propertyGroupContainer.titleBox,
                            spawningBehavior: childSpawningBehavior);
                    }

                    propertyGroupContainer.ChoiceInfoText = newPropertyGroup.GetType().GetChoiceInfoText();

                    switch (newPropertyGroup.GetType().GetSelectionChoiceRenderingModifier())
                    {
                        case ChoiceRenderingModifier.Normal:
                            propertyGroupContainer.options.GetComponent<Image>().color = PropertyGroupDropdownNormalBG;
                            propertyGroupContainer.options.captionText.color = PropertyGroupDropdownNormalText;
                            break;

                        case ChoiceRenderingModifier.Controlled:
                            propertyGroupContainer.options.GetComponent<Image>().color = PropertyGroupDropdownControlledBG;
                            propertyGroupContainer.options.captionText.color = PropertyGroupDropdownControlledText;
                            break;

                        default:
                            break;
                    }
                });

                if (propertyGroupChoices.Count == 1 &&
                    (string.IsNullOrEmpty(propertyGroupChoices[0].text) || propertyGroupChoices[0].text == "Default"))
                {
                    //Hide selection dropdown if there's only one choice
                    propertyGroupContainer.options.gameObject.SetActive(false);
                }
            }

            if (shallow || !rendered)
            {
                propertyGroup.BuildPartialPropertyGroup();

                if (childSpawningBehavior == SpawningBehavior.ListTitlesOnly)
                {
                    VisualizeGroupListTitles(
                        propertyContainer: propertyGroup,
                        container: propertyGroupContainer);
                }
            }
            else
            {
                SpawnPropertyWidgets(
                    propertyGroup: propertyGroup,
                    container: propertyGroupContainer.propertyFrame.transform,
                    protectedWidget: propertyGroupContainer.titleBox,
                    spawningBehavior: childSpawningBehavior);
            }
        }

        protected void SpawnPropertyWidgets(
            IPropertyGroup propertyGroup,
            Transform container,
            Transform protectedWidget,
            SpawningBehavior spawningBehavior)
        {
            //Clear
            foreach (Transform t in container)
            {
                if (protectedWidget != t)
                {
                    GameObject.Destroy(t.gameObject);
                }
            }

            VisualizePropertyGroup(
                propertyContainer: propertyGroup,
                parentTransform: container,
                respawnPropertyGroupCallback: () => SpawnPropertyWidgets(propertyGroup, container, protectedWidget, spawningBehavior),
                spawningBehavior: spawningBehavior);
        }

        protected void SpawnParameterTemplateWidgets(
            ControlledParameterTemplate parameterTemplate,
            List<string> controllerTitles,
            List<(string path, int parameter)> controllerSelections,
            PropertyGroupContainer container)
        {
            //Clear
            foreach (Transform t in container.propertyFrame.transform)
            {
                if (container.titleBox != t)
                {
                    GameObject.Destroy(t.gameObject);
                }
            }

            int choiceIndex = 0;

            for (int i = 0; i < controllerSelections.Count; i++)
            {
                if (parameterTemplate.Controller == controllerSelections[i].path &&
                    parameterTemplate.ControllerParameter == controllerSelections[i].parameter)
                {
                    choiceIndex = i;
                }
            }

            //Handle parameter assignment
            GameObject baseWidget = widgetFactory.GetContainerWidget(
                config: WidgetFactory.ContainerConfig.Config_Normal_Even,
                parent: container.propertyFrame,
                slots: 2);

            LayoutElement layoutElement = baseWidget.AddComponent<LayoutElement>();
            layoutElement.minHeight = 60;

            widgetFactory.CreateLabelWidget(
                parent: baseWidget,
                text: "Controller:",
                alignment: TextAnchor.MiddleLeft);

            widgetFactory.CreateDropdownWidget(
                parent: baseWidget,
                value: choiceIndex,
                formattingOptions: WidgetFactory.EditorWidgetFormattingOptions.Config_StringEntry,
                onValueChanged: (int value) =>
                {
                    parameterTemplate.Controller = controllerSelections[value].path;
                    parameterTemplate.ControllerParameter = controllerSelections[value].parameter;
                },
                optionList: controllerTitles);

            VisualizePropertyGroup(
                propertyContainer: parameterTemplate,
                parentTransform: container.propertyFrame.transform,
                respawnPropertyGroupCallback: null,
                spawningBehavior: SpawningBehavior.NestedInternal);
        }

        private void SpawnKeyedPropertyWidget(
            IPropertyGroup propertyGroup,
            PropertyInfo valueProperty,
            PropertyInfo keyProperty,
            GameObject baseWidget)
        {
            string propertyLabel = valueProperty.GetInputFieldName();

            FieldDisplayAttribute attribute = propertyGroup.GetFieldDisplayAttribute(propertyLabel);

            if (attribute == null)
            {
                widgetFactory.CreateLabelWidget(
                    parent: baseWidget,
                    text: "NULL",
                    alignment: TextAnchor.MiddleLeft);
                return;
            }

            LabelSwapTrigger swapTrigger = widgetFactory.CreateTriggeredLabelWidget(
                parent: baseWidget,
                text: $"{attribute.displayTitle}:",
                alignment: TextAnchor.MiddleLeft).GetComponent<LabelSwapTrigger>();

            GameObject valueWidget = SpawnValueInputWidget(
                owningPropertyGroup: propertyGroup,
                property: valueProperty,
                attribute: attribute,
                baseWidget: baseWidget,
                respawnPropertyGroupCallback: null);

            GameObject keyWidget = widgetFactory.CreateInputFieldWidget(
                parent: baseWidget,
                formattingOptions: WidgetFactory.EditorWidgetFormattingOptions.Config_KeyEntry,
                text: (string)keyProperty.GetValue(propertyGroup),
                contentType: InputField.ContentType.Alphanumeric,
                onEndEdit: (string input) => SubmitKeyString(propertyGroup, keyProperty, input),
                index: 1);

            string keyValue = (string)keyProperty.GetValue(propertyGroup);

            bool keyMode = keyValue != null && keyValue != "";

            valueWidget.SetActive(!keyMode);
            keyWidget.SetActive(keyMode);

            swapTrigger.OnDoubleClick = () =>
            {
                bool valueMode = valueWidget.activeSelf;

                if (valueMode)
                {
                    keyProperty.SetValue(propertyGroup, keyWidget.GetComponent<InputField>().text);
                }
                else
                {
                    keyProperty.SetValue(propertyGroup, "");
                }

                valueWidget.SetActive(!valueMode);
                keyWidget.SetActive(valueMode);
            };
        }

        private void SpawnKeyWidget(
            IPropertyGroup propertyGroup,
            PropertyInfo property,
            GameObject baseWidget)
        {
            string propertyLabel = property.GetOutputKeyFieldName();

            FieldDisplayAttribute attribute = propertyGroup.GetFieldDisplayAttribute(propertyLabel);

            if (attribute == null)
            {
                widgetFactory.CreateLabelWidget(
                    parent: baseWidget,
                    text: "NULL",
                    alignment: TextAnchor.MiddleLeft);
                return;
            }

            widgetFactory.CreateLabelWidget(
                parent: baseWidget,
                text: $"{attribute.displayTitle}:",
                alignment: TextAnchor.MiddleLeft);

            widgetFactory.CreateInputFieldWidget(
                parent: baseWidget,
                formattingOptions: WidgetFactory.EditorWidgetFormattingOptions.Config_KeyEntry,
                text: (string)property.GetValue(propertyGroup),
                contentType: InputField.ContentType.Alphanumeric,
                onEndEdit: (string input) => SubmitKeyString(propertyGroup, property, input),
                index: 1);
        }

        public void SpawnPropertyWidget(
            IPropertyGroup propertyGroup,
            PropertyInfo property,
            GameObject baseWidget,
            Action respawnPropertyGroupCallback)
        {
            string propertyLabel = property.GetInputFieldName();

            FieldDisplayAttribute attribute = propertyGroup.GetFieldDisplayAttribute(propertyLabel);

            if (attribute == null)
            {
                widgetFactory.CreateLabelWidget(
                    parent: baseWidget,
                    text: "NULL",
                    alignment: TextAnchor.MiddleLeft);
                return;
            }

            widgetFactory.CreateLabelWidget(
                parent: baseWidget,
                text: $"{attribute.displayTitle}:",
                alignment: TextAnchor.MiddleLeft);

            if (attribute is ScriptFieldDisplayAttribute)
            {
                GameObject scriptBaseWidget = widgetFactory.GetContainerWidget(
                    config: WidgetFactory.ContainerConfig.Config_Normal_Even,
                    parent: baseWidget.transform.parent.gameObject,
                    slots: 1);

                LayoutElement layoutElement = scriptBaseWidget.AddComponent<LayoutElement>();
                layoutElement.minHeight = 800;

                widgetFactory.CreateScriptInputFieldWidget(
                    parent: scriptBaseWidget,
                    text: (string)property.GetValue(propertyGroup),
                    onEndEdit: input => SubmitString(propertyGroup, property, input),
                    index: 0);
            }
            else if (attribute is MultiLineStringFieldDisplayAttribute)
            {
                GameObject multilineBaseWidget = widgetFactory.GetContainerWidget(
                    config: WidgetFactory.ContainerConfig.Config_Normal_Even,
                    parent: baseWidget.transform.parent.gameObject,
                    slots: 1);

                LayoutElement layoutElement = multilineBaseWidget.AddComponent<LayoutElement>();
                layoutElement.minHeight = 400;

                widgetFactory.CreateLargeInputFieldWidget(
                    parent: multilineBaseWidget,
                    text: (string)property.GetValue(propertyGroup),
                    onEndEdit: input => SubmitString(propertyGroup, property, input),
                    index: 0);
            }
            else
            {
                SpawnValueInputWidget(
                    owningPropertyGroup: propertyGroup,
                    property: property,
                    attribute: attribute,
                    baseWidget: baseWidget,
                    respawnPropertyGroupCallback: respawnPropertyGroupCallback);
            }
        }

        private GameObject SpawnValueInputWidget(
            IPropertyGroup owningPropertyGroup,
            PropertyInfo property,
            FieldDisplayAttribute attribute,
            GameObject baseWidget,
            Action respawnPropertyGroupCallback,
            IPropertyGroup concretePropertyGroup = null)
        {
            switch (attribute)
            {
                case DoubleFieldDisplayAttribute doubleAtt:
                    return widgetFactory.CreateInputFieldWidget(
                        parent: baseWidget,
                        formattingOptions: WidgetFactory.EditorWidgetFormattingOptions.Config_ValueEntry,
                        text: Convert.ToSingle(property.GetValue(owningPropertyGroup)),
                        postfixText: doubleAtt.postfix,
                        contentType: InputField.ContentType.DecimalNumber,
                        newOnEndEdit: (string input, InputField inputField) =>
                            SubmitDouble(owningPropertyGroup, property, doubleAtt, inputField, input),
                        index: 1);

                case IntFieldDisplayAttribute intAtt:
                    return widgetFactory.CreateInputFieldWidget(
                        parent: baseWidget,
                        formattingOptions: WidgetFactory.EditorWidgetFormattingOptions.Config_ValueEntry,
                        text: Convert.ToInt32(property.GetValue(owningPropertyGroup)),
                        postfixText: intAtt.postfix,
                        contentType: InputField.ContentType.IntegerNumber,
                        newOnEndEdit: (string input, InputField inputField) =>
                            SubmitInt(owningPropertyGroup, property, intAtt, inputField, input),
                        index: 1);

                case StringFieldDisplayAttribute _:
                    return widgetFactory.CreateInputFieldWidget(
                        parent: baseWidget,
                        formattingOptions: WidgetFactory.EditorWidgetFormattingOptions.Config_ValueEntry,
                        text: (string)property.GetValue(owningPropertyGroup),
                        contentType: InputField.ContentType.Standard,
                        onEndEdit: input => SubmitString(owningPropertyGroup, property, input),
                        index: 1);

                case ScriptFieldDisplayAttribute _:
                    Debug.LogError($"Not the proper way to spawn a script field!");
                    return widgetFactory.CreateLabelWidget(
                        parent: baseWidget,
                        text: "NULL",
                        alignment: TextAnchor.MiddleLeft,
                        index: 1);

                case MultiLineStringFieldDisplayAttribute _:
                    Debug.LogError($"Not the proper way to spawn a script field!");
                    return widgetFactory.CreateLargeInputFieldWidget(
                        parent: baseWidget,
                        text: (string)property.GetValue(owningPropertyGroup),
                        onEndEdit: input => SubmitString(owningPropertyGroup, property, input),
                        index: 1);

                case BoolDisplayAttribute _:
                    return widgetFactory.CreateToggleWidget(
                        parent: baseWidget,
                        value: Convert.ToBoolean(property.GetValue(owningPropertyGroup)),
                        onValueChanged: value => SubmitBool(owningPropertyGroup, property, value));

                case EnumDropdownDisplayAttribute enumAtt:
                    {
                        //Check local class first
                        Type owningType = owningPropertyGroup.GetType();
                        MethodInfo choiceListMethod = null;
                        for (Type currentType = owningType; currentType != null && choiceListMethod == null; currentType = currentType.BaseType)
                        {
                            choiceListMethod = currentType.GetMethod(enumAtt.choiceListMethodName);
                        }

                        if (choiceListMethod == null && concretePropertyGroup != null)
                        {
                            //Search using matching concrete property group
                            owningType = concretePropertyGroup.GetType();

                            for (Type currentType = owningType; currentType != null && choiceListMethod == null; currentType = currentType.BaseType)
                            {
                                choiceListMethod = currentType.GetMethod(enumAtt.choiceListMethodName);
                            }
                        }

                        if (choiceListMethod == null)
                        {
                            //If local class check fails, try from SetupMethods
                            choiceListMethod = typeof(SetupMethods).GetMethod(enumAtt.choiceListMethodName);
                        }

                        if (choiceListMethod == null && FallbackSetupMethodsClass != null)
                        {
                            //If local class check fails, try from SetupMethods
                            choiceListMethod = FallbackSetupMethodsClass.GetMethod(enumAtt.choiceListMethodName);
                        }

                        //If that still fails, throw exception
                        if (choiceListMethod == null)
                        {
                            throw new Exception($"Invalid ChoiceListMethodName: {enumAtt.choiceListMethodName}");
                        }

                        List<ValueNamePair> choiceList = choiceListMethod.Invoke(owningPropertyGroup, null) as List<ValueNamePair>;

                        List<string> choiceTitles = new List<string>();
                        List<int> choiceValues = new List<int>();

                        int choiceIndex = 0;

                        for (int i = 0; i < choiceList.Count; i++)
                        {
                            ValueNamePair pair = choiceList[i];

                            choiceTitles.Add(pair.name);
                            choiceValues.Add(pair.value);

                            if (Convert.ToInt32(property.GetValue(owningPropertyGroup)) == pair.value)
                            {
                                choiceIndex = i;
                            }
                        }

                        return widgetFactory.CreateDropdownWidget(
                            parent: baseWidget,
                            value: choiceIndex,
                            formattingOptions: WidgetFactory.EditorWidgetFormattingOptions.Config_StringEntry,
                            onValueChanged: value => SubmitEnum(owningPropertyGroup, property, choiceValues, value),
                            optionList: choiceTitles);
                    }

                case StringDropdownDisplayAttribute stringDropAtt:
                    {
                        //Check local class first
                        Type owningType = owningPropertyGroup.GetType();
                        MethodInfo choiceListMethod = null;
                        for (Type currentType = owningType; currentType != null && choiceListMethod == null; currentType = currentType.BaseType)
                        {
                            choiceListMethod = currentType.GetMethod(stringDropAtt.choiceListMethodName);
                        }

                        if (choiceListMethod == null && concretePropertyGroup != null)
                        {
                            //Search using matching concrete property group
                            owningType = concretePropertyGroup.GetType();

                            for (Type currentType = owningType; currentType != null && choiceListMethod == null; currentType = currentType.BaseType)
                            {
                                choiceListMethod = currentType.GetMethod(stringDropAtt.choiceListMethodName);
                            }
                        }

                        if (choiceListMethod == null)
                        {
                            //If local class check fails, try from SetupMethods
                            choiceListMethod = typeof(SetupMethods).GetMethod(stringDropAtt.choiceListMethodName);
                        }

                        if (choiceListMethod == null && FallbackSetupMethodsClass != null)
                        {
                            //If local class check fails, try from Fallback
                            choiceListMethod = FallbackSetupMethodsClass.GetMethod(stringDropAtt.choiceListMethodName);
                        }

                        //If that still fails, throw exception
                        if (choiceListMethod == null)
                        {
                            throw new Exception($"Invalid ChoiceListMethodName: {stringDropAtt.choiceListMethodName}");
                        }

                        List<string> choiceList = choiceListMethod.Invoke(owningPropertyGroup, null) as List<string>;
                        List<string> displayList = choiceList;

                        int choiceIndex = 0;

                        string currentValue = (string)property.GetValue(owningPropertyGroup);

                        if (!string.IsNullOrEmpty(currentValue))
                        {
                            choiceIndex = choiceList.IndexOf(currentValue);
                            if (choiceIndex == -1)
                            {
                                if (stringDropAtt.retainMissingValues)
                                {
                                    //DisplayList needs to be separated now, since it will hold different values in the 0 slot.
                                    displayList = new List<string>(choiceList);
                                    displayList.Insert(0, $"Missing: {currentValue}");
                                    choiceList.Insert(0, currentValue);
                                    choiceIndex = 0;
                                }
                                else
                                {
                                    Debug.Log($"Resetting Property due to missing prior value: {currentValue}");
                                    choiceIndex = 0;
                                }
                            }
                        }

                        if (choiceList.Count > 0)
                        {
                            currentValue = choiceList[choiceIndex];
                            property.SetValue(owningPropertyGroup, currentValue);
                        }

                        if (stringDropAtt.forceRefreshOnChange)
                        {
                            if (respawnPropertyGroupCallback == null)
                            {
                                //respawnPropertyGroupCallback is passed as null from contexts where it isn't currently supported
                                throw new NotSupportedException(
                                    "StringDropdownDisplay's ForceRefreshOnChange attribute not currently supported from this context.");
                            }

                            return widgetFactory.CreateDropdownWidget(
                                parent: baseWidget,
                                value: choiceIndex,
                                formattingOptions: WidgetFactory.EditorWidgetFormattingOptions.Config_StringEntry,
                                onValueChanged: value =>
                                    {
                                        //Update Value
                                        SubmitStringDropdown(owningPropertyGroup, property, choiceList, value);
                                        //Force Refresh
                                        respawnPropertyGroupCallback.Invoke();
                                    },
                                optionList: displayList);
                        }
                        else
                        {
                            return widgetFactory.CreateDropdownWidget(
                                parent: baseWidget,
                                value: choiceIndex,
                                formattingOptions: WidgetFactory.EditorWidgetFormattingOptions.Config_StringEntry,
                                onValueChanged: value => SubmitStringDropdown(owningPropertyGroup, property, choiceList, value),
                                optionList: displayList);
                        }
                    }

                case FieldMirrorDisplayAttribute _:
                case ControlledExtractionAttribute _:
                    (IPropertyGroup matchingConcretePropertyGroup, FieldDisplayAttribute concreteAttribute) =
                        owningPropertyGroup.SearchHierarchyForConcreteFieldAttributeAndPropertyGroup(attribute.fieldName);

                    return SpawnValueInputWidget(
                        owningPropertyGroup: owningPropertyGroup,
                        property: property,
                        attribute: concreteAttribute,
                        baseWidget: baseWidget,
                        respawnPropertyGroupCallback: respawnPropertyGroupCallback,
                        concretePropertyGroup: matchingConcretePropertyGroup);

                default:
                    Debug.LogError($"Unexpected PropertyType: {property.PropertyType}");
                    return null;
            }
        }

        #region Spawning Methods

        protected PropertyGroupContainer SpawnPropertyGroupContainer() => Instantiate(propertyGroupPrefab).GetComponent<PropertyGroupContainer>();
        protected PropertyGroupContainer SpawnNestedPropertyGroupContainer() => Instantiate(nestedPropertyGroupPrefab).GetComponent<PropertyGroupContainer>();
        protected PropertyGroupContainer SpawnFlatPropertyGroupContainer() => Instantiate(flatPropertyGroupPrefab).GetComponent<PropertyGroupContainer>();
        protected PropertyListItemContainer SpawnPropertyListItem() => Instantiate(nestedPropertyListItemPrefab).GetComponent<PropertyListItemContainer>();

        #endregion
        #region Helper Methods

        public static void SubmitDouble(
            IPropertyGroup group,
            PropertyInfo property,
            DoubleFieldDisplayAttribute doubleAtt,
            InputField inputField,
            string value)
        {
            double oldValue = double.Parse(value);
            double newValue = GeneralMath.Clamp(oldValue, doubleAtt.minimum, doubleAtt.maximum);

            if (newValue != oldValue)
            {
                inputField.text = newValue.ToString();
            }

            property.SetValue(group, newValue);
        }

        public static void SubmitInt(
            IPropertyGroup group,
            PropertyInfo property,
            IntFieldDisplayAttribute intAtt,
            InputField inputField,
            string value)
        {
            int oldValue = int.Parse(value);
            int newValue = GeneralMath.Clamp(oldValue, intAtt.minimum, intAtt.maximum);

            if (newValue != oldValue)
            {
                inputField.text = newValue.ToString();
            }

            property.SetValue(group, newValue);
        }

        public static void SubmitBool(
            IPropertyGroup group,
            PropertyInfo property,
            bool value)
        {
            property.SetValue(group, value);
        }

        public static void SubmitString(
            IPropertyGroup group,
            PropertyInfo property,
            string value)
        {
            property.SetValue(group, value?.Trim()?.Replace("\r", ""));
        }

        public static void SubmitKeyString(
            IPropertyGroup group,
            PropertyInfo property,
            string value)
        {
            value = value?.Trim();

            if (string.IsNullOrEmpty(value) || char.IsLetter(value[0]))
            {
                //Empty or null is fine, as is starting with a letter
                property.SetValue(group, value);
            }
            else
            {
                property.SetValue(group, $"_{value}");
            }
        }

        public static void SubmitEnum(
            IPropertyGroup group,
            PropertyInfo property,
            List<int> choiceValues,
            int value)
        {
            property.SetValue(group, choiceValues[value]);
        }

        public static void SubmitStringDropdown(
            IPropertyGroup group,
            PropertyInfo property,
            List<string> choiceValues,
            int value)
        {
            property.SetValue(group, choiceValues[value]);
        }

        private static string GetUniqueListItemName(
            string name,
            IList propertyGroupList,
            IPropertyGroup propertyGroup = null)
        {
            HashSet<string> usedNames = new HashSet<string>(
                propertyGroupList
                    .Cast<IPropertyGroup>()
                    .Where(x => x != propertyGroup)
                    .Select(x => x.GetItemTitle()));

            if (!usedNames.Contains(name))
            {
                //No Collision
                return name;
            }

            //
            //Collision
            //

            //Check if there's already an appended (#)
            if (name.Contains("(") && name.EndsWith(")"))
            {
                int indexOfOpen = name.LastIndexOf(" (");
                int indexOfClose = name.LastIndexOf(")");

                int indexInside = indexOfOpen + 2;
                int length = indexOfClose - indexInside;

                //Make sure there are contents
                //This would reject "TestFile ()"
                if (length > 0)
                {
                    string valueString = name.Substring(indexInside, length);

                    //Make sure value string can be parsed entirely, otherwise its not a modifier
                    //This would reject "TestFile (words)"
                    if (int.TryParse(valueString, out int modifierValue))
                    {
                        name = name.Substring(0, indexOfOpen);
                    }
                }
            }

            for (int modifier = 2; ; modifier++)
            {
                if (!usedNames.Contains($"{name} ({modifier})"))
                {
                    //No Collision
                    return $"{name} ({modifier})";
                }
            }
        }

        private static bool ListItemNameValidator(object group, string name)
        {
            if (name.Contains("."))
            {
                //No periods in titles
                return false;
            }

            return true;
        }

        private static void ListItemNameUpdater(object group, string name)
        {
            (group as IPropertyGroup).SetItemTitle(name);
        }

        private static string ListItemNameTranslator(object group)
        {
            return (group as IPropertyGroup).GetItemTitle();
        }

        #endregion Helper Methods
    }
#pragma warning restore IDE0019 // Use pattern matching
}
