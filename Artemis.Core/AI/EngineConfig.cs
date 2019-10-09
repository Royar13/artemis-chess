using Artemis.Core.AI.Evaluation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.AI
{
    public class EngineConfig : IEngineConfig
    {
        public bool ConstantDepth { get; set; } = false;
        public int Depth { get; set; } = 7;
        public int TimeLimit { get; set; } = 4500;
        public int MinimalDepthMultithreading { get; set; } = 3;
        public int NullMoveDepthReduction { get; set; } = 2;
        public bool Multithreading { get; set; } = true;
    }
}
