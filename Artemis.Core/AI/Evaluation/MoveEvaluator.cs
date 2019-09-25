using Artemis.Core.Moves;
using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.AI.Evaluation
{
    public class MoveEvaluator
    {
        Move pvMove;
        Move hashMove;

        public MoveEvaluator(Move pvMove, Move hashMove)
        {
            this.pvMove = pvMove;
            this.hashMove = hashMove;
        }

        public int CalculateScore(Move move)
        {
            int score = 0;
            if (pvMove != null && move.Equals(pvMove))
            {
                return 1000;
            }
            else if (hashMove != null && move.Equals(hashMove))
            {
                return 800;
            }
            return score;
        }
    }
}
