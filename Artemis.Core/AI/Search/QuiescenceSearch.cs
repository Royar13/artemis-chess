using Artemis.Core.AI.Evaluation;
using Artemis.Core.Moves;
using Artemis.Core.Moves.Generator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Artemis.Core.AI.Search
{
    public class QuiescenceSearch
    {
        GameState gameState;
        PositionEvaluator evaluator;
        MoveEvaluator moveEvaluator;
        const int DELTA_PRUNING_MARGIN = 200;

        public QuiescenceSearch(GameState gameState, PositionEvaluator evaluator, MoveEvaluator moveEvaluator)
        {
            this.gameState = gameState;
            this.evaluator = evaluator;
            this.moveEvaluator = moveEvaluator;
        }

        public int Search(int alpha, int beta)
        {
            int standPat = evaluator.Evaluate(0);
            int originalAlpha = alpha;
            if (standPat >= beta)
            {
                return standPat;
            }
            else if (standPat > alpha)
            {
                alpha = standPat;
            }

            List<Move> moves = gameState.GetMoves(GenerationMode.Quiescence);
            //Delta pruning and move ordering
            int compareToScore = originalAlpha - DELTA_PRUNING_MARGIN;
            moves = moves.Select(m => moveEvaluator.EvaluateCapture(m)).Where(m => standPat >= compareToScore - m.Score)
                .OrderByDescending(m => m.Score).Select(m => m.Move).ToList();

            bool cutoff = false;
            for (int i = 0; i < moves.Count && !cutoff; i++)
            {
                Move move = moves[i];
                gameState.MakeMove(move);
                if (move.IsLegal())
                {
                    int score = -Search(-beta, -alpha);
                    if (score >= beta)
                    {
                        //alpha-beta cutoff
                        alpha = beta;
                        cutoff = true;
                    }
                    else if (score > alpha)
                    {
                        alpha = score;
                    }
                }
                gameState.UnmakeMove(move);
            }

            return alpha;
        }
    }
}
