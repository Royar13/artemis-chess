using Artemis.Core.Moves;
using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.AI.Evaluation
{
    public class ScoredMove
    {
        public Move Move { get; }
        public int Score { get; }

        public ScoredMove(Move move, int score)
        {
            Move = move;
            Score = score;
        }
    }
}
