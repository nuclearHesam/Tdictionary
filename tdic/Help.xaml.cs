
using System.Windows;
using tdic.SettingJson;

namespace tdic
{
    /// <summary>
    /// Interaction logic for Help.xaml
    /// </summary>
    public partial class Help : Window
    {
        public Help()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _SetLanaguageSetting(); 
        }

        void _ChangeLanguageSetting(LanguageSettings languageSettings)
        {
            if (languageSettings.MainLanguage != "null")
            {
                _Page(languageSettings.MainLanguage);
                _ButtonsLanguage(languageSettings.MainLanguage);
                _TextBlockLanguage(languageSettings.MainLanguage);
                _MessageBoxLanguage(languageSettings.MainLanguage);
            }
            else
            {
                _ButtonsLanguage(languageSettings.ButtonsLanguage);
                _TextBlockLanguage(languageSettings.TextBlockLanguage);
                _MessageBoxLanguage(languageSettings.MessageBoxLanguage);
            }

            void _Page(string Language)
            {
                if (Language == "English")
                {
                    this.Title = "Help";
                }
                else if (languageSettings.ButtonsLanguage == "Persian")
                {
                    this.Title = "کمک";
                }
            }

            void _ButtonsLanguage(string Language)
            {
                if (Language == "English")
                {

                }
                else if (languageSettings.ButtonsLanguage == "Persian")
                {

                }
                    
            }

            void _TextBlockLanguage(string Language)
            {
                if (Language == "English")
                {
                    help_header_txb.Text = "How can we help you?";
                    help_text_txb.FlowDirection = FlowDirection.LeftToRight;
                    help_text_txb.Text = "Welcome to our dictionary! Here you can translate and define words and phrases as you wish.\nBelow, find a short guide to using our online dictionary.";
                    map_header_txb.Text = "OUR MAIN OFFICE";
                    map_text_txb.Text = "Somewhere on planet earth";
                    phone_header_txb.Text = "PHONE NUMBER";
                    telegram_header_txb.Text = "Telegram";
                    email_header_txb.Text = "Email";
                }
                else if (languageSettings.TextBlockLanguage == "Persian")
                {
                    help_header_txb.Text = "چگونه ما میتوانیم به شما کمک کنیم؟";
                    help_text_txb.FlowDirection = FlowDirection.RightToLeft;
                    help_text_txb.Text = "به فرهنگ لغت ما خوش آمدید! در اینجا می توانید کلمات و عبارات را به دلخواه ترجمه و تعریف کنید.\nدر زیر، راهنمای کوتاهی برای استفاده از فرهنگ لغت آنلاین ما پیدا کنید.";
                    map_header_txb.Text = "دفتر اصلی ما";
                    map_text_txb.Text = "جایی در سیاره زمین";
                    phone_header_txb.Text = "شماره تلفن";
                    telegram_header_txb.Text = "تلگرام";
                    email_header_txb.Text = "پست الکترونیک";
                }
            }

            void _MessageBoxLanguage(string Language)
            {
                if (Language == "English")
                {

                }
                else if (languageSettings.MessageBoxLanguage == "Persian")
                {

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
