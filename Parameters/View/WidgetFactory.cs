using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BGC.Parameters.View
{
    public class WidgetFactory : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField]
        private GameObject optionWidgetPrefab = null;
        [SerializeField]
        private GameObject optionLabelPrefab = null;
        [SerializeField]
        private GameObject optionInputFieldPrefab = null;
        [SerializeField]
        private GameObject optionButtonPrefab = null;
        [SerializeField]
        private GameObject optionTogglePrefab = null;
        [SerializeField]
        private GameObject optionDropdownPrefab = null;
        [SerializeField]
        private GameObject scriptFieldPrefab = null;
        [SerializeField]
        private GameObject inputFieldPrefab = null;

        #region Interface Colors

        protected virtual Color InternalTextNormal { get; } = Color.black;
        protected virtual Color InternalTextHighlight { get; } = Color.green;

        protected virtual Color InternalButtonNormalBG { get; } = Color.white;
        protected virtual Color InternalButtonNormalText { get; } = Color.black;
        protected virtual Color InternalButtonInvertedBG { get; } = Color.black;
        protected virtual Color InternalButtonInvertedText { get; } = Color.white;
        protected virtual Color InternalButtonSpecialBG { get; } = new Color32(0x19, 0x00, 0x48, 0xFF);
        protected virtual Color InternalButtonSpecialText { get; } = Color.white;

        protected virtual Color DeleteButtonBG { get; } = Color.red;
        protected virtual Color DeleteButtonText { get; } = Color.black;

        protected virtual Color InputNormalBG { get; } = Color.white;
        protected virtual Color InputNormalText { get; } = Color.black;

        protected virtual Color InputKeyBG { get; } = Color.black;
        protected virtual Color InputKeyText { get; } = Color.white;

        protected virtual Color InputListBG { get; } = new Color32(0x19, 0x00, 0x48, 0xFF);
        protected virtual Color InputListText { get; } = Color.white;

        protected virtual Color InfoTextNormal { get; } = new Color32(0x10, 0x10, 0x10, 0xFF);

        #endregion Interface Colors

        [Flags]
        public enum ContainerConfig
        {
            None = 0,
            /// <summary>
            /// This is the default setting for the child's override value
            /// </summary>
            Submissive = None,

            //Small elements will have a fraction of the width of normal ones
            Small_Element_0 = 1 << 0,
            Small_Element_1 = 1 << 1,
            Small_Element_2 = 1 << 2,
            Small_Element_3 = 1 << 3,
            Small_Element_4 = 1 << 4,
            Small_Element_5 = 1 << 5,

            //Spacing is 10px
            Spacing_Internal = 1 << 6,
            Spacing_Leading = 1 << 7,
            Spacing_Trailing = 1 << 8,

            _Spacing_Bit_0 = 1 << 9,
            _Spacing_Bit_1 = 1 << 10,
            _Spacing_Bit_2 = 1 << 11,
            __Spacing_Bit_Mask_Protection = 1 << 12,

            Spacing_Size_Mask = _Spacing_Bit_0 | _Spacing_Bit_1 | _Spacing_Bit_2,

            Spacing_Size_Normal = None,
            Spacing_Size_Large = _Spacing_Bit_0,

            _Height_Bit_0 = 1 << 13,
            _Height_Bit_1 = 1 << 14,
            _Height_Bit_2 = 1 << 15,
            _Height_Bit_3 = 1 << 16,
            __Height_Bit_Mask_Protection = 1 << 17,

            Height_Mask = _Height_Bit_0 | _Height_Bit_1 | _Height_Bit_2 | _Height_Bit_3,

            Height_20px = None,
            Height_30px = _Height_Bit_0,
            Height_40px = _Height_Bit_1,
            Height_50px = _Height_Bit_0 | _Height_Bit_1,
            Height_60px = _Height_Bit_2,
            Height_70px = _Height_Bit_0 | _Height_Bit_2,
            Height_80px = _Height_Bit_1 | _Height_Bit_2,
            //Height_Fit_Children = Height_Mask,

            _Smallness_Bit_0 = 1 << 18,
            _Smallness_Bit_1 = 1 << 19,
            _Smallness_Bit_2 = 1 << 20,
            __Smallness_Bit_Mask_Protection = 1 << 21,

            Smallness_Mask = _Smallness_Bit_0 | _Smallness_Bit_1 | _Smallness_Bit_2,

            Smallness_2 = None,
            Smallness_3 = _Smallness_Bit_0,
            Smallness_4 = _Smallness_Bit_1,
            Smallness_5 = _Smallness_Bit_0 | _Smallness_Bit_1,

            _Squeeze_Y_Bit_0 = 1 << 22,
            _Squeeze_Y_Bit_1 = 1 << 23,
            _Squeeze_Y_Bit_2 = 1 << 24,
            __Squeeze_Y_Bit_Mask_Protection = 1 << 25,

            Squeeze_Y_Mask = _Squeeze_Y_Bit_0 | _Squeeze_Y_Bit_1 | _Squeeze_Y_Bit_2,

            Squeeze_Y_0 = None,
            Squeeze_Y_5 = _Squeeze_Y_Bit_0,
            Squeeze_Y_10 = _Squeeze_Y_Bit_1,
            Squeeze_Y_15 = _Squeeze_Y_Bit_0 | _Squeeze_Y_Bit_1,

            _Layout_Add_Bit_0 = 1 << 26,
            _Layout_Add_Bit_1 = 1 << 27,

            Layout_Add_Mask = _Layout_Add_Bit_0 | _Layout_Add_Bit_1,

            Layout_Add_Nothing = None,
            Layout_Add_VGroup = _Layout_Add_Bit_0,
            Layout_Add_Element = _Layout_Add_Bit_1,

            All_Addable_Masks = Spacing_Size_Mask | Height_Mask | Smallness_Mask | Squeeze_Y_Mask,

            Config_Normal_Even = Spacing_Internal | Height_60px | Spacing_Size_Normal,
            Config_Small_1st = Small_Element_0 | Spacing_Internal | Spacing_Leading | Height_60px,
            Config_Small_3rd = Small_Element_2 | Spacing_Internal | Spacing_Leading | Height_60px,
            Config_Small_4th = Small_Element_4 | Spacing_Internal | Spacing_Trailing | Height_60px,
            Config_Larger_Even = Spacing_Internal | Height_70px,
            Config_Header = Spacing_Internal | Height_80px,
            Config_Spacer = Height_30px,
            Config_Header_Small_1st = Small_Element_0 | Spacing_Internal | Spacing_Leading | Height_70px,
            Config_AddSpecial_OptionDelete = Small_Element_0 | Spacing_Internal | Height_60px | Smallness_5,
            Config_AddSpecial_OptionDelete_Header = Small_Element_0 | Spacing_Internal | Height_70px | Smallness_5,

            Config_InfoPanel_Header = Height_60px | Spacing_Internal | Spacing_Size_Large,
            Config_InfoPanel_SubHeader = Height_50px | Spacing_Internal | Spacing_Size_Large,
            Config_InfoPanel_Text = Height_40px | Spacing_Internal | Spacing_Size_Large,
            Config_InfoPanel_Spacer = Height_30px | Spacing_Internal | Spacing_Size_Large,

            Config_Tiny_Spacer_Scroll_Widget = Height_20px | Layout_Add_Element,
            Config_Small_Spacer_Scroll_Widget = Height_30px | Layout_Add_Element,
            Config_Normal_Scroll_Widget = Spacing_Internal | Height_60px | Spacing_Size_Normal | Layout_Add_Element,
            Config_Larger_Scroll_Widget = Spacing_Internal | Height_70px | Spacing_Size_Normal | Layout_Add_Element,
            Config_Large_Spacer_Scroll_Widget = Height_80px | Layout_Add_Element,

            Config_AddSpecial_Instructions = Layout_Add_VGroup,
            Config_AddSpecial_Instructions_Input = Height_Mask,
        }

        [Flags]
        public enum ButtonFormattingOptions
        {
            None = 0,

            _ThemeType_Bit_0 = 1 << 0,
            _ThemeType_Bit_1 = 1 << 1,
            _ThemeType_Bit_2 = 1 << 2,
            _ThemeType_Bit_3 = 1 << 3,
            __ThemeType_Bit_Mask_Protection = 1 << 4,

            ThemeType_Mask = _ThemeType_Bit_0 | _ThemeType_Bit_1 | _ThemeType_Bit_2 | _ThemeType_Bit_3,

            ThemeType_SystemWidget = None,
            ThemeType_KeyInput = _ThemeType_Bit_0,
            ThemeType_ListInput = _ThemeType_Bit_1,
            ThemeType_DeleteButton = _ThemeType_Bit_0 | _ThemeType_Bit_1,

            Config_DeleteButton = ThemeType_DeleteButton,
            Config_ValueEntry = ThemeType_SystemWidget,
            Config_KeyEntry = ThemeType_KeyInput,
            Config_ListEntry = ThemeType_ListInput,
            Config_ToggleValue = ThemeType_SystemWidget
        }

        [Flags]
        public enum EditorWidgetFormattingOptions
        {
            None = 0,

            _ThemeType_Bit_0 = 1 << 0,
            _ThemeType_Bit_1 = 1 << 1,
            _ThemeType_Bit_2 = 1 << 2,
            _ThemeType_Bit_3 = 1 << 3,
            __ThemeType_Bit_Mask_Protection = 1 << 4,

            ThemeType_Mask = _ThemeType_Bit_0 | _ThemeType_Bit_1 | _ThemeType_Bit_2 | _ThemeType_Bit_3,

            ThemeType_SystemWidget = None,
            ThemeType_KeyInput = _ThemeType_Bit_0,
            ThemeType_ListInput = _ThemeType_Bit_1,

            MultiNewLine = 1 << 5,

            TopAlign = 1 << 6,

            Config_StringEntry = ThemeType_SystemWidget,
            Config_ValueEntry = ThemeType_SystemWidget,
            Config_KeyEntry = ThemeType_KeyInput,
            Config_ListEntry = ThemeType_ListInput,

            Config_BlockEntry = ThemeType_SystemWidget | MultiNewLine | TopAlign,
        }

        [Flags]
        public enum TextItemFormattingOptions
        {
            None = 0,

            _ThemeType_Bit_0 = 1 << 0,
            _ThemeType_Bit_1 = 1 << 1,
            _ThemeType_Bit_2 = 1 << 2,
            _ThemeType_Bit_3 = 1 << 3,
            __ThemeType_Bit_Mask_Protection = 1 << 4,

            ThemeType_Mask = _ThemeType_Bit_0 | _ThemeType_Bit_1 | _ThemeType_Bit_2 | _ThemeType_Bit_3,

            ThemeType_SystemText = None,
            ThemeType_InfoText = _ThemeType_Bit_0,

            Config_BatteryText = ThemeType_SystemText,
            Config_InfoText = ThemeType_InfoText

        }

        private static WidgetFactory _instance;
        public static WidgetFactory instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new WidgetFactory();
                }
                return _instance;
            }
        }

        private WidgetFactory()
        {

        }

        /// <summary>
        /// Creates a Container widget to be used with the other widget factory methods.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="slots"></param>
        /// <param name="parent">Optional, if specified, the new widget will become a child</param>
        /// <param name="siblingIndex">Optional, if parent is specified, the new widget will become a child with this index</param>
        /// <param name="parentSlots">Optional, if parent is specified, the new widget will become a child, acting like the parent has this many slots</param>
        /// <returns></returns>
        public GameObject GetContainerWidget(
            ContainerConfig config,
            int slots = 1,
            GameObject parent = null,
            int siblingIndex = -1,
            int parentSlots = -1,
            ContainerConfig parentConfig = ContainerConfig.Submissive)
        {
            GameObject containerWidget = GameObject.Instantiate(optionWidgetPrefab);

            if (parent == null)
            {
                //We want to apply ContainerConfig (and thus height) if there is no parent
                ApplyContainerConfig(containerWidget, config);
            }
            else if (parent.GetComponent<LayoutGroup>() != null)
            {
                //We want to apply ContainerConfig (and thus height) if there is no there is a layout group in parent
                containerWidget.transform.SetParent(parent.transform, false);
                ApplyContainerConfig(containerWidget, config);
            }
            else
            {
                //We want to apply anchors and the like if there is a parent and it doesn't have a layout
                CoupleToParent(
                    widget: containerWidget,
                    parent: parent,
                    index: siblingIndex,
                    slots: parentSlots,
                    config: parentConfig);
            }

            WidgetFactoryContainerOrganizer container = containerWidget.GetComponent<WidgetFactoryContainerOrganizer>();

            //Store its values
            container.config = config;
            container.slots = slots;

            return containerWidget;
        }

        private static void CoupleToParent(
            GameObject widget,
            GameObject parent,
            int index,
            int slots,
            ContainerConfig config,
            ContainerConfig configSupplement = ContainerConfig.None)
        {
            if (parent != null)
            {
                widget.transform.SetParent(parent.transform, false);

                WidgetFactoryContainerOrganizer container = parent.GetComponent<WidgetFactoryContainerOrganizer>();
                if (container != null)
                {
                    //Pull down container's configuration
                    if (config == ContainerConfig.Submissive)
                    {
                        config = container.config;
                    }

                    //Pull down container's slots
                    if (slots == -1)
                    {
                        slots = container.slots;
                    }

                    if (index == -1)
                    {
                        index = container.childCount;
                    }

                    //Let it know we used one more of its slots
                    container.childCount++;
                }
            }

            //Set these values up to meaningful numbers if our parent doesn't have a WidgetFactoryContainerOrganizer
            if (slots == -1)
            {
                slots = 1;
            }

            if (index == -1)
            {
                index = 0;
            }

            if (configSupplement != ContainerConfig.None)
            {
                //Apply container config supplement
                //First by adding the masked values
                uint supplementMaskedValues = (uint)(configSupplement & ContainerConfig.All_Addable_Masks);
                uint configMaskedValues = (uint)(config & ContainerConfig.All_Addable_Masks);
                uint incrementedMaskedValues = (configMaskedValues + supplementMaskedValues) & (uint)ContainerConfig.All_Addable_Masks;
                config = (ContainerConfig)((uint)(config & ~ContainerConfig.All_Addable_Masks) | incrementedMaskedValues);

                //Now turn on flags that were set on in the supplement:
                config |= (configSupplement & ~ContainerConfig.All_Addable_Masks);
            }

            ApplyContainerConfigToWidget(widget, config, index, slots);
        }

        public GameObject CreateLabelWidget(
            GameObject parent = null,
            string text = "",
            TextItemFormattingOptions formattingOptions = TextItemFormattingOptions.None,
            TextAnchor alignment = TextAnchor.MiddleCenter,
            ContainerConfig configOverride = ContainerConfig.Submissive,
            int index = -1,
            int slots = -1)
        {
            GameObject labelWidget = GameObject.Instantiate(optionLabelPrefab);

            //Attach to parent and load up appropriate configurations
            CoupleToParent(
                widget: labelWidget,
                parent: parent,
                index: index,
                slots: slots,
                config: configOverride,
                configSupplement: ContainerConfig.Squeeze_Y_5);

            ApplyTextFormattingOptions(labelWidget, formattingOptions);

            labelWidget.GetComponent<Text>().text = text;
            labelWidget.GetComponent<Text>().alignment = alignment;

            return labelWidget;
        }

        public GameObject CreateTriggeredLabelWidget(
            GameObject parent = null,
            string text = "",
            TextItemFormattingOptions formattingOptions = TextItemFormattingOptions.None,
            TextAnchor alignment = TextAnchor.MiddleCenter,
            ContainerConfig configOverride = ContainerConfig.Submissive,
            int index = -1,
            int slots = -1)
        {
            GameObject labelWidget = GameObject.Instantiate(optionLabelPrefab);

            //Attach to parent and load up appropriate configurations
            CoupleToParent(
                widget: labelWidget,
                parent: parent,
                index: index,
                slots: slots,
                config: configOverride,
                configSupplement: ContainerConfig.Squeeze_Y_5);

            ApplyTextFormattingOptions(labelWidget, formattingOptions);

            labelWidget.GetComponent<Text>().text = text;
            labelWidget.GetComponent<Text>().alignment = alignment;
            labelWidget.GetComponent<Text>().raycastTarget = true;

            LabelSwapTrigger swapTrigger = labelWidget.AddComponent<LabelSwapTrigger>();

            swapTrigger.OnDoubleClick += () => { Debug.Log("Double-clicked"); };

            return labelWidget;
        }

        public GameObject CreateScriptInputFieldWidget(
            GameObject parent = null,
            IConvertible text = null,
            UnityEngine.Events.UnityAction<string> onEndEdit = null,
            ContainerConfig configOverride = ContainerConfig.Submissive,
            int index = -1,
            int slots = -1)
        {
            GameObject inputFieldWidget = GameObject.Instantiate(scriptFieldPrefab);

            if (text == null)
            {
                text = "";
            }

            TMPro.TMP_InputField inputField = inputFieldWidget.GetComponent<TMPro.TMP_InputField>();
            inputField.text = text.ToString();

            //Attach to parent and load up appropriate configurations
            CoupleToParent(
                widget: inputFieldWidget,
                parent: parent,
                index: index,
                slots: slots,
                config: configOverride);

            if (onEndEdit != null)
            {
                inputField.onEndEdit.AddListener(onEndEdit);
            }

            return inputFieldWidget;
        }

        public GameObject CreateLargeInputFieldWidget(
            GameObject parent = null,
            IConvertible text = null,
            UnityEngine.Events.UnityAction<string> onEndEdit = null,
            ContainerConfig configOverride = ContainerConfig.Submissive,
            int index = -1,
            int slots = -1)
        {
            GameObject inputFieldWidget = GameObject.Instantiate(inputFieldPrefab);

            if (text == null)
            {
                text = "";
            }

            TMPro.TMP_InputField inputField = inputFieldWidget.GetComponent<TMPro.TMP_InputField>();
            inputField.text = text.ToString();

            //Attach to parent and load up appropriate configurations
            CoupleToParent(
                widget: inputFieldWidget,
                parent: parent,
                index: index,
                slots: slots,
                config: configOverride);

            if (onEndEdit != null)
            {
                inputField.onEndEdit.AddListener(onEndEdit);
            }

            return inputFieldWidget;
        }

        public GameObject CreateInputFieldWidget(
            GameObject parent = null,
            IConvertible text = null,
            IConvertible postfixText = null,
            EditorWidgetFormattingOptions formattingOptions = EditorWidgetFormattingOptions.None,
            InputField.ContentType contentType = InputField.ContentType.Standard,
            UnityEngine.Events.UnityAction<string> onEndEdit = null,
            UnityEngine.Events.UnityAction<string, InputField> newOnEndEdit = null,
            ContainerConfig configOverride = ContainerConfig.Submissive,
            int index = -1,
            int slots = -1)
        {
            GameObject inputFieldWidget = GameObject.Instantiate(optionInputFieldPrefab);

            //Attach to parent and load up appropriate configurations
            CoupleToParent(
                widget: inputFieldWidget,
                parent: parent,
                index: index,
                slots: slots,
                config: configOverride);

            ApplyInputFieldFormattingOptions(inputFieldWidget, formattingOptions);

            if (text == null)
            {
                text = "";
            }

            if (postfixText == null)
            {
                postfixText = "";
            }

            InputField inputField = inputFieldWidget.GetComponent<InputField>();
            inputField.text = text.ToString();
            inputField.contentType = contentType;

            if (onEndEdit != null)
            {
                inputField.onEndEdit.AddListener(onEndEdit);
            }

            if (newOnEndEdit != null)
            {
                inputField.onEndEdit.AddListener((string value) => { newOnEndEdit.Invoke(value, inputField); });
            }

            inputFieldWidget.GetComponent<OptionInputField>().PostfixText.text = postfixText.ToString();

            return inputFieldWidget;
        }

        public GameObject CreateButtonWidget(
            GameObject parent = null,
            IConvertible text = null,
            UnityEngine.Events.UnityAction onClick = null,
            ButtonFormattingOptions buttonOptions = ButtonFormattingOptions.None,
            ContainerConfig configOverride = ContainerConfig.Submissive,
            int index = -1,
            int slots = -1)
        {
            GameObject buttonWidget = GameObject.Instantiate(optionButtonPrefab);

            //Attach to parent and load up appropriate configurations
            CoupleToParent(
                widget: buttonWidget,
                parent: parent,
                index: index,
                slots: slots,
                config: configOverride);

            //Apply specific widget options
            ApplyButtonFormattingOptions(buttonWidget, buttonOptions);

            Button button = buttonWidget.GetComponent<Button>();
            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }

            if (text == null)
            {
                text = "";
            }
            Text textElement = buttonWidget.GetComponentInChildren<Text>();
            textElement.text = text.ToString();

            return buttonWidget;
        }

        public GameObject CreateToggleWidget(
            GameObject parent = null,
            bool value = false,
            ButtonFormattingOptions formattingOptions = ButtonFormattingOptions.None,
            UnityEngine.Events.UnityAction<bool> onValueChanged = null,
            ContainerConfig configOverride = ContainerConfig.Submissive,
            int index = -1,
            int slots = -1)
        {
            GameObject toggleWidget = GameObject.Instantiate(optionTogglePrefab);

            //Attach to parent and load up appropriate configurations
            CoupleToParent(
                widget: toggleWidget,
                parent: parent,
                index: index,
                slots: slots,
                config: configOverride);

            //Apply specific widget options
            ApplyToggleFormattingOptions(toggleWidget, formattingOptions);

            Toggle toggle = toggleWidget.GetComponent<Toggle>();
            if (onValueChanged != null)
            {
                toggle.onValueChanged.AddListener(onValueChanged);
            }

            toggle.isOn = value;

            return toggleWidget;
        }

        public GameObject CreateDropdownWidget(
            GameObject parent = null,
            int value = 0,
            EditorWidgetFormattingOptions formattingOptions = EditorWidgetFormattingOptions.None,
            UnityEngine.Events.UnityAction<int> onValueChanged = null,
            List<string> optionList = null,
            ContainerConfig configOverride = ContainerConfig.Submissive,
            int index = -1,
            int slots = -1)
        {
            GameObject dropdownWidget = GameObject.Instantiate(optionDropdownPrefab);

            //Attach to parent and load up appropriate configurations
            CoupleToParent(
                widget: dropdownWidget,
                parent: parent,
                index: index,
                slots: slots,
                config: configOverride);


            Dropdown dropdown = dropdownWidget.GetComponent<Dropdown>();
            dropdown.ClearOptions();

            ApplyDropdownFormattingOptions(dropdownWidget, formattingOptions);

            if (optionList != null)
            {
                dropdown.AddOptions(optionList);
            }

            if (onValueChanged != null)
            {
                dropdown.onValueChanged.AddListener(onValueChanged);
            }

            dropdown.value = value;
            dropdown.RefreshShownValue();

            return dropdownWidget;
        }

        public GameObject UpdateWidgetFormattingOptions(
            GameObject widget,
            ButtonFormattingOptions formattingOptions)
        {
            ApplyButtonFormattingOptions(widget, formattingOptions);

            return widget;
        }

        public GameObject UpdateInputFieldFormattingOptions(
            GameObject widget,
            EditorWidgetFormattingOptions formattingOptions)
        {
            ApplyInputFieldFormattingOptions(widget, formattingOptions);

            return widget;
        }

        public static void UpdateContainerConfig(GameObject widget, ContainerConfig config)
        {
            ApplyContainerConfig(widget, config);
        }

        private static void ApplyContainerConfig(GameObject widget, ContainerConfig config)
        {
            RectTransform transform = widget.GetComponent<RectTransform>();

            float height = 20f +
                10f * GetMaskedFlagValue(config, ContainerConfig.Height_Mask, ContainerConfig._Height_Bit_0);
            transform.sizeDelta = new Vector2(0f, height);

            switch (ApplyMask(config, ContainerConfig.Layout_Add_Mask))
            {
                case ContainerConfig.Layout_Add_Nothing:
                    // Do nothing
                    break;

                case ContainerConfig.Layout_Add_Element:
                    {
                        LayoutElement layoutElement = widget.AddComponent<LayoutElement>();
                        layoutElement.minHeight = height;
                        layoutElement.preferredHeight = height;

                        break;
                    }

                case ContainerConfig.Layout_Add_VGroup:
                    {
                        VerticalLayoutGroup layoutGroup = widget.AddComponent<VerticalLayoutGroup>();

                        layoutGroup.childForceExpandHeight = true;
                        layoutGroup.childForceExpandWidth = true;
                        layoutGroup.childControlWidth = true;
                        layoutGroup.childControlHeight = true;

                        break;
                    }

                default:
                    Debug.LogError($"Unexpected Masked ContainerConfig: {ApplyMask(config, ContainerConfig.Layout_Add_Mask)}");
                    break;
            }
        }

        private static void ApplyContainerConfigToWidget(
            GameObject widget,
            ContainerConfig config,
            int index,
            int slots)
        {
            if (index > slots)
            {
                Debug.LogError($"Unexpected value. Index > Slots: {index} > {slots}");
                index = slots;
            }

            float effectiveItemSlots = 0f;
            float effectiveItemIndex = 0f;

            float largeItemEffectiveSize = 2f + GetMaskedFlagValue(config, ContainerConfig.Smallness_Mask, ContainerConfig._Smallness_Bit_0);
            float effectiveItemSize = largeItemEffectiveSize;

            for (int i = 0; i < slots; i++)
            {
                float adjustment = largeItemEffectiveSize;

                if (IsItemSmall(i, config))
                {
                    adjustment = 1f;

                    if (i == index)
                    {
                        effectiveItemSize = 1f;
                    }
                }

                effectiveItemSlots += adjustment;

                if (i < index)
                {
                    effectiveItemIndex += adjustment;
                }
            }

            RectTransform transform = widget.GetComponent<RectTransform>();

            transform.anchorMin = new Vector2(effectiveItemIndex / effectiveItemSlots, 0f);
            transform.anchorMax = new Vector2((effectiveItemIndex + effectiveItemSize) / effectiveItemSlots, 1f);

            bool leftSpacing = false;
            bool rightSpacing = false;

            if (index == 0)
            {
                if (CheckFlag(config, ContainerConfig.Spacing_Leading))
                {
                    leftSpacing = true;
                }
            }
            else if (CheckFlag(config, ContainerConfig.Spacing_Internal))
            {
                leftSpacing = true;
            }

            if (index >= slots - 1)
            {
                if (CheckFlag(config, ContainerConfig.Spacing_Trailing))
                {
                    rightSpacing = true;
                }
            }
            else if (CheckFlag(config, ContainerConfig.Spacing_Internal))
            {
                rightSpacing = true;
            }

            float spacing = 10f + 10f * GetMaskedFlagValue(config, ContainerConfig.Spacing_Size_Mask, ContainerConfig._Spacing_Bit_0);

            float yOffset = 5f * GetMaskedFlagValue(config, ContainerConfig.Squeeze_Y_Mask, ContainerConfig._Squeeze_Y_Bit_0);

            transform.offsetMin = new Vector2()
            {
                x = leftSpacing ? spacing : 0f,
                y = yOffset
            };

            transform.offsetMax = new Vector2()
            {
                x = rightSpacing ? -spacing : 0f,
                y = -yOffset
            };

        }

        //ContainerConfig
        public static bool CheckFlag(ContainerConfig options, ContainerConfig flag)
        {
            return (options & flag) == flag;
        }

        private static bool CheckMaskedFlag(ContainerConfig config, ContainerConfig mask, ContainerConfig flag)
        {
            return (config & mask) == flag;
        }

        private static int GetMaskedFlagValue(ContainerConfig config, ContainerConfig mask, ContainerConfig firstBit)
        {
            return (int)(config & mask) / (int)firstBit;
        }

        public static ContainerConfig ApplyMask(ContainerConfig config, ContainerConfig mask)
        {
            return config & mask;
        }

        //TextItemFormattingOptions
        private static bool CheckFlag(TextItemFormattingOptions options, TextItemFormattingOptions flag)
        {
            return (options & flag) == flag;
        }

        public static TextItemFormattingOptions ApplyMask(TextItemFormattingOptions config, TextItemFormattingOptions mask)
        {
            return config & mask;
        }

        private static int GetMaskedFlagValue(TextItemFormattingOptions config, TextItemFormattingOptions mask, TextItemFormattingOptions firstBit)
        {
            return (int)(config & mask) / (int)firstBit;
        }

        //ButtonFormattingOptions
        private static bool CheckFlag(ButtonFormattingOptions options, ButtonFormattingOptions flag)
        {
            return (options & flag) == flag;
        }

        public static ButtonFormattingOptions ApplyMask(ButtonFormattingOptions config, ButtonFormattingOptions mask)
        {
            return config & mask;
        }

        private static int GetMaskedFlagValue(ButtonFormattingOptions config, ButtonFormattingOptions mask, ButtonFormattingOptions firstBit)
        {
            return (int)(config & mask) / (int)firstBit;
        }

        //InputFieldFormattingOptions
        private static bool CheckFlag(EditorWidgetFormattingOptions options, EditorWidgetFormattingOptions flag)
        {
            return (options & flag) == flag;
        }

        public static EditorWidgetFormattingOptions ApplyMask(EditorWidgetFormattingOptions config, EditorWidgetFormattingOptions mask)
        {
            return config & mask;
        }

        private static int GetMaskedFlagValue(EditorWidgetFormattingOptions config, EditorWidgetFormattingOptions mask, EditorWidgetFormattingOptions firstBit)
        {
            return (int)(config & mask) / (int)firstBit;
        }

        private void ApplyButtonFormattingOptions(
            GameObject widget,
            ButtonFormattingOptions options)
        {
            Color bgColor;
            Color textColor;

            switch (ApplyMask(options, ButtonFormattingOptions.ThemeType_Mask))
            {
                case ButtonFormattingOptions.ThemeType_SystemWidget:
                    bgColor = InternalButtonNormalBG;
                    textColor = InternalButtonNormalText;
                    break;

                case ButtonFormattingOptions.ThemeType_KeyInput:
                    bgColor = InternalButtonInvertedBG;
                    textColor = InternalButtonInvertedText;
                    break;

                case ButtonFormattingOptions.ThemeType_DeleteButton:
                    bgColor = DeleteButtonBG;
                    textColor = DeleteButtonText;
                    break;

                case ButtonFormattingOptions.ThemeType_ListInput:
                    bgColor = InternalButtonSpecialBG;
                    textColor = InternalButtonSpecialText;
                    break;

                default:
                    Debug.LogError($"Unrecognized ThemeType: {(int)ApplyMask(options, ButtonFormattingOptions.ThemeType_Mask)}");
                    goto case ButtonFormattingOptions.ThemeType_SystemWidget;
            }

            widget.GetComponent<Image>().color = bgColor;
            widget.GetComponentInChildren<Text>().color = textColor;
        }

        private void ApplyToggleFormattingOptions(
            GameObject widget,
            ButtonFormattingOptions options)
        {
            Color bgColor;
            Color checkColor;

            switch (ApplyMask(options, ButtonFormattingOptions.ThemeType_Mask))
            {
                case ButtonFormattingOptions.ThemeType_SystemWidget:
                    bgColor = InternalButtonNormalBG;
                    checkColor = InternalButtonNormalText;
                    break;

                case ButtonFormattingOptions.ThemeType_KeyInput:
                    bgColor = InternalButtonInvertedBG;
                    checkColor = InternalButtonInvertedText;
                    break;

                case ButtonFormattingOptions.ThemeType_ListInput:
                    bgColor = InternalButtonSpecialBG;
                    checkColor = InternalButtonSpecialText;
                    break;

                default:
                    Debug.LogError($"Unrecognized ThemeType: {(int)ApplyMask(options, ButtonFormattingOptions.ThemeType_Mask)}");
                    goto case ButtonFormattingOptions.ThemeType_SystemWidget;
            }

            Toggle toggle = widget.GetComponent<Toggle>();
            toggle.targetGraphic.color = bgColor;
            toggle.graphic.color = checkColor;
        }

        private void ApplyInputFieldFormattingOptions(
            GameObject widget,
            EditorWidgetFormattingOptions options)
        {
            Color bgColor;
            Color textColor;

            switch (ApplyMask(options, EditorWidgetFormattingOptions.ThemeType_Mask))
            {
                case EditorWidgetFormattingOptions.ThemeType_SystemWidget:
                    bgColor = InputNormalBG;
                    textColor = InputNormalText;
                    break;

                case EditorWidgetFormattingOptions.ThemeType_KeyInput:
                    bgColor = InputKeyBG;
                    textColor = InputKeyText;
                    break;

                case EditorWidgetFormattingOptions.ThemeType_ListInput:
                    bgColor = InputListBG;
                    textColor = InputListText;
                    break;

                default:
                    Debug.LogError($"Unrecognized ThemeType: {(int)ApplyMask(options, EditorWidgetFormattingOptions.ThemeType_Mask)}");
                    goto case EditorWidgetFormattingOptions.ThemeType_SystemWidget;
            }

            InputField inputfield = widget.GetComponent<InputField>();

            //Set Colors
            widget.GetComponent<Image>().color = bgColor;
            inputfield.textComponent.color = textColor;

            OptionInputField optionInputField = widget.GetComponent<OptionInputField>();

            if (optionInputField != null)
            {
                optionInputField.PostfixText.color = textColor;
            }

            //Set formatting
            if (CheckFlag(options, EditorWidgetFormattingOptions.MultiNewLine))
            {
                inputfield.lineType = InputField.LineType.MultiLineNewline;
            }
            else
            {
                inputfield.lineType = InputField.LineType.SingleLine;
            }

            if (CheckFlag(options, EditorWidgetFormattingOptions.TopAlign))
            {
                inputfield.textComponent.alignment = TextAnchor.UpperLeft;
            }
        }

        private void ApplyDropdownFormattingOptions(
            GameObject widget,
            EditorWidgetFormattingOptions options)
        {
            Color bgColor;
            Color textColor;

            switch (ApplyMask(options, EditorWidgetFormattingOptions.ThemeType_Mask))
            {
                case EditorWidgetFormattingOptions.ThemeType_SystemWidget:
                    bgColor = InternalButtonNormalBG;
                    textColor = InternalButtonNormalText;
                    break;

                case EditorWidgetFormattingOptions.ThemeType_KeyInput:
                    bgColor = InputKeyBG;
                    textColor = InputKeyText;
                    break;

                case EditorWidgetFormattingOptions.ThemeType_ListInput:
                    bgColor = InputListBG;
                    textColor = InputListText;
                    break;

                default:
                    Debug.LogError($"Unrecognized ThemeType: {(int)ApplyMask(options, EditorWidgetFormattingOptions.ThemeType_Mask)}");
                    goto case EditorWidgetFormattingOptions.ThemeType_SystemWidget;
            }

            widget.GetComponent<Image>().color = bgColor;
            widget.GetComponent<Dropdown>().captionText.color = textColor;

        }

        private void ApplyTextFormattingOptions(
            GameObject widget,
            TextItemFormattingOptions options)
        {
            switch (ApplyMask(options, TextItemFormattingOptions.ThemeType_Mask))
            {
                case TextItemFormattingOptions.ThemeType_SystemText:
                    widget.GetComponent<Text>().color = InternalTextNormal;
                    break;

                case TextItemFormattingOptions.ThemeType_InfoText:
                    widget.GetComponent<Text>().color = InfoTextNormal;
                    break;

                default:
                    Debug.LogError($"Unrecognized ThemeType: {(int)ApplyMask(options, TextItemFormattingOptions.ThemeType_Mask)}");
                    break;
            }
        }

        private static bool IsItemSmall(int index, ContainerConfig config)
        {
            switch (index)
            {
                case 0: return CheckFlag(config, ContainerConfig.Small_Element_0);
                case 1: return CheckFlag(config, ContainerConfig.Small_Element_1);
                case 2: return CheckFlag(config, ContainerConfig.Small_Element_2);
                case 3: return CheckFlag(config, ContainerConfig.Small_Element_3);
                case 4: return CheckFlag(config, ContainerConfig.Small_Element_4);
                case 5: return CheckFlag(config, ContainerConfig.Small_Element_5);

                default: return false;
            }
        }

    }
}
