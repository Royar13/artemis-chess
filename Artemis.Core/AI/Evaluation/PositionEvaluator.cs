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
            int[] kingFile = {  BitboardUtils.GetFile(BitboardUtils.BitScanForward(gameState.Pieces[0, (int)PieceType.King])),
                                BitboardUtils.GetFile(BitboardUtils.BitScanForward(gameState.Pieces[1, (int)PieceType.King])) };
            double kingAttackModifier = 1 + Math.Min(1 + Math.Abs(kingFile[1] - kingFile[0]), 5) / (double)5 * 0.6;
            for (int pl = 0; pl <= 1; pl++)
            {
                int sign = pl == gameState.Turn ? 1 : -1;

                if (gameStage != GameStage.Endgame)
                {
                    //rooks connected
                    int rooksConnectedScore = GetRooksConnectedScore(pl);
                    score += sign * rooksConnectedScore;

                    //king safety
                    bool rooksConnected = rooksConnectedScore > 0;
                    int kingFileScore = sign * config.GetKingFileScore(pl, kingFile[pl], rooksConnected, gameState.GetIrrevState());
                    score += kingFileScore;

                    int startFile = Math.Max(0, kingFile[pl] - 1);
                    int endFile = Math.Min(7, kingFile[pl] + 1);
                    for (int f = startFile; f <= endFile; f++)
                    {
                        ulong fileMask = BitboardUtils.GetFileMask(f);
                        //moved king pawns penalty
                        if (kingFile[pl] != 4 || f == 5)
                        {
                            //don't penalize center pawn moves, except moves of the f pawn.
                            ulong friendlyPawn = fileMask & gameState.Pieces[pl, (int)PieceType.Pawn];
                            if (friendlyPawn > 0)
                            {
                                //least advanced pawn
                                int pawnRank = BitboardUtils.GetRank(pl == 0 ? BitboardUtils.BitScanForward(friendlyPawn) : BitboardUtils.BitScanBackward(friendlyPawn));
                                score += sign * config.GetKingPawnMovedPenalty(pl, pawnRank);
                            }
                            else
                            {
                                score += sign * config.GetNoKingPawnPenalty();
                            }
                        }

                        //pawn storm
                        ulong enemyPawn = fileMask & gameState.Pieces[1 - pl, (int)PieceType.Pawn];
                        if (enemyPawn > 0)
                        {
                            //most advanced pawn
                            int pawnRank = BitboardUtils.GetRank(pl == 0 ? BitboardUtils.BitScanForward(enemyPawn) : BitboardUtils.BitScanBackward(enemyPawn));
                            score -= sign * config.GetPawnStormScore(1 - pl, pawnRank);
                        }

                        //king open files penalty
                        if (enemyPawn == 0)
                        {
                            score += sign * config.GetKingOpenFilePenalty();
                        }
                    }
                }

                //pawn structure
                score += sign * EvaluatePawnStructure(pl);

                ulong opKingSurrounding = 0;
                ulong opKingQuarter = 0;

                ulong opKing = gameState.Pieces[1 - pl, (int)PieceType.King];
                int file = BitboardUtils.GetFile(BitboardUtils.BitScanForward(opKing));
                opKingSurrounding = opKing | gameState.MoveGenerators[(int)PieceType.King].GenerateAttacks(1 - pl);
                opKingQuarter = GetKingQuarter(1 - pl, file);

                for (int i = 0; i < 5; i++)
                {
                    PieceType pieceType = (PieceType)i;
                    //material
                    score += sign * BitboardUtils.Popcount(gameState.Pieces[pl, i]) * config.GetPieceValue(pieceType);

                    if (pieceType != PieceType.Pawn && gameStage != GameStage.Endgame)
                    {
                        ulong piece = gameState.Pieces[pl, i];
                        while (piece > 0)
                        {
                            int pieceSq = BitboardUtils.PopLSB(ref piece);
                            ulong attacks = gameState.MoveGenerators[i].GenerateAttacksFromSquare(pieceSq);

                            //mobility
                            score += sign * BitboardUtils.Popcount(attacks) * config.GetMobilityScore(gameStage, pieceType);

                            //center control
                            ulong piecesCenterControl = (gameState.Occupancy[pl] | attacks) & BitboardUtils.EXTENDED_CENTER_MASK;
                            score += sign * BitboardUtils.Popcount(piecesCenterControl) * config.GetPieceCentralControlScore();

                            //king attack
                            ulong directKingAttacks = opKingSurrounding & attacks;
                            ulong quarterKingAttacks = opKingQuarter & (attacks ^ directKingAttacks);
                            kingAttacks[pl] |= directKingAttacks | quarterKingAttacks;
                            kingAttacksScore[pl] += (int)((BitboardUtils.SparsePopcount(directKingAttacks) * config.GetPieceAttackScore(pieceType) +
                                BitboardUtils.Popcount(quarterKingAttacks) * config.GetExtendedPieceAttackScore(pieceType)) * kingAttackModifier);
                            pieceAttacks[pl, i] |= attacks;
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

        private int EvaluatePawnStructure(int pl)
        {
            int score = 0;
            ulong pawns = gameState.Pieces[pl, (int)PieceType.Pawn];
            ulong pawnAttacks = gameState.MoveGenerators[(int)PieceType.Pawn].GenerateAttacks(pl);

            //pawn central control
            ulong pawnsCenterControl = (pawns | pawnAttacks) & BitboardUtils.CENTER_MASK;
            score += BitboardUtils.SparsePopcount(pawnsCenterControl) * config.GetPawnCentralControlScore();
            //pawn support
            score += BitboardUtils.SparsePopcount(pawnAttacks & pawns) * config.GetPawnSupportScore();

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
            int score = 0;
            if (BitboardUtils.SparsePopcount(gameState.Pieces[pl, (int)PieceType.Rook]) == 2)
            {
                int rooksConnectedScore = config.GetRooksConnectedScore();
                ulong rooks = gameState.Pieces[pl, (int)PieceType.Rook];
                int firstRookSq = BitboardUtils.PopLSB(ref rooks);
                ulong firstRook = BitboardUtils.GetBitboard(firstRookSq);
                ulong firstRookAttacks = gameState.MoveGenerators[(int)PieceType.Rook].GenerateAttacksFromSquare(firstRookSq);
                int secondRookSq = BitboardUtils.PopLSB(ref rooks);
                ulong secondRook = BitboardUtils.GetBitboard(secondRookSq);
                if ((firstRookAttacks & secondRook) > 0)
                {
                    //fully connected
                    score = rooksConnectedScore;
                }
                else
                {
                    ulong secondRookAttacks = gameState.MoveGenerators[(int)PieceType.Rook].GenerateAttacksFromSquare(secondRookSq);
                    if ((firstRookAttacks & secondRookAttacks) > 0)
                    {
                        //semi-connected
                        score = (int)(rooksConnectedScore * 0.8);
                    }
                    else if ((firstRook & BitboardUtils.FIRST_RANK[pl]) > 0 && (secondRook & BitboardUtils.FIRST_RANK[pl]) > 0)
                    {
                        ulong sqBetweenRooks = secondRook ^ (secondRook - 2 * firstRook);
                        if ((gameState.Pieces[pl, (int)PieceType.King] & sqBetweenRooks) == 0)
                        {
                            //there can be at most 5 blockers
                            int blockers = BitboardUtils.Popcount(gameState.FullOccupancy & sqBetweenRooks);
                            score = (int)(rooksConnectedScore * (6 - blockers) / (double)6);
                        }
                    }
                }
            }
            else
            {
                score = 1;
            }
            return score;
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
