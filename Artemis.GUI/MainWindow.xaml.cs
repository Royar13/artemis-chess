﻿using Artemis.Core;
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
            MovesListTB.DataContext = movesHistory;
        }

        private void NewGameBtn_Click(object sender, RoutedEventArgs e)
        {
            gm.NewGame();
        }
    }
}
