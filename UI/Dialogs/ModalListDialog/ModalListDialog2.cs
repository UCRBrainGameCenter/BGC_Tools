using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using BGC.DataStructures.Generic;
using BGC.Extensions;

namespace BGC.UI.Dialogs
{
    public class ModalListDialog2 : MonoBehaviour
    {
        [Header("Dialog Buttons")]
        [SerializeField]
        private Button buttonA = null;

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
        private Button addButton = null;

        [Header("Dialog Components")]
        [SerializeField]
        private Text headerText = null;
        [SerializeField]
        private Transform listWidgetArea = null;

        [Header("Prefabs")]
        [SerializeField]
        private GameObject listItemButton = null;

        private static ModalListDialog2 instance;
        
        private IList valueList = null;
        
        private List<SimpleListInput> itemList;
        private SimpleListInput selectedItem;

        private Action<IList> callback;

        private ConstructingPool<GameObject> listButtonPool;

        public ModalListDialog2()
        {
            instance = this;
        }

        private void Awake()
        {
            buttonA.onClick.AddListener(() => HandleButtons());

            upButton.onClick.AddListener(() => DirectionPressed(true));
            downButton.onClick.AddListener(() => DirectionPressed(false));
            deleteButton.onClick.AddListener(DeletePressed);
            addButton.onClick.AddListener(AddPressed);

            listButtonPool = new ConstructingPool<GameObject>(BuildListItem);
            listButtonPool.onCheckIn = CheckIn;
            listButtonPool.onCheckOut = CheckOut;
        }

        private void SetButtonText(string a = "")
        {
            buttonA.gameObject.SetActive(string.IsNullOrEmpty(a) == false);
            buttonA.GetComponentInChildren<Text>().text = a;
        }

        public static void ShowListEditModal(
            string headerText,
            IList propertyList,
            Action<IList> callback)
        {
            instance.headerText.text = headerText;
            instance.SetButtonText(a: "Done");
            instance.editButtonsContainer.SetActive(true);

            Type listType = instance.CheckListType(propertyList);

            if (listType == typeof(int))
            {
                instance.valueList = new List<int>();
            }
            else if (listType == typeof(string))
            {
                instance.valueList = new List<string>();
            }
            else if (listType == typeof(bool))
            {
                instance.valueList = new List<bool>();
            }

            foreach (var t in propertyList)
            {
                instance.valueList.Add(t);
            }
            
            instance.itemList = new List<SimpleListInput>();
            
            instance.callback = callback;

            instance.gameObject.SetActive(true);

            instance.RebuildList();
        }
        
        public Type CheckListType(IList list)
        {
            Type listType = list.GetType();

            if (listType.IsGenericType)
            {
                return listType.GetGenericArguments()[0];
            }

            Debug.LogError("List is not generic");
            return null;
        }

        private void HandleButtons()
        {
            Action<IList> tempCallback = callback;
            
            itemList = null;

            selectedItem = null;
            
            callback = null;

            foreach (Transform listItemButton in listWidgetArea)
            {
                listButtonPool.CheckIn(listItemButton.gameObject);
            }

            gameObject.SetActive(false);

            tempCallback?.Invoke(valueList);
        }

        private void DirectionPressed(bool up)
        {
            if(selectedItem == null) return;
            
            int index = itemList.IndexOf(selectedItem);
            
            if(index == -1) return;
            if(up && index == 0) return;
            if(!up && index == itemList.Count - 1) return;

            SelectItem(null);
            
            int otherIndex = up ? index - 1 : index + 1;

            var temp = valueList[otherIndex];
            valueList[otherIndex] = valueList[index];
            valueList[index] = temp;
            //(stringList[index], stringList[otherIndex]) = (stringList[otherIndex], stringList[index]);
            
            itemList[otherIndex].SetValue(valueList[otherIndex]);
            itemList[index].SetValue(valueList[index]);
            
            SelectItem(itemList[otherIndex]);
        }

        private void DeletePressed()
        {
            if(selectedItem == null) return;
            
            int index = itemList.IndexOf(selectedItem);
            
            if(index == -1) return;
            
            valueList.RemoveAt(index);
            RebuildList();
            
            SelectItem(null);
        }

        private void AddPressed()
        {
            valueList.Add("New empty");
            RebuildList();
            
            if (itemList.Count > 0)
            {
                SelectItem(itemList[itemList.LastIndex()]);
            }  
            else
            {
                selectedItem = null;
            }
        }

        private void RebuildList()
        {
            //clear list area
            foreach (Transform listItemButton in listWidgetArea)
            {
                if (listItemButton.gameObject.activeSelf)
                {
                    listButtonPool.CheckIn(listItemButton.gameObject);
                }
            }
            
            itemList.Clear();
            
            foreach (object t in valueList)
            {
                GameObject newListItem = listButtonPool.CheckOut();
                
                newListItem.transform.SetAsLastSibling();
                
                SimpleListInput input = newListItem.GetComponentInChildren<SimpleListInput>();
                input.SetValue(t);
                input.AddListener(SelectItem, ValueChange);
                itemList.Add(input);
            }
        }

        private void SelectItem(SimpleListInput item)
        {
            if(selectedItem != null)
                selectedItem.SetButtonState(false);
            
            selectedItem = item;
            
            if(selectedItem !=null)
                selectedItem.SetButtonState(true);
        }

        private void ValueChange()
        {
            if(selectedItem == null) return;
            
            int index = itemList.IndexOf(selectedItem);
            
            if(index == -1) return;
            
            valueList[index] = selectedItem.GetValue();
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
        }

        private void CheckOut(GameObject listItem)
        {
            listItem.SetActive(true);
            SetButtonState(listItem, false);
        }

        public void SetButtonState(GameObject listItem, bool active)
        {
            listItem.GetComponentInChildren<SimpleListInput>().SetButtonState(active);
        }
    }
}
