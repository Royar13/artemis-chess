using Artemis.Core.AI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artemis.GUI.Settings
{
    public class UISettings : EngineConfig, ISettings, INotifyPropertyChanged
    {
        public bool ChangesMade = false;
        private bool constantDepth = false;
        public override bool ConstantDepth
        {
            get { return constantDepth; }
            set
            {
                constantDepth = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConstantDepth"));
            }
        }
        private int depth = 7;
        public override int Depth
        {
            get { return depth; }
            set
            {
                depth = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Depth"));
            }
        }
        private int timeLimit = 4500;
        public override int TimeLimit
        {
            get { return timeLimit; }
            set
            {
                timeLimit = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TimeLimit"));
            }
        }
        private List<TimeEntry> timeLimitEntries = new List<TimeEntry>
        {
            new TimeEntry(1000),new TimeEntry(2000), new TimeEntry(3000), new TimeEntry(4500), new TimeEntry(10000), new TimeEntry(30000), new TimeEntry(60000)
        };
        public List<TimeEntry> TimeLimitEntries
        {
            get { return timeLimitEntries; }
            set
            {
                timeLimitEntries = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TimeLimitEntries"));
            }
        }
        private bool multithreading = true;
        public override bool Multithreading
        {
            get { return multithreading; }
            set
            {
                multithreading = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Multithreading"));
            }
        }
        public InputSource[] PlayerType { get; set; } = { InputSource.Player, InputSource.Engine };
        public InputSource WhitePlayerType
        {
            get { return PlayerType[0]; }
            set
            {
                PlayerType[0] = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("WhitePlayerType"));
            }
        }
        public InputSource BlackPlayerType
        {
            get { return PlayerType[1]; }
            set
            {
                PlayerType[1] = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("BlackPlayerType"));
            }
        }
        private PlayerTypeEntry[] playerTypeEntries = new PlayerTypeEntry[2];
        public PlayerTypeEntry[] PlayerTypeEntries
        {
            get { return playerTypeEntries; }
            set
            {
                playerTypeEntries = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PlayerTypeEntries"));
            }
        }
        private int bottomColor = 0;
        public int BottomColor
        {
            get { return bottomColor; }
            set
            {
                bottomColor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("BottomColor"));
            }
        }
        private ColorEntry[] colorEntries = { new ColorEntry(0, "White"), new ColorEntry(1, "Black") };
        public ColorEntry[] ColorEntries
        {
            get { return colorEntries; }
            set
            {
                colorEntries = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ColorEntries"));
            }
        }

        public UISettings()
        {
            PlayerTypeEntries[0] = new PlayerTypeEntry(InputSource.Player, "Player");
            PlayerTypeEntries[1] = new PlayerTypeEntry(InputSource.Engine, "Engine");

            PropertyChanged += Settings_PropertyChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ChangesMade = true;
        }
    }
}
