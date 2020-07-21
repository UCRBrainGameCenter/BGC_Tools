using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using BGC.DataStructures.Generic;

namespace BGC.UI.Dialogs
{
    public class ModalListDialog : MonoBehaviour
    {
        [Header("Dialog Buttons")]
        [SerializeField]
        private Button buttonA = null;
        [SerializeField]
        private Button buttonB = null;

        [Header("List Edit Components")]
        [SerializeField]
        private GameObject editButtonsContainer = null;
        [SerializeField]
        private Button upButton = null;
        [SerializeField]
        private Button downButton = null;
        [SerializeField]
        private Button deleteButton = null;
        [SerializeField]
        private Button editButton = null;

        [Header("Dialog Components")]
        [SerializeField]
        private Text headerText = null;
        [SerializeField]
        private Transform listWidgetArea = null;

        [Header("Prefabs")]
        [SerializeField]
        private GameObject listItemButton = null;

        private static ModalListDialog instance;

        private InputField.ContentType inputType = InputField.ContentType.Alphanumeric;

        public enum Response
        {
            A = 0,
            B
        }

        private bool forceSelection;
        private string itemTitle;
        private IList itemList;
        private object selectedItem;
        private Func<object, string> nameTranslator;
        private Func<object, string, bool> nameValidator;

        private Action<Response> callback;
        private Action<object, string> nameUpdater;
        private Action<object, Response> selectCallback;

        private ConstructingPool<GameObject> listButtonPool;
        private Dictionary<object, GameObject> buttonMap;

        public ModalListDialog()
        {
            instance = this;
        }

        public void Initialize()
        {
            instance = this;
        }

        private void Awake()
        {
            buttonA.onClick.AddListener(() => HandleButtons(Response.A));
            buttonB.onClick.AddListener(() => HandleButtons(Response.B));

            upButton.onClick.AddListener(() => DirectionPressed(true));
            downButton.onClick.AddListener(() => DirectionPressed(false));
            deleteButton.onClick.AddListener(DeletePressed);
            editButton.onClick.AddListener(EditPressed);

            listButtonPool = new ConstructingPool<GameObject>(BuildListItem);
            listButtonPool.onCheckIn = CheckIn;
            listButtonPool.onCheckOut = CheckOut;

            buttonMap = new Dictionary<object, GameObject>();
        }

        private void SetButtonText(string a = "", string b = "")
        {
            buttonA.gameObject.SetActive(string.IsNullOrEmpty(a) == false);
            buttonB.gameObject.SetActive(string.IsNullOrEmpty(b) == false);

            buttonA.GetComponentInChildren<Text>().text = a;
            buttonB.GetComponentInChildren<Text>().text = b;
        }

        public static void ShowListEditModal(
            string headerText,
            IList itemList,
            Func<object, string> nameTranslator,
            Action<object, string> nameUpdater,
            Action<Response> callback,
            Func<object, string, bool> nameValidator = null,
            InputField.ContentType inputType = InputField.ContentType.Alphanumeric)
        {
            //Update header
            instance.headerText.text = headerText;

            //Update button text and visibility
            instance.SetButtonText(a: "Done");

            instance.editButtonsContainer.SetActive(true);

            instance.forceSelection = false;

            instance.itemList = itemList;
            instance.nameTranslator = nameTranslator;
            instance.nameValidator = nameValidator;
            instance.nameUpdater = nameUpdater;
            instance.callback = callback;

            instance.gameObject.SetActive(true);

            instance.inputType = inputType;

            instance.RebuildList();
        }

        public static void ShowListSelectModal(
            string headerText,
            IList itemList,
            string buttonALabel,
            string buttonBLabel,
            bool forceSelection,
            Func<object, string> nameTranslator,
            Action<object, Response> selectCallback,
            Action<Response> buttonCallback = null)
        {
            //Update header
            instance.headerText.text = headerText;

            //Update button text and visibility
            instance.SetButtonText(a: buttonALabel, b: buttonBLabel);

            instance.editButtonsContainer.SetActive(false);

            instance.forceSelection = forceSelection;
            instance.buttonA.enabled = !forceSelection;

            instance.itemList = itemList;
            instance.nameTranslator = nameTranslator;
            instance.nameValidator = null;
            instance.nameUpdater = null;
            instance.selectCallback = selectCallback;
            instance.callback = buttonCallback;

            instance.gameObject.SetActive(true);

            instance.inputType = InputField.ContentType.Alphanumeric;

            instance.RebuildList();
        }


        private void HandleButtons(Response response)
        {
            Action<Response> tempCallback = callback;
            Action<object, Response> tempSelectedCallback = selectCallback;
            object tempSelectedItem = selectedItem;

            itemTitle = null;
            itemList = null;

            selectedItem = null;

            nameTranslator = null;
            nameValidator = null;
            nameUpdater = null;
            callback = null;
            selectCallback = null;

            foreach (Transform listItemButton in listWidgetArea)
            {
                listButtonPool.CheckIn(listItemButton.gameObject);
            }

            buttonMap.Clear();

            gameObject.SetActive(false);

            tempCallback?.Invoke(response);
            tempSelectedCallback?.Invoke(tempSelectedItem, response);
        }

