using Artemis.GUI.Settings;
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

namespace Artemis.GUI.Settings
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private UISettings settings;
        private SettingsDataAccess settingsData = new SettingsDataAccess();
        public event EventHandler<SettingsUpdatedEventArgs> SettingsUpdated;

        public SettingsWindow(UISettings settings)
        {
            InitializeComponent();

            this.settings = settings;
            settings.ChangesMade = false;
            DataContext = settings;
            Closing += SettingsWindow_Closing;
        }

        private void SettingsWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (settings.ChangesMade)
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to exit the settings without saving?", "Exit", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void Save()
        {
            settingsData.Save(settings);
            settings.ChangesMade = false;
            SettingsUpdated?.Invoke(this, new SettingsUpdatedEventArgs(settings));
            Close();
        }
    }
}
