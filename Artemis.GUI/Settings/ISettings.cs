using Artemis.Core.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artemis.GUI.Settings
{
    public interface ISettings : IEngineConfig
    {
        InputSource[] PlayerType { get; }
        int BottomColor { get; }
    }
}
