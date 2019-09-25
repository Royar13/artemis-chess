using Artemis.Core.AI.Evaluation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.AI
{
    public class EngineConfig
    {
        public int InitialAlpha = -PositionEvaluator.CHECKMATE_SCORE * 2;
        public int InitialBeta = PositionEvaluator.CHECKMATE_SCORE * 2;
        public bool ConstantDepth = true;
        public int Depth = 5;
        public int MaxDepth = 10;
    }
}
