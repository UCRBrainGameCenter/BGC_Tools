using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using System.IO;

namespace BGC.Utility.FileBrowser
{
    /// <summary>
    /// Script for scene that allows users to view files in a local project
    /// </summary>
    public class FileBrowser : MonoBehaviour
    {
        public static string ReturnToScene;

        [SerializeField]
        private GameObject navigationPanel;
        [SerializeField]
        private GameObject navigationContent;
        [SerializeField]
        private GameObject fileContent;

        [SerializeField]
        private Button parentDirectoryButton;
        [SerializeField]
        private Button toggleNavigationButton;
        [SerializeField]
        private Button mainMenuButton;
        [SerializeField]
        private Button buttonPrefab;

        [SerializeField]
        private Text pathText;
        [SerializeField]
        private Text fileName;

        [SerializeField]
        private Text fileTextPrefab;

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

        private void Awake()
        {
            parentDirectoryButton.onClick.AddListener(MoveUpDirectory);
            toggleNavigationButton.onClick.AddListener(ToggleNavPanel);
            mainMenuButton.onClick.AddListener(GoBack);

            OpenDirectory(BGC.IO.DataManagement.RootDirectory);
        }

        private void OnDestroy()
        {
            toggleNavigationButton.onClick.RemoveListener(GoBack);
        }

        private void GoBack()
        {
            Assert.IsFalse(System.String.IsNullOrEmpty(ReturnToScene));
            SceneManager.LoadScene(ReturnToScene);
        }

        private void ToggleNavPanel()
        {
            navigationPanel.SetActive(!navigationPanel.activeInHierarchy);
        }

        private void MoveUpDirectory()
        {
            if (Path.GetFullPath(currentDirectory) != Path.GetFullPath(BGC.IO.DataManagement.RootDirectory))
            {
                OpenDirectory(Directory.GetParent(currentDirectory).FullName);
            }
        }

        private void UpdateChildren()
        {
            ClearChildren(navigationContent);
            childFiles.Clear();

            childDirectories = Directory.GetDirectories(currentDirectory).ToList();
            for (int i = 0; i < childDirectories.Count; i++)
            {
                Button dir = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity);
                dir.transform.SetParent(navigationContent.transform);
                dir.GetComponentInChildren<Text>().text = $"<b>{Path.GetFileName(childDirectories[i])}</b>";
                string directory = Path.GetFullPath(childDirectories[i]);
                dir.onClick.AddListener(() => OpenDirectory(directory));
            }

            childFiles.AddRange(Directory.GetFiles(currentDirectory));
            for (int i = 0; i < childFiles.Count; i++)
            {
                Button file = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity);
                file.transform.SetParent(navigationContent.transform);
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
                Destroy(obj.transform.GetChild(i).gameObject); //Destroy the child.
            }
        }

        private void OpenDirectory(string path)
        {
            currentDirectory = path;
            UpdateChildren();
            UpdatePathText();
        }

        private void OpenFile(string path)
        {
            string currentFileText = File.ReadAllText(path);
            ClearChildren(fileContent);

            for (int i = 0; i < currentFileText.Length; i += CharLimit)
            {
                Text fileText = Instantiate(fileTextPrefab, Vector3.zero, Quaternion.identity);
                fileText.text = currentFileText.Substring(i, Mathf.Min(CharLimit - 1, currentFileText.Length - i));
                fileText.transform.SetParent(fileContent.transform);
            }

            fileName.text = $"({Path.GetFileName(path)})";
        }

        private void UpdatePathText()
        {
            // @todo: make this cleaner
            pathText.text = "Path: " + currentDirectory.Remove(0, BGC.IO.DataManagement.RootDirectory.Length)
                                .Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + "/";
        }
    }
}