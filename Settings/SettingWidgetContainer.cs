using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingWidgetContainer : MonoBehaviour
{
    [SerializeField]
    private Text labelText = null;
    [SerializeField]
    private Text valueText = null;
    [SerializeField]
    private Button settingButton = null;

    public Text LabelText => labelText;
    public Text ValueText => valueText;
    public Button SettingButton => settingButton;
}
