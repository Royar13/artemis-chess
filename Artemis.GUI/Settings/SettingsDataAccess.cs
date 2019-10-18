using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artemis.GUI.Settings
{
    class SettingsDataAccess
    {
        public void Load(UISettings settings)
        {
            settings.ConstantDepth = Properties.Settings.Default.ConstantDepth;
            settings.Depth = Properties.Settings.Default.Depth;
            settings.TimeLimit = Properties.Settings.Default.TimeLimit;
            settings.Multithreading = Properties.Settings.Default.Multithreading;
            settings.UseOpeningBook = Properties.Settings.Default.UseOpeningBook;
            settings.WhitePlayerType = (InputSource)Properties.Settings.Default.WhitePlayerType;
            settings.BlackPlayerType = (InputSource)Properties.Settings.Default.BlackPlayerType;
            settings.BottomColor = Properties.Settings.Default.BottomColor;
        }

        public void Save(UISettings settings)
        {
            Properties.Settings.Default.ConstantDepth = settings.ConstantDepth;
            Properties.Settings.Default.Depth = settings.Depth;
            Properties.Settings.Default.TimeLimit = settings.TimeLimit;
            Properties.Settings.Default.Multithreading = settings.Multithreading;
            Properties.Settings.Default.UseOpeningBook = settings.UseOpeningBook;
            Properties.Settings.Default.WhitePlayerType = (int)settings.WhitePlayerType;
            Properties.Settings.Default.BlackPlayerType = (int)settings.BlackPlayerType;
            Properties.Settings.Default.BottomColor = settings.BottomColor;
            Properties.Settings.Default.Save();
        }
    }
}
