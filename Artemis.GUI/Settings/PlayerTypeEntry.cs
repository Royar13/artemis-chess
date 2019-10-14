using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artemis.GUI.Settings
{
    public class PlayerTypeEntry
    {
        public InputSource Value { get; }
        public string Display { get; }

        public PlayerTypeEntry(InputSource value, string display)
        {
            Value = value;
            Display = display;
        }
    }
}
