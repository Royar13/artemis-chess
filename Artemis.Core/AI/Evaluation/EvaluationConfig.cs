using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.AI.Evaluation
{
    public class EvaluationConfig
    {
        private int contemptFactor = 50;
        private int[] piecesValue = { 500, 290, 320, 100, 900, 0 };
        private Modifier[] mobility = { new Modifier(0, 1, 0), new Modifier(2), new Modifier(5), new Modifier(0), new Modifier(0, 1, 1) };
        private int pawnCentralControl = 25;
        private int space = 2;
        private int pieceCentralControl = 3;
        private int pawnSupport = 2;
        private int[] kingFile = { 15, 20, 15, -15, -10, 5, 25, 15 };
        private int rooksConnected = 20;
        private int[] pieceAttack = { 10, 6, 6, 0, 9 };
        private int[] pawnStormRank = { 0, 0, 2, 4, 7, 12, 16 };
        private int kingPawnMovedPenalty = -3;
        private int kingOpenFilePenalty = -25;
        private double directAttackModifier = 1.3;
        private int[] pieceDefense = { 3, 4, 5, 1, 1 };
        private int doubledPawnsPenalty = -17;
        private int isolatedPawnPenalty = -17;
        private int isolatedPawnOpenFilePenalty = -25;
        private int[] passedPawnRank = { 0, 5, 10, 25, 30, 35, 40 };
        private double passedPawnDefenderModifier = 1.7;
        private int[] rookRank = { 0, 0, 0, 0, 0, 10, 20, 10 };
        private int rookOpenFile = 15;
        private double knightPawnProtectorModifier = 1.2;
        private double knightOutpostModifier = 1.8;
        private int[] knightSquare = new int[64]
        {
            0, 2, 4, 2, 2, 4, 2, 0,
            1, 2, 2, 9, 9, 2, 2, 1,
            3, 6, 10, 10, 10, 10, 6, 3,
            4, 9, 9, 14, 14, 9, 9, 4,
            12, 12, 20, 16, 16, 20, 12, 12,
            14, 22, 26, 26, 26, 26, 22, 14,
            8, 13, 17, 18, 18, 17, 13, 8,
            2, 6, 6, 7, 7, 6, 6, 2
        };
        private int[] endgameKingSquare ={  0, 6, 12, 18, 18, 12, 6, 0,
                                            10, 16, 22, 28, 28, 22, 16, 10,
                                            20, 26, 32, 38, 38, 32, 26, 20,
                                            30, 36, 42, 48, 48, 42, 36, 30,
                                            35, 41, 47, 53, 53, 47, 41, 35,
                                            30, 36, 42, 48, 48, 42, 36, 30,
                                            25, 31, 37, 43, 43, 37, 31, 25,
                                            20, 26, 32, 38, 38, 32, 26, 20 };
        private int endgameEnemyKingCenterDistance = 10;
        private int endgameKingsDistance = 5;

        public int GetContemptFactor()
        {
            return contemptFactor;
        }

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

        public int GetSpaceScore()
        {
            return space;
        }

        public int GetPawnSupportScore()
        {
            return pawnSupport;
        }

        public int GetPieceCentralControlScore()
        {
            return pieceCentralControl;
        }

        public int GetKingFileScore(int pl, int file, bool rooksConnected, IrrevState irrevState)
        {
            int score = kingFile[file];
            if (score < 0)
            {
                if (!irrevState.CastlingAllowed[pl, 0] && !irrevState.CastlingAllowed[pl, 1])
                {
                    score = (int)(score * 1.5);
                }
            }
            else if (!rooksConnected)
            {
                score = (int)(score * 0.2);
            }
            return score;
        }

        public int GetRooksConnectedScore()
        {
            return rooksConnected;
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
            return (int)(pieceDefense[(int)pieceType] * directAttackModifier);
        }

        public int GetExtendedPieceDefenseScore(PieceType pieceType)
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
            int score = pl == 0 ? pawnStormRank[rank] : pawnStormRank[7 - rank];
            return score;
        }

        public int GetRookRankScore(int pl, int rank, GameStage stage)
        {
            int score = 0;
            if (stage != GameStage.Opening)
            {
                score = pl == 0 ? rookRank[rank] : rookRank[7 - rank];
            }
            return score;
        }

        public int GetRookOpenFileScore()
        {
            return rookOpenFile;
        }

        public int GetKingPawnMovedPenalty(int moves, double opKingAttackModifier)
        {
            return moves == 0 ? 0 : (int)(kingPawnMovedPenalty * Math.Pow(1.7, moves) * opKingAttackModifier);
        }

        public int GetKingOpenFilePenalty(double opKingAttackModifier)
        {
            return (int)(kingOpenFilePenalty * opKingAttackModifier);
        }

        public int GetKnightSquareScore(int pl, int sq, int pawnProtectors, bool outpost)
        {
            if (pl == 1)
            {
                sq = BitboardUtils.MirrorRank(sq);
            }
            double score = knightSquare[sq];
            double outpostModifier = outpost ? knightOutpostModifier : 1;
            score *= Math.Pow(knightPawnProtectorModifier, pawnProtectors) * outpostModifier;
            return (int)score;
        }

        public int GetEndgameKingSquareScore(int pl, int kingSq)
        {
            if (pl == 1)
            {
                kingSq = BitboardUtils.MirrorRank(kingSq);
            }
            return endgameKingSquare[kingSq];
        }

        public int GetEndgameEnemyKingCenterDistanceScore(int distance)
        {
            return endgameEnemyKingCenterDistance * distance;
        }

        public int GetEndgameKingsDistanceScore(int distance)
        {
            int score = (14 - distance) * endgameKingsDistance;
            return score;
        }

        public int GetPassedPawnScore(int pl, int rank, int pawnDefenders)
        {
            if (pl == 1)
            {
                rank = 7 - rank;
            }
            int score = (int)(passedPawnRank[rank] * Math.Pow(passedPawnDefenderModifier, pawnDefenders));
            return score;
        }
    }
}
