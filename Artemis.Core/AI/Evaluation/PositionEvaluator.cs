using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.AI.Evaluation
{
    public class PositionEvaluator
    {
        ArtemisEngine engine;
        GameState gameState;
        EvaluationConfig config;
        public const int CHECKMATE_SCORE = 1000000;
        private const ulong SAFE_KINGSIDE_SQUARES = 0xC0;
        private const ulong SAFE_QUEENSIDE_SQUARES = 0x7;
        /// <summary>
        /// A mask of the 16 squares (quarter of the board) around the king, indexed by the king's file
        /// </summary>
        private readonly ulong[] kingQuarter = { 0x0F0F0F0F00000000, 0x0F0F0F0F00000000, 0x0F0F0F0F00000000,
            0x3C3C3C3C00000000, 0x3C3C3C3C00000000,
            0xF0F0F0F000000000, 0xF0F0F0F000000000, 0xF0F0F0F000000000 };

        public PositionEvaluator(ArtemisEngine engine, GameState gameState, EvaluationConfig config)
        {
            this.engine = engine;
            this.gameState = gameState;
            this.config = config;
        }

        public int Evaluate(int depth)
        {
            GameResult result = gameState.GetResult();
            if (result != GameResult.Ongoing)
            {
                if (result == GameResult.Checkmate)
                {
                    return -CHECKMATE_SCORE - depth;
                }
                else
                {
                    return 0;
                }
            }

            int score = 0;
            ulong[,] pieceAttacks = new ulong[2, 5];
            ulong[] kingQuarterAttacks = new ulong[2];
            int[] kingQuarterAttacksAmount = new int[2];
            for (int pl = 0; pl <= 1; pl++)
            {
                int sign = pl == gameState.Turn ? 1 : -1;

                ulong opKingSurrounding = 0;
                ulong opKingQuarter = 0;
                if (engine.GameStage != GameStage.Endgame)
                {
                    //king safety
                    bool kingCastled = IsKingCastled(pl);
                    if (!kingCastled)
                    {
                        IrrevState irrevState = gameState.GetIrrevState();
                        if (!irrevState.CastlingAllowed[pl, 0] && !irrevState.CastlingAllowed[pl, 1])
                        {
                            score += sign * config.GetKingMiddlePenalty();
                        }
                    }
                    else
                    {
                        score += sign * config.GetKingCastledScore();
                    }

                    ulong opKing = gameState.Pieces[1 - pl, (int)PieceType.King];
                    int file = BitboardUtils.GetFile(BitboardUtils.BitScanForward(opKing));
                    opKingSurrounding = opKing | gameState.MoveGenerators[(int)PieceType.King].GenerateAttacks(1 - pl);
                    opKingQuarter = GetKingQuarter(1 - pl, file);
                    ulong pawnProtectors = gameState.Pieces[pl, (int)PieceType.Pawn] & opKingSurrounding;
                    score += sign * BitboardUtils.Popcount(pawnProtectors) * config.GetKingPawnProtectorsScore();
                }

                //pawn structure
                score += sign * EvaluatePawnStructure(pl);

                ulong attacks = 0;
                for (int i = 0; i < 5; i++)
                {
                    PieceType pieceType = (PieceType)i;
                    //material
                    score += sign * BitboardUtils.Popcount(gameState.Pieces[pl, i]) * config.GetPieceValue(pieceType);
                    pieceAttacks[pl, i] = gameState.MoveGenerators[i].GenerateAttacks(pl);
                    //mobility
                    score += sign * BitboardUtils.Popcount(pieceAttacks[pl, i]) * config.GetMobilityScore(engine.GameStage, pieceType);
                    if (pieceType == PieceType.Pawn)
                    {
                        //pawn central control
                        ulong pawnsCenterControl = (gameState.Pieces[pl, i] | pieceAttacks[pl, i]) & BitboardUtils.CENTER_MASK;
                        score += sign * BitboardUtils.SparsePopcount(pawnsCenterControl) * config.GetPawnCentralControlScore();
                        //pawn support
                        score += sign * BitboardUtils.SparsePopcount(pieceAttacks[pl, i] & gameState.Pieces[pl, i]) * config.GetPawnSupportScore();
                    }
                    else
                    {
                        attacks |= pieceAttacks[pl, i];
                    }

                    if (engine.GameStage != GameStage.Endgame)
                    {
                        //king attack
                        ulong kingAttacks = opKingSurrounding & pieceAttacks[pl, i];
                        score += sign * BitboardUtils.SparsePopcount(kingAttacks) * config.GetKingAttackScore();
                    }

                    ulong extendedKingAttacks = opKingQuarter & pieceAttacks[pl, i];
                    kingQuarterAttacks[pl] |= extendedKingAttacks;
                    kingQuarterAttacksAmount[pl] += BitboardUtils.Popcount(extendedKingAttacks);
                }

                //center control
                ulong piecesCenterControl = (gameState.Occupancy[pl] | attacks) & BitboardUtils.EXTENDED_CENTER_MASK;
                score += sign * BitboardUtils.Popcount(piecesCenterControl) * config.GetPieceCentralControlScore();
            }

            if (engine.GameStage != GameStage.Endgame)
            {
                //complete king attack analysis
                for (int pl = 0; pl <= 1; pl++)
                {
                    int sign = pl == gameState.Turn ? -1 : 1;
                    int defenseAmount = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        ulong defense = kingQuarterAttacks[1 - pl] & pieceAttacks[pl, i];
                        defenseAmount += BitboardUtils.Popcount(defense);
                    }
                    int opAttackScore = kingQuarterAttacksAmount[1 - pl] * config.GetExtendedKingAttackScore() - defenseAmount * config.GetExtendedKingDefenseScore();
                    if (opAttackScore > 0)
                    {
                        score += sign * opAttackScore;
                    }
                }
            }
            return score;
        }

        private ulong GetSafeKingsideSquares(int pl)
        {
            ulong mask = SAFE_KINGSIDE_SQUARES;
            if (pl == 1)
            {
                mask <<= 56;
            }
            return mask;
        }

        private ulong GetSafeQueensideSquares(int pl)
        {
            ulong mask = SAFE_QUEENSIDE_SQUARES;
            if (pl == 1)
            {
                mask <<= 56;
            }
            return mask;
        }

        private int EvaluatePawnStructure(int pl)
        {
            int score = 0;
            ulong pawns = gameState.Pieces[pl, (int)PieceType.Pawn];
            ulong abovePawns;
            if (pl == 0)
            {
                abovePawns = (pawns << 8) | (pawns << 16);
            }
            else
            {
                abovePawns = (pawns >> 8) | (pawns >> 16);
            }
            ulong doubledPawns = abovePawns & pawns;
            score += BitboardUtils.SparsePopcount(doubledPawns) * config.GetDoubledPawnsPenalty();
            pawns ^= doubledPawns;
            ulong pawnsCopy = pawns;
            int isolatedPawns = 0;
            int isolatedPawnsOnEmptyFile = 0;
            ulong opPawns = gameState.Pieces[1 - pl, (int)PieceType.Pawn];
            while (pawnsCopy > 0)
            {
                int pawn = BitboardUtils.PopLSB(ref pawnsCopy);
                int file = BitboardUtils.GetFile(pawn);
                int rank = BitboardUtils.GetRank(pawn);
                ulong leftFileMask = file > 0 ? BitboardUtils.GetFileMask(file - 1) : 0;
                ulong rightFileMask = file < 7 ? BitboardUtils.GetFileMask(file + 1) : 0;
                ulong fileMask = leftFileMask | rightFileMask;
                if ((pawns & fileMask) == 0)
                {
                    ulong aboveMask = BitboardUtils.GetFileMask(file);
                    if (pl == 0)
                    {
                        aboveMask <<= (rank + 1) * 8;
                    }
                    else
                    {
                        aboveMask >>= (8 - rank) * 8;
                    }
                    if ((opPawns & aboveMask) == 0)
                    {
                        isolatedPawnsOnEmptyFile++;
                    }
                    else
                    {
                        isolatedPawns++;
                    }
                }
            }
            score += isolatedPawns * config.GetIsolatedPawnPenalty();
            score += isolatedPawnsOnEmptyFile * config.GetIsolatedPawnOpenFilePenalty();
            return score;
        }

        public bool IsKingCastled(int pl)
        {
            ulong king = gameState.Pieces[pl, (int)PieceType.King];
            bool kingCastled = false;
            ulong kingside = GetSafeKingsideSquares(pl);
            ulong queenside = GetSafeQueensideSquares(pl);
            if ((kingside & king) > 0)
            {
                ulong rookPos = gameState.Pieces[pl, (int)PieceType.Rook] & kingside;
                if (rookPos == 0 || rookPos > king)
                {
                    kingCastled = true;
                }
            }
            else if ((queenside & king) > 0)
            {
                ulong rookPos = gameState.Pieces[pl, (int)PieceType.Rook] & queenside;
                if (rookPos == 0 || rookPos < king)
                {
                    kingCastled = true;
                }
            }
            return kingCastled;
        }

        private ulong GetKingQuarter(int pl, int kingFile)
        {
            ulong quarter = kingQuarter[kingFile];
            if (pl == 1)
            {
                quarter <<= 32;
            }
            return quarter;
        }
    }
}
