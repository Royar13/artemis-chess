using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artemis.GUI.Settings
{
    public class TimeEntry
    {
        public string Display
        {
            get
            {
                double timeSec = (double)Value / 1000;
                return timeSec.ToString();
            }
        }
        public int Value { get; }

        public TimeEntry(int timeMs)
        {
            Value = timeMs;
        }
    }
}
