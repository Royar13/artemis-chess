using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.AI.Evaluation
{
    public class EvaluationStats
    {
        public int Score;
        public double[] KingAttackModifier;
        public int[] RooksConnectedScore = new int[2];
        public int[] KingSafetyScore = new int[2];
        public int[] KingFileScore = new int[2];
        public int[] KingPawnMoves = new int[2];
        public int[] KingPawnMovesScore = new int[2];
        public int[] PawnStormScore = new int[2];
        public int[] OpenKingFilesScore = new int[2];
        public int[] PawnStructureScore = new int[2];
        public int[] DoubledPawnsPenalty = new int[2];
        public int[] IsolatedPawnsPenalty = new int[2];
        public int[] SpaceScore = new int[2];
        public int[] PassedPawnScore = new int[2];
        public int[] MaterialScore = new int[2];
        public int[] MobilityScore = new int[2];
        public int[,] MobilityPieceScore = new int[2, 5];
        public int[] CenterControlScore = new int[2];
        public int[,] CenterControlPieceScore = new int[2, 5];
        public int[] RookOpenFileScore = new int[2];
        public int[] RookRankScore = new int[2];
        public int[] KnightSquareScore = new int[2];
        public int[] KingAttackScore = new int[2];
        public int[,] KingAttackPieceScore = new int[2, 5];
        public int[] KingDefenseScore = new int[2];
        public int[,] KingDefensePieceScore = new int[2, 5];
        public int[] AttackDefenseDif = new int[2];
        public int[] EndgameKingSquare = new int[2];
        public int[] EndgameCornerMate = new int[2];
        public int[] EndgameKPK = new int[2];

        string lineSep = "----------------------------------------------------------------------------------------------------------------------------------------------------";
        string secondSep = "\t\t\t\t\t";
        string pieceSep = "\t";

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            string pieceTitles = $"Rook{pieceSep}| Knight{pieceSep}| Bishop{pieceSep}| Queen{pieceSep}\t";
            string scoreStr = Score > 0 ? "+" + Score : Score.ToString();
            builder.AppendLine($"Score\t\t\t| {scoreStr}");
            builder.AppendLine(lineSep);
            builder.AppendLine($"\t\t\t| White{secondSep}| Black");
            builder.AppendLine(lineSep);
            AppendStat(builder, "Rooks Connected\t\t", RooksConnectedScore);
            AppendStat(builder, "King Attack Modifier\t", KingAttackModifier);
            AppendStat(builder, "King Safety\t\t", KingSafetyScore);
            AppendStat(builder, "King File\t\t\t", KingFileScore);
            AppendStat(builder, "King Pawn Moves Amount\t", KingPawnMoves);
            AppendStat(builder, "King Pawn Moves\t\t", KingPawnMovesScore);
            AppendStat(builder, "King Open Files\t\t", OpenKingFilesScore);
            AppendStat(builder, "Pawn Storm\t\t", PawnStormScore);
            AppendStat(builder, "Pawn Structure\t\t", PawnStructureScore);
            AppendStat(builder, "Doubled Pawns\t\t", DoubledPawnsPenalty);
            AppendStat(builder, "Isolated Pawns\t\t", IsolatedPawnsPenalty);
            AppendStat(builder, "Space\t\t\t", SpaceScore);
            AppendStat(builder, "Passed Pawn\t\t", PassedPawnScore);
            AppendStat(builder, "Material\t\t\t", MaterialScore);
            AppendStat(builder, "Mobility\t\t\t", MobilityScore);
            AppendStat(builder, "Center Control\t\t", CenterControlScore);
            AppendStat(builder, "Rook Open File\t\t", RookOpenFileScore);
            AppendStat(builder, "Rook Rank\t\t", RookRankScore);
            AppendStat(builder, "Knight Square\t\t", KnightSquareScore);
            AppendStat(builder, "King Attack\t\t", KingAttackScore);
            AppendStat(builder, "King Defense\t\t", KingDefenseScore);
            AppendStat(builder, "Attack-Defense Dif\t", AttackDefenseDif);
            builder.AppendLine($"\t\t\t| {pieceTitles}| {pieceTitles}");
            builder.AppendLine(lineSep);
            AppendPieceStat(builder, "Mobility\t\t\t", MobilityPieceScore);
            AppendPieceStat(builder, "Center Control\t\t", CenterControlPieceScore);
            AppendPieceStat(builder, "King Attack\t\t", KingAttackPieceScore);
            AppendPieceStat(builder, "King Defense\t\t", KingDefensePieceScore);
            builder.AppendLine($"\t\t\t\t\t\tENDGAME");
            builder.AppendLine(lineSep);
            AppendStat(builder, "King Square\t\t", EndgameKingSquare);
            AppendStat(builder, "Corner Mate\t\t", EndgameCornerMate);
            AppendStat(builder, "KPK\t\t\t", EndgameKPK);
            return builder.ToString();
        }

        private void AppendStat(StringBuilder builder, string title, int[] scores)
        {
            builder.AppendLine($"{title}| {scores[0]}{secondSep}| {scores[1]}");
            builder.AppendLine(lineSep);
        }

        private void AppendStat(StringBuilder builder, string title, double[] scores)
        {
            builder.AppendLine($"{title}| {scores[0]}{secondSep}| {scores[1]}");
            builder.AppendLine(lineSep);
        }

        private void AppendPieceStat(StringBuilder builder, string title, int[,] scores)
        {
            StringBuilder line = new StringBuilder($"{title}");
            for (int pl = 0; pl <= 1; pl++)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (i != 3)
                    {
                        line.Append($"| {scores[pl, i]}\t");
                    }
                }

                if (pl == 0)
                {
                    line.Append($"\t");
                }
            }
            builder.AppendLine(line.ToString());
            builder.AppendLine(lineSep);
        }

        private string GetPieceStats(int[,] stat, int pl)
        {
            return null;
        }
    }
}
