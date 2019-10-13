using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.AI.Evaluation
{
    public class PositionEvaluatorWithStats : PositionEvaluator
    {
        EvaluationStats stats;

        public PositionEvaluatorWithStats(GameState gameState, EvaluationConfig config) : base(gameState, config)
        {
        }

        public int Evaluate(int depth, GameStage gameStage, out EvaluationStats evaluationStats)
        {
            stats = new EvaluationStats();
            int score = Evaluate(depth, gameStage);
            int sign = gameState.Turn == 0 ? 1 : -1;
            stats.Score = sign * score;
            evaluationStats = stats;
            return score;
        }

        public override int EvaluateRooksConnected(int pl)
        {
            int score = base.EvaluateRooksConnected(pl);
            stats.RooksConnectedScore[pl] = ApplySign(pl, score);
            return score;
        }

        protected override double CalculateKingAttackModifier(int[] kingFile)
        {
            double modifier = base.CalculateKingAttackModifier(kingFile);
            stats.KingAttackModifier = modifier;
            return modifier;
        }

        protected override int CalculatePieceKingAttackScore(int pl, PieceType pieceType, ulong directKingAttacks, ulong quarterKingAttacks, double kingAttackModifier, int[] kingAttacksScore)
        {
            int score = base.CalculatePieceKingAttackScore(pl, pieceType, directKingAttacks, quarterKingAttacks, kingAttackModifier, kingAttacksScore);
            stats.KingAttackPieceScore[pl, (int)pieceType] += score;
            stats.KingAttackScore[pl] += score;
            return score;
        }

        protected override int CalculatePieceKingDefenseScore(int pl, PieceType pieceType, ulong pieceAttacks, ulong opKingAttacks, ulong kingSurrounding, int[] kingAttacksScore)
        {
            int score = base.CalculatePieceKingDefenseScore(pl, pieceType, pieceAttacks, opKingAttacks, kingSurrounding, kingAttacksScore);
            stats.KingDefensePieceScore[pl, (int)pieceType] += score;
            stats.KingDefenseScore[pl] += score;
            return score;
        }

        protected override int EvaluateAttackVsDefense(int pl, int[] kingAttacksScore)
        {
            int score = base.EvaluateAttackVsDefense(pl, kingAttacksScore);
            stats.AttackDefenseDif[pl] = ApplySign(pl, score);
            return score;
        }

        protected override int EvaluateEndgameCornerMate(int pl, int[] material)
        {
            int score = base.EvaluateEndgameCornerMate(pl, material);
            stats.EndgameCornerMate[pl] = ApplySign(pl, score);
            return score;
        }

        protected override int EvaluateEndgameKingSquare(int pl)
        {
            int score = base.EvaluateEndgameKingSquare(pl);
            stats.EndgameKingSquare[pl] = ApplySign(pl, score);
            return score;
        }

        protected override int EvaluateKingFile(int pl, int kingFile, int rooksConnectedScore)
        {
            int score = base.EvaluateKingFile(pl, kingFile, rooksConnectedScore);
            stats.KingFileScore[pl] = ApplySign(pl, score);
            stats.KingSafetyScore[pl] += ApplySign(pl, score);
            return score;
        }

        protected override int EvaluateKingPawnMoves(int pl, int moves)
        {
            int score = base.EvaluateKingPawnMoves(pl, moves);
            stats.KingPawnMoves[pl] += moves;
            stats.KingPawnMovesScore[pl] += ApplySign(pl, score);
            stats.KingSafetyScore[pl] += ApplySign(pl, score);
            return score;
        }

        protected override int EvaluateMaterial(int pl, PieceType pieceType, int[] materialArr)
        {
            int score = base.EvaluateMaterial(pl, pieceType, materialArr);
            stats.MaterialScore[pl] += ApplySign(pl, score);
            return score;
        }

        protected override int EvaluateMobility(int pl, PieceType pieceType, ulong pieceAttacks, GameStage gameStage)
        {
            int score = base.EvaluateMobility(pl, pieceType, pieceAttacks, gameStage);
            stats.MobilityPieceScore[pl, (int)pieceType] += ApplySign(pl, score);
            stats.MobilityScore[pl] += ApplySign(pl, score);
            return score;
        }

        protected override int EvaluateOpenKingFile(int pl, ulong enemyPawnsOnFile)
        {
            int score = base.EvaluateOpenKingFile(pl, enemyPawnsOnFile);
            stats.OpenKingFilesScore[pl] += ApplySign(pl, score);
            stats.KingSafetyScore[pl] += ApplySign(pl, score);
            return score;
        }

        protected override int EvaluatePawnStorm(int pl, ulong pawnsOnFile, double kingAttackModifier)
        {
            int score = base.EvaluatePawnStorm(pl, pawnsOnFile, kingAttackModifier);
            stats.PawnStormScore[pl] += ApplySign(pl, score);
            stats.KingSafetyScore[1 - pl] += ApplySign(pl, score);
            return score;
        }

        protected override int EvaluatePawnStructure(int pl)
        {
            int score = base.EvaluatePawnStructure(pl);
            stats.PawnStructureScore[pl] = ApplySign(pl, score);
            return score;
        }

        protected override int EvaluatePieceCenterControl(int pl, PieceType pieceType, ulong pieceAttacks)
        {
            int score = base.EvaluatePieceCenterControl(pl, pieceType, pieceAttacks);
            stats.CenterControlPieceScore[pl, (int)pieceType] += ApplySign(pl, score);
            stats.CenterControlScore[pl] += ApplySign(pl, score);
            return score;
        }
    }
}
