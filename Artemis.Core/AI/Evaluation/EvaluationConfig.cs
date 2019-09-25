using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.AI.Evaluation
{
    public class EvaluationConfig
    {
        private int[] piecesValue = { 500, 290, 300, 100, 900, 0 };
        private int[] mobility = { 1, 5, 3, 0, 2 };
        private int pawnCentralControl = 20;
        private int pieceCentralControl = 5;
        private int pawnSupport = 2;
        private int kingCastled = 40;
        private int kingMiddlePenalty = -60;
        private int kingPawnProtectors = 8;
        private int kingAttack = 10;

        public int GetPieceValue(PieceType pieceType)
        {
            return piecesValue[(int)pieceType];
        }

        public int GetMobilityScore(PieceType pieceType)
        {
            return mobility[(int)pieceType];
        }

        public int GetPawnCentralControlScore()
        {
            return pawnCentralControl;
        }

        public int GetPawnSupportScore()
        {
            return pawnSupport;
        }

        public int GetPieceCentralControlScore()
        {
            return pieceCentralControl;
        }

        public int GetKingCastledScore()
        {
            return kingCastled;
        }

        public int GetKingMiddlePenalty()
        {
            return kingMiddlePenalty;
        }

        public int GetKingPawnProtectorsScore()
        {
            return kingPawnProtectors;
        }

        public int GetKingAttackScore()
        {
            return kingAttack;
        }
    }
}
