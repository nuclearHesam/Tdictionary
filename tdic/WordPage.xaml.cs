using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;
using tdic.DataContext;
using tdic.SettingJson;
using tdic.WordsRepository;
using WordsDBModelView;
using WordsListedModelView;
using System.Diagnostics;
using tdic.ModelViews;

namespace tdic
{
    /// <summary>
    /// Interaction logic for WordPage.xaml
    /// </summary>
    public partial class WordPage : Window
    {
        readonly string WordID;
        public WordPage(string WordID)
        {
            InitializeComponent();
            this.WordID = WordID;
        }
        Word word = new();
        List<Phonetic> phonetics = new();
        List<Meaning> meanings = new();
        Meaning meaning = new() { Definitions = new List<Definition>() };
        byte offset = 0;
        bool _isFirstTime = false;
        string sourceUrl;

        #region MessageBoxLanguage

        MessageBoxOptions mbo_MessageBoxLanguage = new();

        readonly MessageBoxLanguage mbl_DeleteWord = new();

        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var Settingslang = Serializer.ReadSettingJson();
            ChangeLanguageSetting(Settingslang.LanguageSettings);

            uk_speaker_btn.IsEnabled = false;
            us_speaker_btn.IsEnabled = false;
            ca_speaker_btn.IsEnabled = false;
            btn_us_folder.IsEnabled = false;
            btn_uk_folder.IsEnabled = false;
            btn_ca_folder.IsEnabled = false;

            if (this.WordID != null)
            {
                using (UnitOfWork db = new())
                {
                    var worddb = db.WordsRepository.ReadWord(this.WordID);
                    var phoneticsdb = db.WordsRepository.ReadPhonetics(this.WordID);
                    var meaningsdb = db.WordsRepository.ReadMeanings(this.WordID);
                    List<Definitions> definitionsdb = new();

                    foreach (var meaning in meaningsdb)
                    {
                        var definitions = db.WordsRepository.ReadDefinitions(meaning.MeaningID);
                        foreach (var definition in definitions)
                        {
                            definitionsdb.Add(definition);
                        }
                    }

                    word = ListedModelConvertor.WordsConvertor(worddb, phoneticsdb, meaningsdb, definitionsdb);
                    SetWord();
                }
            }
        }

        void SetWord()
        {
            if (word != null)
            {
                this.Title = word.English;
                txt_English.Text = word.English;
                txt_Persian.Text = word.Persian;

                meanings = word.Meanings;
                phonetics = word.Phonetics;
                EnableSpeakers();

                meaning = meanings.FirstOrDefault(meaning => meaning.PartOfSpeech == "noun");
                if (meaning != null)
                {
                    DefinitionCount_txt.Text = meaning.Definitions.Count.ToString();
                    Definition_txt.Text = example_txt.Text = "";

                    if (meaning.Definitions.Count >= 1)
                    {
                        Definition_txt.Text = meaning.Definitions[0].definition.ToString();
                        if (meaning.Definitions[0].Example != null)
                        {
                            example_txt.Text = meaning.Definitions[0].Example.ToString();
                        }
                    }
                }

                if(word.SourceUrl != null)
                {
                    Uri Source = new Uri(word.SourceUrl);
                    url_Hyperlink.NavigateUri = Source;
                }
                else
                {
                    url_Hyperlink.IsEnabled = false;    
                }

                switch (word.Rate)
                {
                    case "1":
                        {
                            starone.Content = "★";
                            startwo.Content = "☆";
                            starthree.Content = "☆";
                            starfour.Content = "☆";
                            starfive.Content = "☆";
                        }
                        break;
                    case "2":
                        {
                            starone.Content = "★";
                            startwo.Content = "★";
                            starthree.Content = "☆";
                            starfour.Content = "☆";
                            starfive.Content = "☆";
                        }
                        break;
                    case "3":
                        {
                            starone.Content = "★";
                            startwo.Content = "★";
                            starthree.Content = "★";
                            starfour.Content = "☆";
                            starfive.Content = "☆";
                        }
                        break;
                    case "4":
                        {
                            starone.Content = "★";
                            startwo.Content = "★";
                            starthree.Content = "★";
                            starfour.Content = "★";
                            starfive.Content = "☆";
                        }
                        break;
                    case "5":
                        {
                            starone.Content = "★";
                            startwo.Content = "★";
                            starthree.Content = "★";
                            starfour.Content = "★";
                            starfive.Content = "★";
                        }
                        break;
                    default:
                        {

                        }
                        break;
                }
            }
        }

