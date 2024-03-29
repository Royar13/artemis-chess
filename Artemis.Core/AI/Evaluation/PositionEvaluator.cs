﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.AI.Evaluation
{
    public class PositionEvaluator
    {
        protected GameState gameState;
        protected EvaluationConfig config;
        public const int CHECKMATE_SCORE = 1000000;
        /// <summary>
        /// A mask of the 16 squares (quarter of the board) around the king, indexed by the king's file
        /// </summary>
        private readonly ulong[] kingQuarter = { 0x0F0F0F0F, 0x0F0F0F0F, 0x0F0F0F0F,
            0x3C3C3C3C, 0x3C3C3C3C, 0xF0F0F0F0, 0xF0F0F0F0, 0xF0F0F0F0 };

        public PositionEvaluator(GameState gameState, EvaluationConfig config)
        {
            this.gameState = gameState;
            this.config = config;
        }

        protected int ApplySign(int pl, int score)
        {
            int sign = pl == gameState.Turn ? 1 : -1;
            return sign * score;
        }

        public int Evaluate(int depth, GameStage gameStage, int engineColor)
        {
            GameResultData resultData = gameState.GetResult();
            if (resultData.Result != GameResult.Ongoing)
            {
                if (resultData.Result == GameResult.Win)
                {
                    return -CHECKMATE_SCORE - depth;
                }
                else
                {
                    return GetDrawScore(engineColor);
                }
            }

            int score = 0;
            ulong[,] pieceAttacks = new ulong[2, 5];
            ulong[] kingAttacks = new ulong[2];
            int[] kingAttacksScore = new int[2];
            ulong[] kingSurrounding = new ulong[2];
            int[] kingFile = {  BitboardUtils.GetFile(BitboardUtils.BitScanForward(gameState.Pieces[0, (int)PieceType.King])),
                                BitboardUtils.GetFile(BitboardUtils.BitScanForward(gameState.Pieces[1, (int)PieceType.King])) };
            double[] kingAttackModifier = CalculateKingAttackModifier(kingFile);
            int[] material = new int[2];
            for (int pl = 0; pl <= 1; pl++)
            {
                if (gameStage != GameStage.Endgame)
                {
                    //rooks connected
                    int rooksConnectedScore = EvaluateRooksConnected(pl);
                    score += rooksConnectedScore;

                    //king safety
                    score += EvaluateKingFile(pl, kingFile[pl], rooksConnectedScore);

                    int startFile = Math.Min(5, Math.Max(0, kingFile[pl] - 1));
                    int pawnProtectorsMoves = 0;
                    for (int f = startFile; f <= startFile + 2; f++)
                    {
                        ulong fileMask = BitboardUtils.GetFileMask(f);
                        //moved king pawns penalty
                        pawnProtectorsMoves += GetPawnMovesOfFile(pl, f, kingFile[pl]);

                        //pawn storm
                        ulong enemyPawnsOnFile = fileMask & gameState.Pieces[1 - pl, (int)PieceType.Pawn];
                        score += EvaluatePawnStorm(1 - pl, enemyPawnsOnFile, kingAttackModifier[1 - pl]);

                        //king open files penalty
                        score += EvaluateOpenKingFile(pl, enemyPawnsOnFile, kingAttackModifier[1 - pl]);
                    }
                    score += EvaluateKingPawnMoves(pl, pawnProtectorsMoves, kingAttackModifier[1 - pl]);
                }

                //pawn structure
                score += EvaluatePawnStructure(pl);

                ulong opKing = gameState.Pieces[1 - pl, (int)PieceType.King];
                int file = BitboardUtils.GetFile(BitboardUtils.BitScanForward(opKing));
                kingSurrounding[1 - pl] = opKing | gameState.MoveGenerators[(int)PieceType.King].GenerateAttacks(1 - pl);
                ulong opKingQuarter = GetKingQuarter(1 - pl, file);

                for (int i = 0; i < 5; i++)
                {
                    PieceType pieceType = (PieceType)i;
                    //material
                    score += EvaluateMaterial(pl, pieceType, material);

                    if (pieceType != PieceType.Pawn)
                    {
                        ulong piece = gameState.Pieces[pl, i];
                        while (piece > 0)
                        {
                            int pieceSq = BitboardUtils.PopLSB(ref piece);
                            ulong attacks = gameState.MoveGenerators[i].GenerateAttacksFromSquare(pieceSq);

                            //mobility
                            score += EvaluateMobility(pl, pieceType, attacks, gameStage);

                            //center control
                            score += EvaluatePieceCenterControl(pl, pieceType, attacks);

                            if (pieceType == PieceType.Rook)
                            {
                                //rook on open file & 7th rank
                                score += EvaluateRookOpenFile(pl, pieceSq);
                                score += EvaluateRookRank(pl, pieceSq, gameStage);
                            }
                            else if (pieceType == PieceType.Knight)
                            {
                                //knight outpost
                                score += EvaluateKnightSquare(pl, pieceSq);
                            }

                            if (gameStage != GameStage.Endgame)
                            {
                                //king attack
                                ulong directKingAttacks = kingSurrounding[1 - pl] & attacks;
                                ulong quarterKingAttacks = opKingQuarter & (attacks ^ directKingAttacks);
                                kingAttacks[pl] |= directKingAttacks | quarterKingAttacks;
                                CalculatePieceKingAttackScore(pl, pieceType, directKingAttacks, quarterKingAttacks, kingAttackModifier[pl], kingAttacksScore);
                                pieceAttacks[pl, i] |= attacks;
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
                    for (int i = 0; i < 5; i++)
                    {
                        CalculatePieceKingDefenseScore(pl, (PieceType)i, pieceAttacks[pl, i], kingAttacks[1 - pl], kingSurrounding[pl], kingAttacksScore);
                    }
                    score += EvaluateAttackVsDefense(1 - pl, kingAttacksScore);
                }
            }
            else
            {
                //endgame specific parameters
                for (int pl = 0; pl <= 1; pl++)
                {
                    //advanced king bonus
                    score += EvaluateEndgameKingSquare(pl);

                    //big material advantage
                    //march king towards enemy king, and push enemy king to the corner
                    score += EvaluateEndgameCornerMate(pl, material);

                    //King & Pawn vs King
                    score += EvaluateEndgameKPK(pl, material);
                }
            }
            return score;
        }

        public int GetDrawScore(int engineColor)
        {
            int sign = gameState.Turn == engineColor ? -1 : 1;
            int score = sign * config.GetContemptFactor();
            return score;
        }

        protected virtual double[] CalculateKingAttackModifier(int[] kingFile)
        {
            double[] modifier = new double[2];
            double modifierBase = 1 + Math.Min(1 + Math.Abs(kingFile[1] - kingFile[0]), 5) / (double)5 * 0.6;
            for (int pl = 0; pl <= 1; pl++)
            {
                modifier[pl] = modifierBase;
                if (gameState.Pieces[pl, (int)PieceType.Queen] == 0)
                {
                    modifier[pl] *= 0.1;
                }
                if (gameState.Pieces[pl, (int)PieceType.Rook] == 0)
                {
                    modifier[pl] *= 0.5;
                }
            }
            return modifier;
        }

        protected virtual int GetPawnMoves(int pl, int rank)
        {
            if (pl == 0)
            {
                return rank - 1;
            }
            else
            {
                return 6 - rank;
            }
        }

        protected virtual int GetPawnMovesOfFile(int pl, int file, int kingFile)
        {
            int pawnMoves = 0;
            if (kingFile != 4 || file == 5)
            {
                //don't penalize center pawn moves, except moves of the f pawn.
                ulong fileMask = BitboardUtils.GetFileMask(file);
                ulong friendlyPawn = fileMask & gameState.Pieces[pl, (int)PieceType.Pawn];
                if (friendlyPawn > 0)
                {
                    int pawnRank = BitboardUtils.GetRank(BitboardUtils.GetLeastAdvanced(pl, friendlyPawn));
                    pawnMoves += Math.Min(3, GetPawnMoves(pl, pawnRank));
                }
                else
                {
                    pawnMoves += 3;
                }
            }
            return pawnMoves;
        }

        protected virtual int EvaluateKingPawnMoves(int pl, int moves, double opKingAttackModifier)
        {
            int score = config.GetKingPawnMovedPenalty(moves, opKingAttackModifier);
            return ApplySign(pl, score);
        }

        protected virtual int EvaluatePawnStorm(int pl, ulong pawnsOnFile, double kingAttackModifier)
        {
            int score = 0;
            if (pawnsOnFile > 0)
            {
                int pawnRank = BitboardUtils.GetRank(BitboardUtils.GetMostAdvanced(pl, pawnsOnFile));
                score = (int)(config.GetPawnStormScore(pl, pawnRank) * kingAttackModifier);
            }
            return ApplySign(pl, score);
        }

        protected virtual int EvaluateOpenKingFile(int pl, ulong enemyPawnsOnFile, double opKingAttackModifier)
        {
            int score = 0;
            if (enemyPawnsOnFile == 0)
            {
                score = config.GetKingOpenFilePenalty(opKingAttackModifier);
            }
            return ApplySign(pl, score);
        }

        protected virtual int EvaluateKingFile(int pl, int kingFile, int rooksConnectedScore)
        {
            bool rooksConnected = Math.Abs(rooksConnectedScore) > 0;
            int score = config.GetKingFileScore(pl, kingFile, rooksConnected, gameState.GetIrrevState());
            return ApplySign(pl, score);
        }

        protected virtual int EvaluateMaterial(int pl, PieceType pieceType, int[] materialArr)
        {
            int score = BitboardUtils.Popcount(gameState.Pieces[pl, (int)pieceType]) * config.GetPieceValue(pieceType);
            materialArr[pl] += score;
            return ApplySign(pl, score);
        }

        protected virtual int EvaluateMobility(int pl, PieceType pieceType, ulong pieceAttacks, GameStage gameStage)
        {
            int score = BitboardUtils.Popcount(pieceAttacks) * config.GetMobilityScore(gameStage, pieceType);
            return ApplySign(pl, score);
        }

        protected virtual int EvaluatePieceCenterControl(int pl, PieceType pieceType, ulong pieceAttacks)
        {
            ulong pieceCenterControl = (gameState.Pieces[pl, (int)pieceType] | pieceAttacks) & BitboardUtils.EXTENDED_CENTER_MASK;
            int score = BitboardUtils.Popcount(pieceCenterControl) * config.GetPieceCentralControlScore();
            return ApplySign(pl, score);
        }

        protected virtual int CalculatePieceKingAttackScore(int pl, PieceType pieceType, ulong directKingAttacks, ulong quarterKingAttacks, double kingAttackModifier, int[] kingAttacksScore)
        {
            int score = (int)((BitboardUtils.SparsePopcount(directKingAttacks) * config.GetPieceAttackScore(pieceType) +
                                BitboardUtils.Popcount(quarterKingAttacks) * config.GetExtendedPieceAttackScore(pieceType)) * kingAttackModifier);
            kingAttacksScore[pl] += score;
            return score;
        }

        protected virtual int CalculatePieceKingDefenseScore(int pl, PieceType pieceType, ulong pieceAttacks, ulong opKingAttacks, ulong kingSurrounding, int[] kingAttacksScore)
        {
            ulong defense = opKingAttacks & pieceAttacks & kingSurrounding;
            ulong extendedDefense = (opKingAttacks & pieceAttacks) ^ defense;
            int score = BitboardUtils.Popcount(defense) * config.GetPieceDefenseScore(pieceType) +
                BitboardUtils.Popcount(extendedDefense) * config.GetExtendedPieceDefenseScore(pieceType);
            kingAttacksScore[1 - pl] -= score;
            return score;
        }

        protected virtual int EvaluateAttackVsDefense(int pl, int[] kingAttacksScore)
        {
            int score = 0;
            if (kingAttacksScore[pl] > 0)
            {
                score = ApplySign(pl, kingAttacksScore[pl]);
            }
            return score;
        }

        protected virtual int EvaluateRookRank(int pl, int rookSqInd, GameStage gameStage)
        {
            int rank = BitboardUtils.GetRank(rookSqInd);
            int score = config.GetRookRankScore(pl, rank, gameStage);
            return ApplySign(pl, score);
        }

        protected virtual int EvaluateRookOpenFile(int pl, int rookSqInd)
        {
            int score = 0;
            int file = BitboardUtils.GetFile(rookSqInd);
            ulong fileMask = BitboardUtils.GetFileMask(file);
            ulong pawns = gameState.Pieces[pl, (int)PieceType.Pawn] & fileMask;
            if (pawns == 0)
            {
                score = config.GetRookOpenFileScore();
            }
            return ApplySign(pl, score);
        }

        protected virtual int EvaluatePawnStructure(int pl)
        {
            int score = 0;
            ulong pawns = gameState.Pieces[pl, (int)PieceType.Pawn];
            ulong pawnAttacks = gameState.MoveGenerators[(int)PieceType.Pawn].GenerateAttacks(pl);

            //pawn central control
            ulong pawnsCenterControl = (pawns | pawnAttacks) & BitboardUtils.CENTER_MASK;
            score += BitboardUtils.SparsePopcount(pawnsCenterControl) * config.GetPawnCentralControlScore();
            //pawn support
            score += BitboardUtils.SparsePopcount(pawnAttacks & pawns) * config.GetPawnSupportScore();

            //doubled pawns
            ulong doubledPawns;
            score += CalculateDoubledPawnsPenalty(pl, pawns, out doubledPawns);
            pawns ^= doubledPawns;

            ulong pawnsCopy = pawns;
            ulong opPawns = gameState.Pieces[1 - pl, (int)PieceType.Pawn];
            while (pawnsCopy > 0)
            {
                int pawn = BitboardUtils.PopLSB(ref pawnsCopy);
                int file = BitboardUtils.GetFile(pawn);
                int rank = BitboardUtils.GetRank(pawn);
                //space
                score += EvaluateSpace(pl, rank);

                ulong leftFileMask = file > 0 ? BitboardUtils.GetFileMask(file - 1) : 0;
                ulong rightFileMask = file < 7 ? BitboardUtils.GetFileMask(file + 1) : 0;
                ulong fileMask = leftFileMask | rightFileMask;
                ulong threeFilesMask = fileMask | BitboardUtils.GetFileMask(file);
                //passed pawn
                score += CalculatePassedPawnScore(pl, pawn, rank, threeFilesMask);

                //isolated pawn
                score += CalculateIsolatedPawnPenalty(pl, rank, file, fileMask, pawns, opPawns);
            }
            return ApplySign(pl, score);
        }

        protected virtual int CalculateDoubledPawnsPenalty(int pl, ulong pawns, out ulong doubledPawns)
        {
            ulong belowPawns;
            if (pl == 0)
            {
                belowPawns = (pawns >> 8) | (pawns >> 16);
            }
            else
            {
                belowPawns = (pawns << 8) | (pawns << 16);
            }
            doubledPawns = belowPawns & pawns;
            int score = BitboardUtils.SparsePopcount(doubledPawns) * config.GetDoubledPawnsPenalty();
            return score;
        }

        protected virtual int CalculateIsolatedPawnPenalty(int pl, int pawnRank, int pawnFile, ulong leftRightFilesMask, ulong pawns, ulong opPawns)
        {
            int score = 0;
            if ((pawns & leftRightFilesMask) == 0)
            {
                ulong aboveMask = BitboardUtils.GetFileMask(pawnFile);
                if (pl == 0)
                {
                    aboveMask <<= (pawnRank + 1) * 8;
                }
                else
                {
                    aboveMask >>= (8 - pawnRank) * 8;
                }
                if ((opPawns & aboveMask) == 0)
                {
                    score = config.GetIsolatedPawnOpenFilePenalty();
                }
                else
                {
                    score = config.GetIsolatedPawnPenalty();
                }
            }
            return score;
        }

        public virtual int EvaluateRooksConnected(int pl)
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
            return ApplySign(pl, score);
        }

        /// <summary>
        /// Give a score to a pawn if it's a passed pawn
        /// </summary>
        /// <param name="pl">Player that has the pawn</param>
        /// <param name="pawnRank">Rank of the pawn (1-6)</param>
        /// <param name="threeFilesMask">The files to the left and right of the pawn, and the pawn's file.
        /// Only two files for A and H pawns.</param>
        /// <returns></returns>
        protected virtual int CalculatePassedPawnScore(int pl, int pawnSq, int pawnRank, ulong threeFilesMask)
        {
            int score = 0;
            if (pl == 0)
            {
                threeFilesMask <<= (pawnRank + 1) * 8;
            }
            else
            {
                threeFilesMask >>= (8 - pawnRank) * 8;
            }
            ulong enemyPawns = gameState.Pieces[1 - pl, (int)PieceType.Pawn];
            ulong enemyPawnsOnFiles = threeFilesMask & enemyPawns;
            if (enemyPawnsOnFiles == 0)
            {
                ulong attacksBackwards = gameState.MoveGenerators[(int)PieceType.Pawn].GenerateAttacksFromSquare(pawnSq, 1 - pl);
                int pawnDefenders = BitboardUtils.SparsePopcount(attacksBackwards & gameState.Pieces[pl, (int)PieceType.Pawn]);
                score = config.GetPassedPawnScore(pl, pawnRank, pawnDefenders);
            }
            return score;
        }

        protected virtual int EvaluateSpace(int pl, int pawnRank)
        {
            int advances = pl == 0 ? pawnRank - 1 : 6 - pawnRank;
            int score = advances * config.GetSpaceScore();
            return ApplySign(pl, score);
        }

        protected virtual int EvaluateKnightSquare(int pl, int sq)
        {
            int file = BitboardUtils.GetFile(sq);
            int rank = BitboardUtils.GetRank(sq);
            ulong leftFileMask = file > 0 ? BitboardUtils.GetFileMask(file - 1) : 0;
            ulong rightFileMask = file < 7 ? BitboardUtils.GetFileMask(file + 1) : 0;
            ulong fileMask = leftFileMask | rightFileMask;
            if (pl == 0)
            {
                fileMask <<= (rank + 1) * 8;
            }
            else
            {
                fileMask >>= (8 - rank) * 8;
            }
            bool outpost = false;
            if ((gameState.Pieces[1 - pl, (int)PieceType.Pawn] & fileMask) == 0)
            {
                outpost = true;
            }
            ulong pawnDefenders = gameState.MoveGenerators[(int)PieceType.Pawn].GenerateAttacksFromSquare(sq, 1 - pl) & gameState.Pieces[pl, (int)PieceType.Pawn];
            int pawnDefendersAmount = BitboardUtils.SparsePopcount(pawnDefenders);
            int score = config.GetKnightSquareScore(pl, sq, pawnDefendersAmount, outpost);
            return ApplySign(pl, score);
        }

        protected virtual int EvaluateEndgameKingSquare(int pl)
        {
            int kingSq = BitboardUtils.GetSquareInd(gameState.Pieces[pl, (int)PieceType.King]);
            int score = config.GetEndgameKingSquareScore(pl, kingSq);
            return ApplySign(pl, score);
        }

        protected virtual int EvaluateEndgameCornerMate(int pl, int[] material)
        {
            int score = 0;
            if (material[pl] - material[1 - pl] >= config.GetPieceValue(PieceType.Rook))
            {
                int king = BitboardUtils.GetSquareInd(gameState.Pieces[pl, (int)PieceType.King]);
                int enemyKing = BitboardUtils.GetSquareInd(gameState.Pieces[1 - pl, (int)PieceType.King]);
                int enemyKingCenterDistance = BitboardUtils.DistanceToCenter(enemyKing);
                int kingsDistance = BitboardUtils.Distance(king, enemyKing);
                score += config.GetEndgameEnemyKingCenterDistanceScore(enemyKingCenterDistance) + config.GetEndgameKingsDistanceScore(kingsDistance);
            }
            return ApplySign(pl, score);
        }

        protected virtual int EvaluateEndgameKPK(int pl, int[] material)
        {
            int score = 0;
            if (material[pl] == config.GetPieceValue(PieceType.Pawn) && material[1 - pl] == 0)
            {
                int winConditions = 0;
                int king = BitboardUtils.GetSquareInd(gameState.Pieces[pl, (int)PieceType.King]);
                int kingRank = BitboardUtils.GetRank(king);
                int pawn = BitboardUtils.GetSquareInd(gameState.Pieces[pl, (int)PieceType.Pawn]);
                int pawnRank = BitboardUtils.GetRank(pawn);
                if (GetOpposition() == pl)
                {
                    //has opposition
                    winConditions++;
                }

                if (pl == 0)
                {
                    if (kingRank >= 5)
                    {
                        //6th rank
                        winConditions++;
                    }
                    if (kingRank > pawnRank)
                    {
                        //king in front of pawn
                        winConditions++;
                    }
                }
                else
                {
                    if (kingRank <= 2)
                    {
                        //6th rank
                        winConditions++;
                    }
                    if (kingRank < pawnRank)
                    {
                        //king in front of pawn
                        winConditions++;
                    }
                }
                if (winConditions >= 2)
                {
                    score += 5 * config.GetPieceValue(PieceType.Pawn);
                }
            }
            return ApplySign(pl, score);
        }

        protected int GetOpposition()
        {
            int wking = BitboardUtils.GetSquareInd(gameState.Pieces[0, (int)PieceType.King]);
            int bking = BitboardUtils.GetSquareInd(gameState.Pieces[1, (int)PieceType.King]);
            int distance = BitboardUtils.Distance(wking, bking);
            int opposition = distance % 2 == 0 ? 1 - gameState.Turn : gameState.Turn;
            return opposition;
        }

        protected virtual ulong GetKingQuarter(int pl, int kingFile)
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
