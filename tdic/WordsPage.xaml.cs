using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using tdic.DataContext;
using WordsDBModelView;

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
        }


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

        }    }
}
