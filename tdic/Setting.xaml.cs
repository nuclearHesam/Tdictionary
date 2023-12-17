using System.Windows;
using tdic.SettingJson;

namespace tdic
{
    /// <summary>
    /// Interaction logic for Setting.xaml
    /// </summary>
    public partial class Setting : Window
    {
        public Setting()
        {
            InitializeComponent();
        }
        Settings settings = new Settings();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _SetLanaguageSetting();
        }

        #region check boxes

        private void Custom_Language_cbx_Checked(object sender, RoutedEventArgs e)
        {
            Custom_Language_gbx.Visibility = Visibility.Visible;
            Main_Language_cmx.IsEnabled = false;
        }

        private void Custom_Language_cbx_Unchecked(object sender, RoutedEventArgs e)
        {
            Custom_Language_gbx.Visibility = Visibility.Collapsed;
            Main_Language_cmx.IsEnabled = true;
        }

        #endregion

        private void Save_setting_btn_Click(object sender, RoutedEventArgs e)
        {
            LanguageSettings languageSettings = new LanguageSettings();
            // Mian Language
            if (Custom_Language_cbx.IsChecked != true)
            {
                if (Main_Language_cmx.SelectedIndex == 1)
                {
                    languageSettings.MainLanguage = "English";
                    languageSettings.ButtonsLanguage = "English";
                    languageSettings.TextBlockLanguage = "English";
                    languageSettings.MessageBoxLanguage = "English";
                }
                else if (Main_Language_cmx.SelectedIndex == 0)
                {
                    languageSettings.MainLanguage = "Persian";
                    languageSettings.ButtonsLanguage = "Persian";
                    languageSettings.TextBlockLanguage = "Persian";
                    languageSettings.MessageBoxLanguage = "Persian";
                }
            }
            // Custom Lnaguage
            else
            {
                languageSettings.MainLanguage = "null";
                languageSettings.ButtonsLanguage = buttons_Language_cmx.SelectedValue.ToString().Split(" ")[1];
                languageSettings.TextBlockLanguage = TextBlock_Language_cmx.SelectedValue.ToString().Split(" ")[1];
                languageSettings.MessageBoxLanguage = MessageBox_Language_cmx.SelectedValue.ToString().Split(" ")[1];
            }

            settings.LanguageSettings = languageSettings;
            Serializer.WriteSettingJson(settings);

            DialogResult = true;

            _SetLanaguageSetting();
        }

        void _ChangeLanguageSetting(LanguageSettings languageSettings)
        {
            if (languageSettings.MainLanguage != "null")
            {
                Custom_Language_gbx.Visibility = Visibility.Collapsed;
                Main_Language_cmx.IsEnabled = true;
                Custom_Language_cbx.IsChecked = false;

                _Page(languageSettings.MainLanguage);
                _ButtonsLanguage(languageSettings.MainLanguage);
                _TextBlockLanguage(languageSettings.MainLanguage);
                _MessageBoxLanguage(languageSettings.MainLanguage);
            }
            else
            {
                Custom_Language_gbx.Visibility = Visibility.Visible;
                Main_Language_cmx.IsEnabled = false;
                Custom_Language_cbx.IsChecked = true;

                _ButtonsLanguage(languageSettings.ButtonsLanguage);
                _TextBlockLanguage(languageSettings.TextBlockLanguage);
                _MessageBoxLanguage(languageSettings.MessageBoxLanguage);
            }

            void _Page(string Language)
            {
                if (Language == "English")
                {
                    this.Title = "Settings";
                    Language_tbi.Header = "Language";
                    Wallpaper_tbi.Header = "Wallpaper";
                    Custom_Language_cbx.Content = "Custom Language";
                    Custom_Language_gbx.Header = "Custom Language";

                    Main_Language_cmx.SelectedIndex = 1;
                }
                else if (languageSettings.ButtonsLanguage == "Persian")
                {
                    this.Title = "تنظیمات";
                    Language_tbi.Header = "زبان";
                    Wallpaper_tbi.Header = "کاغذ دیواری";
                    Custom_Language_cbx.Content = "زبان سفارشی";
                    Custom_Language_gbx.Header = "زبان سفارشی";

                    Main_Language_cmx.SelectedIndex = 0;
                }
            }

            void _ButtonsLanguage(string Language)
            {
                if (Language == "English")
                {
                    Save_setting_btn.Content = "Save";

                    Main_Language_cmx.SelectedIndex = 1;
                }
                else if (languageSettings.ButtonsLanguage == "Persian")
                {
                    Save_setting_btn.Content = "ذخیره";

                    Main_Language_cmx.SelectedIndex = 0;
                }
            }

            void _TextBlockLanguage(string Language)
            {
                if (Language == "English")
                {
                    Main_Language_txb.Text = "Language:";
                    button_Language_txb.Text = "buttons Language:";
                    Texts_Language_txb.Text = "Texts Language:";
                    MessageBox_Language_txb.Text = "MessageBox Language:";

                    Main_Language_cmx.SelectedIndex = 1;
                }
                else if (languageSettings.TextBlockLanguage == "Persian")
                {
                    Main_Language_txb.Text = "زبان:";
                    button_Language_txb.Text = "زبان دکمه ها:";
                    Texts_Language_txb.Text = "زبان متون:";
                    MessageBox_Language_txb.Text = "زبان پیام ها:";

                    Main_Language_cmx.SelectedIndex = 0;
                }
            }

            void _MessageBoxLanguage(string Language)
            {
                if (Language == "English")
                {
                    Main_Language_cmx.SelectedIndex = 1;
                }
                else if (languageSettings.MessageBoxLanguage == "Persian")
                {
                    Main_Language_cmx.SelectedIndex = 0;
                }
            }
        }

        private void _SetLanaguageSetting()
        {
            var Settingslang = Serializer.ReadSettingJson();
            _ChangeLanguageSetting(Settingslang.LanguageSettings);
        }
    }
}
