using Artemis.Core.AI.Search.Heuristics;
using Artemis.Core.Moves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Artemis.Core.AI.Evaluation
{
    public class MoveEvaluator
    {
        Move pvMove;
        Move hashMove;
        Move[] killerMoves;

        public MoveEvaluator(Move pvMove, Move hashMove, Move[] killerMoves)
        {
            this.pvMove = pvMove;
            this.hashMove = hashMove;
            this.killerMoves = killerMoves;
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
            else if (killerMoves.Any(m => m != null && m.Equals(move)))
            {
                return 600;
            }
            return score;
        }
    }
}
