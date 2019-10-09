﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.AI.Evaluation
{
    public class EvaluationConfig
    {
        private int[] piecesValue = { 500, 290, 300, 100, 900, 0 };
        private Modifier[] mobility = { new Modifier(1), new Modifier(5), new Modifier(3), new Modifier(0), new Modifier(0, 1, 1) };
        private int pawnCentralControl = 20;
        private int pieceCentralControl = 5;
        private int pawnSupport = 2;
        private int[] kingCastled = { 30, 40 };
        private int kingMiddlePenalty = -30;
        private int kingPawnProtectors = 8;
        private int[] pieceAttack = { 10, 6, 6, 0, 12 };
        private int pawnStorm = 4;
        private int pawnStormPerRank = 2;
        private int kingPawnMovedPenalty = -6;
        private int kingOpenFilePenalty = -20;
        private double directAttackModifier = 1.3;
        private int[] pieceDefense = { 3, 4, 5, 1, 1 };
        private int doubledPawnsPenalty = -30;
        private int isolatedPawnPenalty = -30;
        private int isolatedPawnOpenFilePenalty = -45;

        public int GetPieceValue(PieceType pieceType)
        {
            return piecesValue[(int)pieceType];
        }

        public int GetMobilityScore(GameStage stage, PieceType pieceType)
        {
            return mobility[(int)pieceType].Get(stage);
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

        public int GetKingCastledScore(int side)
        {
            return kingCastled[side];
        }

        public int GetKingMiddlePenalty()
        {
            return kingMiddlePenalty;
        }

        public int GetKingPawnProtectorsScore()
        {
            return kingPawnProtectors;
        }

        public int GetPieceAttackScore(PieceType pieceType)
        {
            return (int)(pieceAttack[(int)pieceType] * directAttackModifier);
        }

        public int GetExtendedPieceAttackScore(PieceType pieceType)
        {
            return pieceAttack[(int)pieceType];
        }

        public int GetPieceDefenseScore(PieceType pieceType)
        {
            return pieceDefense[(int)pieceType];
        }

        public int GetDoubledPawnsPenalty()
        {
            return doubledPawnsPenalty;
        }

        public int GetIsolatedPawnPenalty()
        {
            return isolatedPawnPenalty;
        }

        public int GetIsolatedPawnOpenFilePenalty()
        {
            return isolatedPawnOpenFilePenalty;
        }

        public int GetPawnStormScore(int pl, int rank)
        {
            int score = 0;
            if (pl == 0 && rank >= 3)
            {
                score = pawnStorm + pawnStormPerRank * (rank - 3);
            }
            else if (pl == 1 && rank <= 4)
            {
                score = pawnStorm + pawnStormPerRank * (4 - rank);
            }
            return score;
        }

        public int GetKingPawnMovedPenalty(int pl, int rank)
        {
            int initialRank = pl == 0 ? 1 : 6;
            int moved = Math.Min(3, Math.Abs(rank - initialRank));
            return kingPawnMovedPenalty * moved;
        }

        public int GetKingOpenFilePenalty()
        {
            return kingOpenFilePenalty;
        }
    }
}
