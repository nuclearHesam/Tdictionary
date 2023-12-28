using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using tdic.DataContext;
using tdic.SettingJson;
using tdic.WordsRepository;
using WordsDBModelView;
using WordsListedModelView;
using System.Windows.Documents;
using System.Windows.Media;

namespace tdic
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        List<Words> words;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // create setting json file (basic setting) 
            if (!File.Exists("settings.json"))
            {
                LanguageSettings languageSettings = new()
                {
                    MainLanguage = "English",
                    ButtonsLanguage = "English",
                    TextBlockLanguage = "English",
                    MessageBoxLanguage = "English",
                };

                var Settings = new Settings { LanguageSettings = languageSettings };

                Serializer.WriteSettingJson(Settings);
            }

            // create db file
            if (!File.Exists("Words.db"))
            {
                using (UnitOfWork db = new())
                {
                    db.WordsRepository.CreateSqliteFie();
                }
            }

            BindListBox();

            SetLanaguageSetting();
        }

        #region Toolbar

        private void Setting_btn_Click(object sender, RoutedEventArgs e)
        {
            Setting setting = new();
            if (setting.ShowDialog() == true)
            {
                SetLanaguageSetting();
            }
        }

        private void Help_btn_Click(object sender, RoutedEventArgs e)
        {
            Help help = new();
            help.ShowDialog();
        }

        #endregion

        #region Buttons

        private void Add_new_Word_btn_Click(object sender, RoutedEventArgs e)
        {
            AddWord addWord = new();
            addWord.ShowDialog();
            BindListBox();
        }

        private void Words_btn_Click(object sender, RoutedEventArgs e)
        {
            WordsPage words = new();
            words.Show();
            BindListBox();
        }

        #endregion

        private void Words_lbx_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (Words_lbx.SelectedItem != null)
            {
                if (Words_lbx.SelectedItem is Words selectedWord)
                {
                    string wordID = selectedWord.WordID;
                    using (UnitOfWork db = new())
                    {
                       var word = db.WordsRepository.ReadWord(wordID);
                        var phonetics = db.WordsRepository.ReadPhonetics(wordID);
                        var meanings = db.WordsRepository.ReadMeanings(wordID);
                        List<Definitions> definitions = new();
                        foreach (var meaning in meanings)
                        {
                            var definition = db.WordsRepository.ReadDefinitions(meaning.MeaningID);
                            foreach (var item in definition)
                            {
                                definitions.Add(item);
                            }
                        }

                        var Listedword = ListedModelConvertor.WordsConvertor(word, phonetics, meanings, definitions);
                        SetWordTreeView(Listedword);
                    }
                }
            }
        }

        private void Words_lbx_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Words_lbx.SelectedItem != null)
            {
                if (Words_lbx.SelectedItem is Words selectedWord)
                {
                    string wordID = selectedWord.WordID;
                    WordPage wordPage = new(wordID);
                    wordPage.ShowDialog();
                    BindListBox();
                }
            }
        }

        void SetWordTreeView(Word word)
        {
            Word_trw.Items.Clear();

            // Add the root node
            TreeViewItem rootNode = new()
            {
                Header = $"{word.English} ({word.Translation})"
            };
            Word_trw.Items.Add(rootNode);

            // Add meanings as child nodes
            foreach (var meaning in word.Meanings)
            {
                TreeViewItem meaningNode = new()
                {
                    Header = $"{meaning.PartOfSpeech}"
                };
                rootNode.Items.Add(meaningNode);

                // Add definitions as child nodes
                int count = 1;
                foreach (var definition in meaning.Definitions)
                {
                    TreeViewItem definitionNode = new()
                    {
                        Header = $"{count++}. {definition.definition}"
                    };
                    meaningNode.Items.Add(definitionNode);

                    if (definition.Example != null && definition.Example != "")
                    {
                        TreeViewItem exampleNode = new()
                        {
                            Header = definition.Example
                        };
                        definitionNode.Items.Add(exampleNode);
                    }
                }
            }

            // Add phonetics as child nodes
            foreach (var phonetic in word.Phonetics)
            {
                TreeViewItem phoneticNode = new()
                {
                    Header = $"Phonetic({phonetic.Language}): {phonetic.Text}"
                };
                rootNode.Items.Add(phoneticNode);
            }

            // Expand the root node
            rootNode.IsExpanded = true;
        }

        private void PRONOUN_rbt_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton? radioButton = e.Source as RadioButton;
                switch (radioButton.Name)
                {
                    case "PRONOUN_rbt":
                        {
                            SetWordByFilter("pronoun");
                        }
                        break;
                    case "VERB_rbt":
                        {
                            SetWordByFilter("verb");
                        }
                        break;
                    case "NOUN_rbt":
                        {
                            SetWordByFilter("noun");
                        }
                        break;
                    case "ADJECTIVE_rbt":
                        {
                            SetWordByFilter("adjective");
                        }
                        break;
                    case "ADVERB_rbt":
                        {
                            SetWordByFilter("adverb");
                        }
                        break;
                    case "PREPOSITION_rbt":
                        {
                            SetWordByFilter("preposition");
                        }
                        break;
                    case "CONJUNCTION_rbt":
                        {
                            SetWordByFilter("conjunction");
                        }
                        break;
                    case "INTERJECTION_rbt":
                        {
                            SetWordByFilter("interjection");
                        }
                        break;
                    default:
                        {

                        }
                        break;
            }

            void SetWordByFilter(string filter)
            {
                using (UnitOfWork db = new())
                {
                    words = db.WordsRepository.ReadWordByFilter(filter);
                    Words_lbx.ItemsSource = words;
                    Word_trw.Items.Clear();
                }
            }
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink hl = e.Source as Hyperlink;
            string NavigateUri = hl.NavigateUri.OriginalString;

            var sortedWord = words.FindAll(word => word.English.ToLower().StartsWith(NavigateUri));

            Words_lbx.ItemsSource = sortedWord;
        }

        private void txt_Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filter = txt_Search.Text.ToLower();
            var fwords = words.FindAll(word => word.English.ToLower().StartsWith(filter));

            Word_trw.Items.Clear();
            Words_lbx.ItemsSource = fwords;
        }

        private void btn_Reset_Click(object sender, RoutedEventArgs e)
        {
            BindListBox();
            txt_Search.Text = "";
            PRONOUN_rbt.IsChecked = false;
            VERB_rbt.IsChecked = false;
            NOUN_rbt.IsChecked = false;
            ADJECTIVE_rbt.IsChecked = false;
            ADVERB_rbt.IsChecked = false;
            PREPOSITION_rbt.IsChecked = false;
            CONJUNCTION_rbt.IsChecked = false;
            INTERJECTION_rbt.IsChecked = false;
        }

        private void BindListBox()
        {
            using (UnitOfWork db = new())
            {
                words = db.WordsRepository.ReadLimit100Words();
            }
            Words_lbx.ItemsSource = words;
            Word_trw.Items.Clear();
        }

        void ChangeLanguageSetting(LanguageSettings languageSettings)
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
                    grd_RadioButtons.FlowDirection = FlowDirection.LeftToRight;
                    this.Title = "TDictionary";
                }
                else if (languageSettings.ButtonsLanguage == "Persian")
                {
                    grd_RadioButtons.FlowDirection = FlowDirection.RightToLeft;
                    this.Title = "تی دیکشنری";
                }
            }

            void _ButtonsLanguage(string Language)
            {
                if (Language == "English")
                {
                    INTERJECTION_rbt.FontFamily = ADJECTIVE_rbt.FontFamily = CONJUNCTION_rbt.FontFamily = NOUN_rbt.FontFamily = PREPOSITION_rbt.FontFamily = VERB_rbt.FontFamily = ADVERB_rbt.FontFamily = PRONOUN_rbt.FontFamily = Help_btn.FontFamily = Setting_btn.FontFamily = FindResource("Proxima Medium") as FontFamily;
                    Image_Gallery_btn.FontFamily = Translate_btn.FontFamily = Daily_Practice_btn.FontFamily = Daily_Course_btn.FontFamily = Words_btn.FontFamily = Add_new_Word_btn.FontFamily = FindResource("Proxima ExtraBold") as FontFamily;

                    Setting_btn.Content = "Setting";
                    Help_btn.Content = "Help";
                    Add_new_Word_btn.Content = "Add new Word";
                    Words_btn.Content = "Words";
                    Daily_Course_btn.Content = "Daily Course";
                    Daily_Practice_btn.Content = "Daily Practice";
                    Translate_btn.Content = "Translate";
                    Image_Gallery_btn.Content = "Image Gallary";
                    PRONOUN_rbt.Content = "PRONOUN";
                    ADVERB_rbt.Content = "ADVERB";
                    VERB_rbt.Content = "VERB";
                    PREPOSITION_rbt.Content = "PREPOSITION";
                    NOUN_rbt.Content = "NOUN";
                    CONJUNCTION_rbt.Content = "CONJUNCTION";
                    ADJECTIVE_rbt.Content = "AJECTIVE";
                    INTERJECTION_rbt.Content = "INTERJECTION";
                }
                else if (languageSettings.ButtonsLanguage == "Persian")
                {
                    INTERJECTION_rbt.FontFamily = ADJECTIVE_rbt.FontFamily = CONJUNCTION_rbt.FontFamily = NOUN_rbt.FontFamily = PREPOSITION_rbt.FontFamily = VERB_rbt.FontFamily = ADVERB_rbt.FontFamily = PRONOUN_rbt.FontFamily = Help_btn.FontFamily = Setting_btn.FontFamily = FindResource("Vazir") as FontFamily;
                    Image_Gallery_btn.FontFamily = Translate_btn.FontFamily = Daily_Practice_btn.FontFamily = Daily_Course_btn.FontFamily = Words_btn.FontFamily = Add_new_Word_btn.FontFamily = FindResource("Vazir Bold") as FontFamily;

                    Setting_btn.Content = "تنظیمات";
                    Help_btn.Content = "کمک";
                    Add_new_Word_btn.Content = "افزودن کلمه جدید";
                    Words_btn.Content = "کلمه ها";
                    Daily_Course_btn.Content = "دوره روزانه";
                    Daily_Practice_btn.Content = "تمرین روزانه";
                    Translate_btn.Content = "ترجمه";
                    Image_Gallery_btn.Content = "گالری تصاویر";
                    PRONOUN_rbt.Content = "ضمیر";
                    ADVERB_rbt.Content = "قید";
                    VERB_rbt.Content = "فعل";
                    PREPOSITION_rbt.Content = "حرف اضافه";
                    NOUN_rbt.Content = "اسم";
                    CONJUNCTION_rbt.Content = "حرف ربط";
                    ADJECTIVE_rbt.Content = "صفت";
                    INTERJECTION_rbt.Content = "حرف ندا";
                }
            }

            void _TextBlockLanguage(string Language)
            {
                if (Language == "English")
                {
                    txt_Search.FontFamily = Search_txb.FontFamily = FindResource("Proxima Medium") as FontFamily;
                    
                    Search_txb.Text = "Search:";
                }
                else if (languageSettings.TextBlockLanguage == "Persian")
                {
                    txt_Search.FontFamily = Search_txb.FontFamily = FindResource("Vazir") as FontFamily;

                    Search_txb.Text = "جست و جو:";
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

        private void SetLanaguageSetting()
        {
            var Settingslang = Serializer.ReadSettingJson();
            ChangeLanguageSetting(Settingslang.LanguageSettings);
        }
    }
}
