using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Assertions;

namespace BGC.Utility.FileBrowser
{
    /// <summary>
    /// Script for scene that allows users to view files in a local project
    /// </summary>
    public class FileBrowser : MonoBehaviour
    {
        public static string ReturnToScene = "";

        [SerializeField]
        private GameObject navigationPanel = null;
        [SerializeField]
        private GameObject navigationContent = null;
        [SerializeField]
        private GameObject fileContent = null;

        [SerializeField]
        private Button parentDirectoryButton = null;
        [SerializeField]
        private Button toggleNavigationButton = null;
        [SerializeField]
        private Button mainMenuButton = null;
        [SerializeField]
        private Button buttonPrefab = null;
        [SerializeField]
        private Button asccendingOrDescendingButton = null;

        [SerializeField]
        private Text pathText = null;
        [SerializeField]
        private Text fileName = null;

        [SerializeField]
        private Text fileTextPrefab = null;

        private List<string> childDirectories = new List<string>();
        private List<string> childFiles = new List<string>();

        private static string currentDirectory;

        // Max characters a single Text UI object can have
        private const int CharLimit = 16250;

        // Extensions that a user can actually open in the viewer. All files will be show in the browser, however.
        private static readonly string[] AcceptableExtensions =
        {
            ".txt", ".json", ".bgc", ".int", ".float", ".str", ".user"
        };

        private Text ascendingOrderButtonText = null;
        private bool ascendingOrder = false;

        private void Awake()
        {
            Assert.IsNotNull(asccendingOrDescendingButton);
            Assert.IsNotNull(toggleNavigationButton);
            Assert.IsNotNull(parentDirectoryButton);
            Assert.IsNotNull(navigationContent);
            Assert.IsNotNull(navigationPanel);
            Assert.IsNotNull(mainMenuButton);
            Assert.IsNotNull(fileTextPrefab);
            Assert.IsNotNull(buttonPrefab);
            Assert.IsNotNull(fileContent);
            Assert.IsNotNull(pathText);
            Assert.IsNotNull(fileName);

            ascendingOrderButtonText = asccendingOrDescendingButton.GetComponentInChildren<Text>();
            Assert.IsNotNull(ascendingOrderButtonText);

            asccendingOrDescendingButton.onClick.AddListener(ToggleDisplayOrder);
            toggleNavigationButton.onClick.AddListener(ToggleNavPanel);
            parentDirectoryButton.onClick.AddListener(MoveUpDirectory);
            mainMenuButton.onClick.AddListener(GoBack);

            OpenDirectory(IO.DataManagement.RootDirectory);
        }

        private void OnDestroy()
        {
            toggleNavigationButton.onClick.RemoveListener(GoBack);
        }

        private void GoBack()
        {
            Assert.IsFalse(string.IsNullOrEmpty(ReturnToScene));
            SceneManager.LoadScene(ReturnToScene);
        }

        private void ToggleDisplayOrder()
        {
            ascendingOrder = !ascendingOrder;

            if (ascendingOrder)
            {
                ascendingOrderButtonText.text = "ASC";
            }
            else
            {
                ascendingOrderButtonText.text = "DESC";
            }

            OpenDirectory(currentDirectory);
        }

        private void ToggleNavPanel()
        {
            navigationPanel.SetActive(!navigationPanel.activeInHierarchy);
        }

        private void MoveUpDirectory()
        {
            if (Path.GetFullPath(currentDirectory) != Path.GetFullPath(IO.DataManagement.RootDirectory))
            {
                OpenDirectory(Directory.GetParent(currentDirectory).FullName);
            }
        }

        private void UpdateChildren()
        {
            ClearChildren(navigationContent);
            childFiles.Clear();

            childDirectories = Directory.GetDirectories(currentDirectory).ToList();
            if (ascendingOrder)
            {
                childDirectories.Sort((a, b) => -1 * a.CompareTo(b));
            }
            else
            {
                childDirectories.Sort();
            }

            for (int i = 0; i < childDirectories.Count; i++)
            {
                Button dir = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity, navigationContent.transform);
                dir.GetComponentInChildren<Text>().text = $"<b>{Path.GetFileName(childDirectories[i])}</b>";
                string directory = Path.GetFullPath(childDirectories[i]);
                dir.onClick.AddListener(() => OpenDirectory(directory));
            }

            childFiles.AddRange(Directory.GetFiles(currentDirectory));
            if (ascendingOrder)
            {
                childFiles.Sort((a, b) => -1 * a.CompareTo(b));
            }
            else
            {
                childFiles.Sort();
            }

            for (int i = 0; i < childFiles.Count; i++)
            {
                Button file = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity, navigationContent.transform);
                Text label = file.GetComponentInChildren<Text>();
                label.text = $"<i>{Path.GetFileName(childFiles[i])}</i>";

                if (AcceptableExtensions.Contains(Path.GetExtension(childFiles[i])))
                {
                    string path = Path.GetFullPath(childFiles[i]);
                    file.onClick.AddListener(() => OpenFile(path));
                }
                else
                {
                    label.color = Color.gray;
                }
            }
        }

        private void ClearChildren(GameObject obj)
        {
            int children = obj.transform.childCount;
            for (int i = children - 1; i >= 0; i--)
            {
                Destroy(obj.transform.GetChild(i).gameObject);
            }
        }

        private void OpenDirectory(string path)
        {
            currentDirectory = path;
            UpdateChildren();

            // set UI for directory path
            string directory = currentDirectory.Remove(0, IO.DataManagement.RootDirectory.Length);
            directory = directory.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            pathText.text = $"Path: {directory}/";
        }

        private void OpenFile(string path)
        {
            string currentFileText = File.ReadAllText(path);
            ClearChildren(fileContent);

            for (int i = 0; i < currentFileText.Length; i += CharLimit)
            {
                Text fileText = Instantiate(fileTextPrefab, Vector3.zero, Quaternion.identity, fileContent.transform);
                fileText.text = currentFileText.Substring(i, Math.Min(CharLimit - 1, currentFileText.Length - i));
            }

            fileName.text = $"({Path.GetFileName(path)})";
        }
    }
}