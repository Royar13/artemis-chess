using Artemis.Core;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Artemis.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GameManager gm;

        public MainWindow()
        {
            InitializeComponent();
            MovesHistory movesHistory = new MovesHistory();
            gm = new GameManager(Board, movesHistory);
            gm.NewGame();
            MainContainer.DataContext = gm;
            MovesListTB.DataContext = movesHistory;
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            gm.NewGame();
        }

        private void MovesListTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.Focus();
            tb.CaretIndex = tb.Text.Length;
            tb.ScrollToEnd();
        }

        private void FenTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                TextBox tb = (TextBox)sender;
                bool updated = gm.LoadFen(tb.Text);
                if (!updated)
                {
                    tb.Text = gm.FEN;
                }
            }
        }

        private void SelectivelyHandleMouseButton(object sender, MouseButtonEventArgs e)
        {
            var textbox = (sender as TextBox);
            if (textbox != null)
            {
                e.Handled = true;
                textbox.Focus();
                textbox.SelectAll();
            }
        }

        private void SelectAllText(object sender, RoutedEventArgs e)
        {
            var textBox = e.OriginalSource as TextBox;
            if (textBox != null)
                textBox.SelectAll();
        }

        private void FenTB_LostFocus(object sender, RoutedEventArgs e)
        {
            //trigger property update, simply by reassigning the current value
            gm.FEN = gm.FEN;
        }

        private void UndoMove_Click(object sender, RoutedEventArgs e)
        {
            gm.Undo();
        }
    }
}
