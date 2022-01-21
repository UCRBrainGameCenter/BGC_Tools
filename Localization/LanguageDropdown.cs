using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace BGC.Localization
{
    public class LanguageDropdown : MonoBehaviour
    {
        public Text languageString;
        public Text titleString;

        private string[] allLanguages;
        private int currentLocInList;
        public LocalizationSystem.Language currentlySelectedLanguage;

        // Start is called before the first frame update
        void Start()
        {
            // Generate list of available Locales
            int selected = 0;
            allLanguages = System.Enum.GetNames(typeof(LocalizationSystem.Language));

            for (int i = 0; i < allLanguages.Length; ++i)
            {
                var locale = allLanguages[i];
                if (LocalizationSystem.language.ToString() == locale)
                    selected = i;
            }

            currentLocInList = selected;
            languageString.text = allLanguages[selected];
            titleString.text = LocalizationSystem.GetLocalizedValue("Language") + " :";
        }

        static void LocaleSelected(int index)
        {
            var languages = System.Enum.GetNames(typeof(LocalizationSystem.Language));
            if (System.Enum.TryParse(languages[index], out LocalizationSystem.Language chosenLanguage))
            {
                LocalizationSystem.language = chosenLanguage;
            }
            else
            {
                Debug.Log("Error finding language");
            }
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }

        public void NextLanguage()
        {
            currentLocInList++;
            if (currentLocInList >= allLanguages.Length)
            {
                currentLocInList = 0;
            }
            languageString.text = allLanguages[currentLocInList];
            if (System.Enum.TryParse(allLanguages[currentLocInList], out LocalizationSystem.Language chosenLanguage))
            {
                LocalizationSystem.language = chosenLanguage;
            }
        }

        public void ReloadScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }
    }

}