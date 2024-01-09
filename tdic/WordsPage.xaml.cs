using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using tdic.DataContext;
using tdic.SettingJson;
using Tdictionary.Models;
using WordsDBModelView;
using static Tdictionary.Models.Setting;

namespace tdic
{
    /// <summary>
    /// Interaction logic for WordsPage.xaml
    /// </summary>
    public partial class WordsPage : Window
    {
        public WordsPage()
        {
            InitializeComponent();
        }
        List<Words> Words = new();
        Words Word;

        #region MessageBoxLanguage

        MessageBoxOptions mbo_MessageBoxLanguage = new();

        readonly MessageBoxLanguage mbl_DeleteWord = new();

        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BindListBox();

            var Settingslang = Serializer.ReadSettingJson();
            ChangeLanguageSetting(Settingslang.LanguageSettings);
        }

        #region Search Filter Sort

        private void txt_Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            var filteredWords = Words.FindAll(w => w.English.Contains(txt_Search.Text.ToLower().Trim()));

            Words_lbx.ItemsSource = filteredWords;
            lbl_Count.Content = filteredWords.Count;
        }

        private void txt_Pos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string boxItem = txt_Pos.SelectedItem.ToString().Split(' ')[1];
                switch (boxItem)
                {
                    case "pronoun":
                        {
                            SetWordByFilter("pronoun");
                        }
                        break;
                    case "verb":
                        {
                            SetWordByFilter("verb");
                        }
                        break;
                    case "noun":
                        {
                            SetWordByFilter("noun");
                        }
                        break;
                    case "adjective":
                        {
                            SetWordByFilter("adjective");
                        }
                        break;
                    case "adverb":
                        {
                            SetWordByFilter("adverb");
                        }
                        break;
                    case "preposition":
                        {
                            SetWordByFilter("preposition");
                        }
                        break;
                    case "conjunction":
                        {
                            SetWordByFilter("conjunction");
                        }
                        break;
                    case "interjection":
                        {
                            SetWordByFilter("interjection");
                        }
                        break;
                    default:
                        {

                        }
                        break;
                }
            }
            catch
            {

            }

            void SetWordByFilter(string filter)
            {
                using (UnitOfWork db = new())
                {
                    Words = db.WordsRepository.ReadWordByFilter(filter);

                    lbl_Count.Content = Words.Count;
                    Words_lbx.ItemsSource = Words;
                }
            }
        }

        private void btn_SortUP_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(Words_lbx.Items);
            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription("English", ListSortDirection.Ascending));

            btn_SortUP.Background = new SolidColorBrush(Colors.WhiteSmoke);
            btn_SortDown.Background = new SolidColorBrush(Colors.White);
        }

        private void btn_SortDown_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(Words_lbx.Items);
            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription("English", ListSortDirection.Descending));

            btn_SortUP.Background = new SolidColorBrush(Colors.White);
            btn_SortDown.Background = new SolidColorBrush(Colors.WhiteSmoke);
        }

        private void btn_Reset_Click(object sender, RoutedEventArgs e)
        {
            grpx_Word.IsEnabled = false;

            ICollectionView view = CollectionViewSource.GetDefaultView(Words_lbx.Items);
            view.SortDescriptions.Clear();
            btn_SortUP.Background = new SolidColorBrush(Colors.White);
            btn_SortDown.Background = new SolidColorBrush(Colors.White);
            txt_Pos.SelectedItem = null;
            txt_Search.Text = "";
            txb_Word.Text = "";
            txb_Translate.Text = "";
            txb_Meanings.Text = "";
            txb_Definitions.Text = "";
            txb_Phonetics.Text = "";
            txb_Images.Text = "";

            starone.Content = startwo.Content = starthree.Content = starfive.Content = starfour.Content = "☆";

            BindListBox();
        }

        #endregion

        #region Word

        private void Words_lbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Words_lbx.SelectedItem != null)
            {
                if (Words_lbx.SelectedItem is Words SelectedWord)
                {
                    // read word
                    using UnitOfWork db = new UnitOfWork();

                    int[] counts = db.WordsRepository.ReadCounts(SelectedWord.WordID);

                    grpx_Word.IsEnabled = true;
                    Word = SelectedWord;

                    // set informations
                    txb_Word.Text = SelectedWord.English;
                    txb_Translate.Text = SelectedWord.Translation;
                    txb_Meanings.Text = counts[1].ToString();
                    txb_Definitions.Text = counts[2].ToString();
                    txb_Phonetics.Text = counts[0].ToString();
                    //txb_Images.Text = counts[3];

                    // set stars
                    switch (SelectedWord.Rate)
                    {
                        case "1":
                            {
                                starone.Content = "★";
                                startwo.Content = starthree.Content = starfive.Content = starfour.Content = "☆";
                            }
                            break;
                        case "2":
                            {
                                startwo.Content = starone.Content = "★";
                                starthree.Content = starfive.Content = starfour.Content = "☆";
                            }
                            break;
                        case "3":
                            {
                                starthree.Content = startwo.Content = starone.Content = "★";
                                starfive.Content = starfour.Content = "☆";
                            }
                            break;
                        case "4":
                            {
                                starfour.Content = starthree.Content = startwo.Content = starone.Content = "★";
                                starfive.Content = "☆";
                            }
                            break;
                        case "5":
                            {
                                starthree.Content = starfour.Content = starfive.Content = startwo.Content =  starfive.Content = starone.Content = "★";
                            }
                            break;
                        default:
                            {
                                starone.Content = startwo.Content = starthree.Content = starfive.Content = starfour.Content = "☆";
                            }
                            break;
                    }

                    // Set SourceUrl
                    if(SelectedWord.SourceUrl != null)
                    {
                        txb_Source_Url.IsEnabled = true;

                        url_Hyperlink.NavigateUri = new Uri(SelectedWord.SourceUrl);
                    }
                    else
                    {
                        txb_Source_Url.IsEnabled = false;
                    }

                }
            }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            string url = e.Uri.AbsoluteUri;
            System.Diagnostics.Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            e.Handled = true;
        }

        private void Stars_Click(object sender, RoutedEventArgs e)
        {
            Button? button = e.Source as Button;
            Words word = Words_lbx.SelectedItem as Words;

            using UnitOfWork db = new();

            switch (button.Name)
            {
                case "starone":
                    {
                        starone.Content = "★";
                        startwo.Content = starthree.Content = starfive.Content = starfour.Content = "☆";
                        word.Rate = "1";
                        db.WordsRepository.UpdateWordRate(1, Word.WordID);
                    }
                    break;
                case "startwo":
                    {
                        startwo.Content = starone.Content = "★";
                        starthree.Content = starfive.Content = starfour.Content = "☆";
                        word.Rate = "2";
                        db.WordsRepository.UpdateWordRate(2, Word.WordID);
                    }
                    break;
                case "starthree":
                    {
                        starthree.Content = startwo.Content = starone.Content = "★";
                        starfive.Content = starfour.Content = "☆";
                        word.Rate = "3";
                        db.WordsRepository.UpdateWordRate(3, Word.WordID);
                    }
                    break;
                case "starfour":
                    {
                        starfour.Content = starthree.Content = startwo.Content = starone.Content = "★";
                        starfive.Content = "☆";
                        word.Rate = "4";
                        db.WordsRepository.UpdateWordRate(4, Word.WordID);
                    }
                    break;
                case "starfive":
                    {
                        starthree.Content = starfour.Content = starfive.Content = startwo.Content = starfive.Content = starone.Content = "★";
                        word.Rate = "5";
                        db.WordsRepository.UpdateWordRate(5, Word.WordID);
                    }
                    break;
                default:
                    {

                    }
                    break;
            }
        }

        private void btn_Open_Click(object sender, RoutedEventArgs e)
        {
            // open word
            WordPage wordPage = new(Word.WordID);
            wordPage.ShowDialog();

            // refresh list
            BindListBox();
        }

        private void btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(mbl_DeleteWord.Text + Word.English, mbl_DeleteWord.Caption, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.None, mbo_MessageBoxLanguage) == MessageBoxResult.Yes)
            {
                using (UnitOfWork db = new())
                {
                    db.WordsRepository.DeleteWord(Word.WordID);
                    BindListBox();
                }
            }
        }

        private void btn_Edit_Click(object sender, RoutedEventArgs e)
        {
            AddWord word = new AddWord(true,Word.WordID);
            word.ShowDialog();
        }

        #endregion

        private void BindListBox()
        {
            UnitOfWork db = new();

            Words = db.WordsRepository.ReadWords();

            Words_lbx.ItemsSource = Words;

            lbl_Count.Content = Words.Count;
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

                }
                else if (languageSettings.ButtonsLanguage == "Persian")
                {
                    grpx_Search.Header = "جستوجو";
                    grpx_Word.Header = "کلمه";
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
                    
                }
                else if (languageSettings.TextBlockLanguage == "Persian")
                {
                    lbl_Meanings.Content = "معانی:";
                    lbl_Definitions.Content = "توضیحات:";
                    lbl_Phonetics.Content = "تلفظ ها:";
                    lbl_Images.Content = "تصاویر:";
                    run_UrlText.Text = "لینک منبع:";
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

    }
}
