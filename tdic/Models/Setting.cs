using System.IO;

namespace Tdictionary.Models
{
    public class Setting
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
    }
}
