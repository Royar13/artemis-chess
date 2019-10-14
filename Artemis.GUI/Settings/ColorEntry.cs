using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artemis.GUI.Settings
{
    public class ColorEntry
    {
        public int Value { get; }
        public string Display { get; }

        public ColorEntry(int value, string display)
        {
            Value = value;
            Display = display;
        }
    }
}
