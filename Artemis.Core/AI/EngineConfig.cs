using Artemis.Core.AI.Evaluation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.AI
{
    public class EngineConfig : IEngineConfig
    {
        public bool ConstantDepth { get; set; } = false;
        public int Depth { get; set; } = 5;
        public int TimeLimit { get; set; } = 4500;
    }
}