        private void DirectionPressed(bool up)
        {
            //Don't do anything if we would move it out of the bounds of the current stages
            int sourcePosition = itemList.IndexOf(selectedItem);
            if (sourcePosition == (up ? 0 : itemList.Count - 1))
            {
                return;
            }

            int destinationPosition = sourcePosition + (up ? -1 : 1);

            object temp = itemList[sourcePosition];
            itemList[sourcePosition] = itemList[destinationPosition];
            itemList[destinationPosition] = temp;

            RebuildList();
        }

        private void DeletePressed()
        {
            ModalDialog.ShowCustomSimpleModal(
                headerText: $"Delete {itemTitle}",
                bodyText: $"Are you sure you want to delete \"{nameTranslator(selectedItem)}\"?",
                buttonALabel: "Delete",
                buttonBLabel: "Cancel",
                buttonCLabel: null,
                callback: DeleteCallback);
        }

        private void EditPressed()
        {
            ModalDialog.ShowInputModal(ModalDialog.Mode.InputConfirmCancel,
                headerText: "Edit Name",
                bodyText: $"Enter new name for \"{nameTranslator(selectedItem)}\".",
                inputCallback: EditCallback,
                inputType: inputType);
        }

        private void EditCallback(ModalDialog.Response response, string newName)
        {
            switch (response)
            {
                case ModalDialog.Response.Confirm:
                    HandleNewName(newName);
                    return;

                case ModalDialog.Response.Cancel:
                    //Do nothing
                    return;

                default:
                    Debug.LogError($"Unexpected ModalDialog.Response: {response}");
                    return;
            }
        }

        private void DeleteCallback(ModalDialog.Response response)
        {
            switch (response)
            {
                case ModalDialog.Response.A:
                    //Delete
                    itemList.Remove(selectedItem);
                    RebuildList();
                    return;

                case ModalDialog.Response.B:
                    //Do nothing
                    return;

                default:
                    Debug.LogError($"Unexpected ModalDialog.Response: {response}");
                    return;
            }
        }

        private void HandleNewName(string newName)
        {
            if (string.IsNullOrEmpty(newName))
            {
                ModalDialog.ShowSimpleModal(ModalDialog.Mode.Accept,
                    headerText: "Error",
                    bodyText: $"{itemTitle} names cannot be blank.");
                return;
            }

            if (nameTranslator(selectedItem) == newName)
            {
                //We don't care, it was set to the same thing
                return;
            }

            if (itemList.Cast<object>().Select(nameTranslator).Contains(newName))
            {
                ModalDialog.ShowSimpleModal(ModalDialog.Mode.Accept,
                    headerText: "Error",
                    bodyText: $"{itemTitle} name \"{newName}\" already in use in this context.");

                return;
            }

            if (nameValidator != null && !nameValidator.Invoke(selectedItem, newName))
            {
                return;
            }

            //Update Name
            nameUpdater(selectedItem, newName);

            buttonMap[selectedItem].GetComponentInChildren<Text>().text = newName;
        }

        private void RebuildList()
        {
            if (selectedItem != null && !itemList.Contains(selectedItem))
            {
                selectedItem = null;
            }

            buttonMap.Clear();

            foreach (Transform listItemButton in listWidgetArea)
            {
                if (listItemButton.gameObject.activeSelf)
                {
                    listButtonPool.CheckIn(listItemButton.gameObject);
                }
            }

            foreach (object listItem in itemList)
            {
                object tempListItem = listItem;
                GameObject newListItem = listButtonPool.CheckOut();

                newListItem.GetComponentInChildren<Text>().text = nameTranslator(tempListItem);
                newListItem.GetComponent<Button>().onClick.AddListener(() => SelectItem(tempListItem));

                newListItem.transform.SetAsLastSibling();

                buttonMap.Add(tempListItem, newListItem);
            }

            SelectItem(selectedItem);
        }

        private void SelectItem(object newSelectedItem)
        {
            //Update old selected
            if (selectedItem != null)
            {
                SetButtonState(buttonMap[selectedItem], false);
            }

            selectedItem = newSelectedItem;

            //Update selected
            if (selectedItem != null)
            {
                SetButtonState(buttonMap[selectedItem], true);
            }

            //Update Edit Buttons
            upButton.enabled = selectedItem != null;
            downButton.enabled = selectedItem != null;
            editButton.enabled = selectedItem != null;
            deleteButton.enabled = selectedItem != null;

            if (forceSelection)
            {
                buttonA.enabled = newSelectedItem != null;
            }
        }

        private GameObject BuildListItem()
        {
            GameObject temp = Instantiate(listItemButton);
            temp.transform.SetParent(listWidgetArea);
            temp.transform.localScale = Vector3.one;

            return temp;
        }

        private void CheckIn(GameObject listItem)
        {
            listItem.SetActive(false);
            listItem.GetComponent<Button>().onClick.RemoveAllListeners();
        }

        private void CheckOut(GameObject listItem)
        {
            listItem.SetActive(true);
            SetButtonState(listItem, false);
        }

        public void SetButtonState(GameObject listItem, bool active)
        {
            listItem.GetComponent<ListViewButtonControl>().SetButtonState(active);
        }
    }
}
