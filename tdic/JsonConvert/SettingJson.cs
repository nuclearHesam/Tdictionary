using System.IO;
using Newtonsoft.Json;

namespace tdic.SettingJson
{
    public class LanguageSettings
    {
        public string MainLanguage { get; set; } // "English , Persian , null"
        public string ButtonsLanguage { get; set; } // "English , Persian"
        public string TextBlockLanguage { get; set; } // "English , Persian"
        public string MessageBoxLanguage { get; set; } // "English , Persian"
    }

    public class Settings
    {
        public LanguageSettings LanguageSettings { get; set; }
    }

    static class Serializer
    {
        public static void WriteSettingJson(Settings settingItem)
        {
            string jsonFileName = "settings.json";

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(settingItem);

            using (var fileStream = new FileStream(jsonFileName, FileMode.Create))
            using (var streamWriter = new StreamWriter(fileStream))
            using (var jsonWriter = new JsonTextWriter(streamWriter))
            {
                jsonWriter.WriteRaw(json);
            }
        }

        public static Settings ReadSettingJson()
        {
            string jsonFileName = "settings.json";

            string jsonText = File.ReadAllText(jsonFileName);
            Settings loadedSettings = Newtonsoft.Json.JsonConvert.DeserializeObject<Settings>(jsonText);

            return loadedSettings;
        }
    }
}