        void EnableSpeakers()
        {
            var uk = phonetics.Find(ph => ph.Language == "uk");
            if (uk != null)
            {
                uk_txt.Text = uk.Text;

                if (uk.Audio != null)
                {
                    uk_speaker_btn.IsEnabled = true;
                    btn_uk_folder.IsEnabled = true;
                }
                else
                {
                    btn_uk_folder.IsEnabled = false;
                    uk_speaker_btn.IsEnabled = false;
                }
            }

            var us = phonetics.Find(ph => ph.Language == "us");
            if (us != null)
            {
                us_txt.Text = us.Text;

                if (us.Audio != null)
                {
                    btn_us_folder.IsEnabled = true;
                    us_speaker_btn.IsEnabled = true;
                }
                else
                {
                    btn_us_folder.IsEnabled = false;
                    us_speaker_btn.IsEnabled = false;
                }
            }

            var au = phonetics.Find(ph => ph.Language == "au");
            if (au != null)
            {
                ca_txt.Text = au.Text;

                if (au.Audio != null)
                {
                    btn_ca_folder.IsEnabled = true;
                    ca_speaker_btn.IsEnabled = true;
                }
                else
                {
                    ca_speaker_btn.IsEnabled = false;
                    btn_ca_folder.IsEnabled = false;
                }
            }
        }

        #region Meaning

        private void Back_definition_btn_Click(object sender, RoutedEventArgs e)
        {
            if ((offset - 1) != -1)
            {
                offset--;
                Definition_txt.Text = meaning.Definitions[offset].definition;
                example_txt.Text = meaning.Definitions[offset].Example;
                DefinitionCount_txt.Text = (offset + 1) + "/" + (meaning.Definitions.Count).ToString();
            }
        }

        private void Next_definition_btn_Click(object sender, RoutedEventArgs e)
        {
            if (offset + 1 < meaning.Definitions.Count)
            {
                offset++;
                Definition_txt.Text = meaning.Definitions[offset].definition;
                example_txt.Text = meaning.Definitions[offset].Example;
                DefinitionCount_txt.Text = (offset + 1) + "/" + (meaning.Definitions.Count).ToString();
            }
        }

