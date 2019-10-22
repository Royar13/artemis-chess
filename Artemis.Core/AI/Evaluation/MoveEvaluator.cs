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
        EvaluationConfig evalConfig;

        public MoveEvaluator(EvaluationConfig evalConfig)
        {
            this.evalConfig = evalConfig;
        }

        public ScoredMove EvaluateMove(Move move, Move pvMove, Move hashMove, Move[] killerMoves)
        {
            if (pvMove != null && move.Equals(pvMove))
            {
                return new ScoredMove(move, 2100);
            }
            else if (hashMove != null && move.Equals(hashMove))
            {
                return new ScoredMove(move, 2000);
            }
            else if (move is PromotionMove && ((PromotionMove)move).PromotionType == PieceType.Queen)
            {
                return new ScoredMove(move, 1800);
            }
            else if (move.IsCapture())
            {
                int captureScore = GetMaterialGain(move);
                if (captureScore >= 0)
                {
                    return new ScoredMove(move, 1000 + captureScore);
                }
            }

            if (killerMoves.Any(m => m != null && m.Equals(move)))
            {
                return new ScoredMove(move, 900);
            }
            return new ScoredMove(move, 0);
        }

        private int GetMaterialGain(Move move)
        {
            PieceType capturedPiece = move.GetCapturedPieceType();
            return evalConfig.GetPieceValue(capturedPiece) - evalConfig.GetPieceValue(move.MovedPieceType);
        }

        public ScoredMove EvaluateCapture(Move move)
        {
            int materialGain = GetMaterialGain(move);
            return new ScoredMove(move, materialGain);
        }
    }
}
