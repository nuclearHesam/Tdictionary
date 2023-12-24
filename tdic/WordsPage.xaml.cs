using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using tdic.DataContext;
using WordsDBModelView;
using WordsListedModelView;

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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using UnitOfWork db = new();

            Words = db.WordsRepository.ReadWords();

            Words_lbx.ItemsSource = Words;

            lbl_Count.Content = Words.Count;
        }

        #region

        private void txt_Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            var filteredWords = Words.FindAll(w=> w.English.Contains(txt_Search.Text.ToLower().Trim())); 

            Words_lbx.ItemsSource = filteredWords;
            lbl_Count.Content= filteredWords.Count;
        }

        private void txt_Pos_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

            void SetWordByFilter(string filter)
            {
                using (UnitOfWork db = new())
                {
                    var words = db.WordsRepository.ReadWordByFilter(filter);

                    lbl_Count.Content = words.Count;
                    Words_lbx.ItemsSource = words;
                }
            }
        }

        #endregion


        private void btn_Reset_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {

        }

        private void Stars_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_Delete_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_Edit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_SortUP_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_SortDown_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
