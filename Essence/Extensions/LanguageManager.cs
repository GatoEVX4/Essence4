using Essence.Properties;
using Essence.Translations;
using System.Globalization;
using System.Windows;

namespace Essence
{
    public class LanguageManager
    {
        public LanguageManager()
        {
            if(MainWindow.SettingsManager.Settings.Language == "null")
            {
                string systemLanguage = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
                switch (systemLanguage)
                {
                    case "en":
                        MainWindow.SettingsManager.Settings.Language = "EN";
                        break;

                    case "pt":
                        MainWindow.SettingsManager.Settings.Language = "PT";
                        break;

                    case "es":
                        MainWindow.SettingsManager.Settings.Language = "ES";
                        break;

                    default:
                        MainWindow.SettingsManager.Settings.Language = "default";
                        break;
                }
            }
            switch (MainWindow.SettingsManager.Settings.Language)
            {
                case "EN":
                    lang = 1;
                    break;

                case "PT":
                    lang = 2;
                    break;

                case "ES":
                    lang = 3;
                    break;

                default:
                    lang = 1;
                    break;
            }
        }

        public int lang = 1;
        public string Translate(string key)
        {
            if (lang == 1 && !string.IsNullOrEmpty(EN_US.ResourceManager.GetString(key)))
                return EN_US.ResourceManager.GetString(key);

            else if (lang == 2 && !string.IsNullOrEmpty(PT_BR.ResourceManager.GetString(key)))
                return PT_BR.ResourceManager.GetString(key);

            else if (lang == 3 && !string.IsNullOrEmpty(EN_US.ResourceManager.GetString(key)))
                return ES_ES.ResourceManager.GetString(key);

            else
                return key;
        }

    }
}