        private void Pos_txt_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_isFirstTime)
            {
                meaning = meanings.Find(meaning => meaning.PartOfSpeech == pos_txt.SelectedItem.ToString().Split(':')[1].Trim());
                offset = 0;
                if (meaning != null)
                {
                    Definition_txt.Text = meaning.Definitions[0].definition;
                    example_txt.Text = meaning.Definitions[0].Example;
                    DefinitionCount_txt.Text = (offset + 1) + "/" + (meaning.Definitions.Count).ToString();
                }
                else
                {
                    Definition_txt.Text = example_txt.Text = "";
                    DefinitionCount_txt.Text = (offset + 1) + "/" + (0 + 1).ToString();
                }
            }
            _isFirstTime = true;
        }

        #endregion

        #region speaker

        private void Us_speaker_btn_Click(object sender, RoutedEventArgs e)
        {
            AudioPlayer("us");
        }

        private void Uk_speaker_btn_Click(object sender, RoutedEventArgs e)
        {
            AudioPlayer("uk");
        }

        private void Ca_speaker_btn_Click(object sender, RoutedEventArgs e)
        {
            AudioPlayer("au");
        }

        void AudioPlayer(string language)
        {
            MediaPlayer mediaPlayer = new();

            try
            {
                var AudioDirectory = phonetics.Find(ph => ph.Language == language).Audio;
                if (File.Exists(AudioDirectory))
                {
                    mediaPlayer.Open(new Uri(AudioDirectory));

                    mediaPlayer.Play();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        #endregion

        #region folder

        private void Btn_us_folder_Click(object sender, RoutedEventArgs e)
        {
            var us = phonetics.Find(ph => ph.Language == "us");
            if (us != null)
            {
                if (us.Audio != null)
                {
                    OpenFolder(us.Audio);
                }
            }
        }

        private void Btn_uk_folder_Click(object sender, RoutedEventArgs e)
        {
            var uk = phonetics.Find(ph => ph.Language == "uk");
            if (uk != null)
            {
                if (uk.Audio != null)
                {
                    OpenFolder(uk.Audio);
                }
            }
        }

        private void Btn_ca_folder_Click(object sender, RoutedEventArgs e)
        {
            var au = phonetics.Find(ph => ph.Language == "au");
            if (au != null)
            {
                if (au.Audio != null)
                {
                    OpenFolder(au.Audio);
                }
            }
        }

        static void OpenFolder(string directory)
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = "explorer.exe",
                Arguments = "/select, \"" + directory + "\""
            };

            Process process = new()
            {
                StartInfo = startInfo
            };

            process.Start();
        }

        #endregion

        private void Starone_Click(object sender, RoutedEventArgs e)
        {
            Button? button = e.Source as Button;
            byte star;
            using UnitOfWork db = new();

            switch (button.Name)
            {
                case "starone":
                    {
                        starone.Content = "★";
                        startwo.Content = "☆";
                        starthree.Content = "☆";
                        starfour.Content = "☆";
                        starfive.Content = "☆";
                        star = 1;
                        db.WordsRepository.UpdateWordRate(star, this.WordID);
                    }
                    break;
                case "startwo":
                    {
                        starone.Content = "★";
                        startwo.Content = "★";
                        starthree.Content = "☆";
                        starfour.Content = "☆";
                        starfive.Content = "☆";
                        star = 2;
                        db.WordsRepository.UpdateWordRate(star, this.WordID);
                    }
                    break;
                case "starthree":
                    {
                        starone.Content = "★";
                        startwo.Content = "★";
                        starthree.Content = "★";
                        starfour.Content = "☆";
                        starfive.Content = "☆";
                        star = 3;
                        db.WordsRepository.UpdateWordRate(star, this.WordID);
                    }
                    break;
                case "starfour":
                    {
                        starone.Content = "★";
                        startwo.Content = "★";
                        starthree.Content = "★";
                        starfour.Content = "★";
                        starfive.Content = "☆";
                        star = 4;
                        db.WordsRepository.UpdateWordRate(star, this.WordID);
                    }
                    break;
                case "starfive":
                    {
                        starone.Content = "★";
                        startwo.Content = "★";
                        starthree.Content = "★";
                        starfour.Content = "★";
                        starfive.Content = "★";
                        star = 5;
                        db.WordsRepository.UpdateWordRate(star, this.WordID);
                    }
                    break;
                default:
                    {

                    }
                    break;
            }
        }

        private void ChangeLanguageSetting(LanguageSettings languageSettings)
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
                    gbx_Meanings.Header = "Meanings";
                    Phonetics_gbx.Header = "Phonetics";
                }
                else if (languageSettings.ButtonsLanguage == "Persian")
                {
                    gbx_Meanings.Header = "معانی";
                    Phonetics_gbx.Header = "تلفظ ها";
                }
            }

            void _ButtonsLanguage(string Language)
            {
                if (Language == "English")
                {
                    back_definition_btn.Content = "< Previous";
                    next_definition_btn.Content = "Next >";
                    btn_Delete.Content = "Delete";
                    btn_Edit.Content = "Edit";
                }
                else if (languageSettings.ButtonsLanguage == "Persian")
                {
                    back_definition_btn.Content = "< قبلی";
                    next_definition_btn.Content = "بعدی >";
                    btn_Delete.Content = "حذف";
                    btn_Edit.Content = "ویرایش";
                }
            }

            void _TextBlockLanguage(string Language)
            {
                if (Language == "English")
                {
                    txb_PartOfSpeech.Text = "PartOfSpeech:";
                }
                else if (languageSettings.TextBlockLanguage == "Persian")
                {
                    txb_PartOfSpeech.Text = "دستور زبان:";
                }
            }

            void _MessageBoxLanguage(string Language)
            {
                if (Language == "English")
                {
                    mbo_MessageBoxLanguage = MessageBoxOptions.None;

                    mbl_DeleteWord.Caption = "";
                    mbl_DeleteWord.Text = "Are you sure you want to delete the ";
                }
                else if (languageSettings.MessageBoxLanguage == "Persian")
                {
                    mbo_MessageBoxLanguage = MessageBoxOptions.RtlReading;

                    mbl_DeleteWord.Caption = "";
                    mbl_DeleteWord.Text = "آیا از حذف کلمه اطمینان دارید؟";
                }
            }
        }

        private void Btn_Translate_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("translate will be open...");
            // show translate page
        }

        private void Btn_Edit_Click(object sender, RoutedEventArgs e)
        {
            AddWord addWord = new(true, WordID);
            addWord.Show();
            this.Close();
        }

        private void Btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(mbl_DeleteWord.Text + word.English, mbl_DeleteWord.Caption, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.None, mbo_MessageBoxLanguage) == MessageBoxResult.Yes)
            {
                using (UnitOfWork db = new())
                {
                    db.WordsRepository.DeleteWord(WordID);
                }
                this.Close();
            }
        }

        private void Hyperlink_Source_url(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            string url = e.Uri.AbsoluteUri;
            System.Diagnostics.Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
