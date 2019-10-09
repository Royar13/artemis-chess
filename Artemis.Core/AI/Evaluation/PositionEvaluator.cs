using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.AI.Evaluation
{
    public class PositionEvaluator
    {
        GameState gameState;
        EvaluationConfig config;
        public const int CHECKMATE_SCORE = 1000000;
        /// <summary>
        /// A mask of the 16 squares (quarter of the board) around the king, indexed by the king's file
        /// </summary>
        private readonly ulong[] kingQuarter = { 0x0F0F0F0F00000000, 0x0F0F0F0F00000000, 0x0F0F0F0F00000000,
            0x3C3C3C3C00000000, 0x3C3C3C3C00000000,
            0xF0F0F0F000000000, 0xF0F0F0F000000000, 0xF0F0F0F000000000 };

        public PositionEvaluator(GameState gameState, EvaluationConfig config)
        {
            this.gameState = gameState;
            this.config = config;
        }

        public int Evaluate(int depth, GameStage gameStage)
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
            ulong[] kingAttacks = new ulong[2];
            int[] kingAttacksScore = new int[2];
            int[] castlingSide = { -1, -1 };
            double[] kingAttackModifier = { 1, 1 };
            for (int pl = 0; pl <= 1; pl++)
            {
                int sign = pl == gameState.Turn ? 1 : -1;

                if (gameStage != GameStage.Endgame)
                {
                    //king safety
                    bool kingCastled = IsKingCastled(pl, out castlingSide[pl]);
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
                        score += sign * config.GetKingCastledScore(castlingSide[pl]);
                    }
                }
            }

            if (castlingSide[0] != castlingSide[1])
            {
                if (castlingSide[0] >= 0)
                {
                    kingAttackModifier[0] = 1.5;
                }
                if (castlingSide[1] >= 0)
                {
                    kingAttackModifier[1] = 1.5;
                }
            }

            for (int pl = 0; pl <= 1; pl++)
            {
                int sign = pl == gameState.Turn ? 1 : -1;

                //pawn structure
                score += sign * EvaluatePawnStructure(pl);

                ulong opKingSurrounding = 0;
                ulong opKingQuarter = 0;

                ulong opKing = gameState.Pieces[1 - pl, (int)PieceType.King];
                int file = BitboardUtils.GetFile(BitboardUtils.BitScanForward(opKing));
                opKingSurrounding = opKing | gameState.MoveGenerators[(int)PieceType.King].GenerateAttacks(1 - pl);
                opKingQuarter = GetKingQuarter(1 - pl, file);
                ulong pawnProtectors = gameState.Pieces[pl, (int)PieceType.Pawn] & opKingSurrounding;
                score += sign * BitboardUtils.Popcount(pawnProtectors) * config.GetKingPawnProtectorsScore();

                for (int i = 0; i < 5; i++)
                {

                    PieceType pieceType = (PieceType)i;
                    //material
                    score += sign * BitboardUtils.Popcount(gameState.Pieces[pl, i]) * config.GetPieceValue(pieceType);
                    pieceAttacks[pl, i] = gameState.MoveGenerators[i].GenerateAttacks(pl);
                    //mobility
                    score += sign * BitboardUtils.Popcount(pieceAttacks[pl, i]) * config.GetMobilityScore(gameStage, pieceType);

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
                        if (gameStage != GameStage.Endgame)
                        {
                            ulong piece = gameState.Pieces[pl, i];
                            while (piece > 0)
                            {
                                ulong pieceSq = BitboardUtils.GetLSB(piece);

                                ulong attacks= gameState.MoveGenerators[i].
                                //center control
                                ulong piecesCenterControl = (gameState.Occupancy[pl] | attacks) & BitboardUtils.EXTENDED_CENTER_MASK;
                                score += sign * BitboardUtils.Popcount(piecesCenterControl) * config.GetPieceCentralControlScore();

                                //king attack
                                ulong directKingAttacks = opKingSurrounding & pieceAttacks[pl, i];
                                ulong quarterKingAttacks = opKingQuarter & (pieceAttacks[pl, i] ^ directKingAttacks);
                                kingAttacks[pl] |= directKingAttacks | quarterKingAttacks;
                                kingAttacksScore[pl] += (int)((BitboardUtils.SparsePopcount(directKingAttacks) * config.GetPieceAttackScore(pieceType) +
                                    BitboardUtils.Popcount(quarterKingAttacks) * config.GetExtendedPieceAttackScore(pieceType)) * kingAttackModifier[pl]);

                                piece ^= pieceSq;
                            }
                        }
                    }


                }
            }

            if (gameStage != GameStage.Endgame)
            {
                //complete king attack analysis
                for (int pl = 0; pl <= 1; pl++)
                {
                    int sign = pl == gameState.Turn ? -1 : 1;
                    int defenseScore = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        ulong defense = kingAttacks[1 - pl] & pieceAttacks[pl, i];
                        defenseScore += BitboardUtils.Popcount(defense) * config.GetPieceDefenseScore((PieceType)i);
                    }
                    int opAttackScore = kingAttacksScore[1 - pl] - defenseScore;
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

        public int GetRooksConnectedScore(int pl)
        {

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
