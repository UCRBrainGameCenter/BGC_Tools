using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using BGC.DataStructures.Generic;
using BGC.Extensions;

namespace BGC.UI.Dialogs
{
    public class PrimitiveListModalDialog : MonoBehaviour
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
        private GameObject listItemString = null;

        private static PrimitiveListModalDialog instance;
        
        private IList valueList = null;
        private Type listType;
        
        private List<SimpleListInput> itemList;
        private SimpleListInput selectedItem;

        private Action<IList> callback;

        private ConstructingPool<GameObject> listButtonPool;

        public PrimitiveListModalDialog()
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
            listButtonPool.onCheckOut = CheckOut;
            listButtonPool.onCheckIn = CheckIn;
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

             instance.listType = instance.CheckListType(propertyList);

            if (instance.listType == typeof(int))
            {
                instance.valueList = new List<int>();
            }
            else if (instance.listType == typeof(string))
            {
                instance.valueList = new List<string>();
            }
            else if (instance.listType == typeof(double))
            {
                instance.valueList = new List<double>();
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
            
            foreach (Transform listItem in listWidgetArea)
            {
                listButtonPool.CheckIn(listItem.gameObject);
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
            itemList.RemoveAt(index);

            Transform target = selectedItem.transform.parent;
            listButtonPool.CheckIn(target.gameObject);
            target.SetAsLastSibling();
            
            SelectItem(null);
        }

        private void AddPressed()
        {
            if (listType == typeof(int))
            {
                valueList.Add(0);
            }
            else if (listType == typeof(string))
            {
                valueList.Add("New empty");
            }
            else if (listType == typeof(double))
            {
                valueList.Add(0.0);
            }
           
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
            SelectItem(null);
            
            //clear list area
            foreach (Transform listItem in listWidgetArea)
            {
                if (listItem.gameObject.activeSelf)
                {
                    listButtonPool.CheckIn(listItem.gameObject);
                }
            }
            
            itemList.Clear();
            
            foreach (object t in valueList)
            {
                GameObject newListItem = listButtonPool.CheckOut();
                
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

            if (listType == typeof(int))
            {
                if (int.TryParse(selectedItem.GetValue().ToString(), out int result))
                {
                    valueList[index] = result;
                    return;
                }

                valueList[index] = 0;
                selectedItem.SetValue("0");
            }
            else if (listType == typeof(string))
            {
                valueList[index] = selectedItem.GetValue();
            }
            else if (listType == typeof(double))
            {
                string input = selectedItem.GetValue().ToString();

                // Check if it's an in-progress double (e.g. "3.", ".")
                bool looksLikeDouble = Regex.IsMatch(input, @"^\d*\.$|^\.$");

                if (double.TryParse(input, out double result))
                {
                    valueList[index] = result;
                }
                else if (!looksLikeDouble)
                {
                    valueList[index] = 0.0;
                    selectedItem.SetValue("0");
                }
            }
        }

        private GameObject BuildListItem()
        {
            GameObject temp = Instantiate(listItemString);;
            
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
            listItem.transform.SetAsLastSibling();
        }

        public void SetButtonState(GameObject listItem, bool active)
        {
            if(listType != null)
                listItem.GetComponentInChildren<SimpleListInput>().SetButtonState(active);
        }
    }
}
