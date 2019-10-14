using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artemis.GUI.Settings
{
    public class SettingsUpdatedEventArgs : EventArgs
    {
        public readonly UISettings Settings;

        public SettingsUpdatedEventArgs(UISettings settings)
        {
            Settings = settings;
        }
    }
}
