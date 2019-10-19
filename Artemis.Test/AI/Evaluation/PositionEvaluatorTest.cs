using System;
using Artemis.Core;
using Artemis.Core.AI;
using Artemis.Core.AI.Evaluation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Artemis.Test.AI.Evaluation
{
    [TestClass]
    public class PositionEvaluatorTest : PositionEvaluatorTestBase
    {
        [TestMethod]
        public void EvaluationTest()
        {
            EvaluationStats stats1 = new EvaluationStats();
            gameState.LoadPosition("r1bqk2r/ppp2ppp/2n1p1n1/8/2PP4/2P1BN1P/P3BPP1/R2Q1RK1 b kq - 0 12");
            int eval1 = evaluator.Evaluate(0, GameStage.Middlegame, 0, out stats1);
            Console.WriteLine(stats1);
        }

        [TestMethod]
        public void RookRankTest()
        {
            int[] rookRankScores = evConfigPO.GetField("rookRank") as int[];

            EvaluationStats stats = new EvaluationStats();
            gameState.LoadPosition("1rbqkbn1/p1p1ppp1/3p3r/Pp5p/Rn1PP2P/3B1N2/1PP2PPR/1NBQK3 b - - 1 9");
            evaluator.Evaluate(0, GameStage.Middlegame, 0, out stats);
            Assert.AreEqual(0, stats.RookRankScore[0]);
            Assert.AreEqual(0, stats.RookRankScore[1]);

            stats = new EvaluationStats();
            gameState.LoadPosition("2bqkbn1/p3ppp1/2pp3r/P6p/p2PP2P/1r1P1N2/1PQ2PPR/1NB1K3 w - - 0 13");
            evaluator.Evaluate(0, GameStage.Endgame, 0, out stats);
            Assert.AreEqual(0, stats.RookRankScore[0]);
            Assert.AreEqual(rookRankScores[5], stats.RookRankScore[1]);

            stats = new EvaluationStats();
            gameState.LoadPosition("2bqkbn1/p3ppp1/2pp4/P2P3p/p3P2P/1r1P1N2/1PQB1PrR/1N2K3 w - - 0 15");
            evaluator.Evaluate(0, GameStage.Middlegame, 0, out stats);
            Assert.AreEqual(0, stats.RookRankScore[0]);
            Assert.AreEqual(rookRankScores[5] + rookRankScores[6], stats.RookRankScore[1]);

            stats = new EvaluationStats();
            gameState.LoadPosition("2bqkbR1/p3pp2/3p3n/P2p3p/p3P2P/1r1P1N2/1PQB1P2/1N2K3 b - - 2 17");
            evaluator.Evaluate(0, GameStage.Endgame, 0, out stats);
            Assert.AreEqual(rookRankScores[7], stats.RookRankScore[0]);
            Assert.AreEqual(rookRankScores[5], stats.RookRankScore[1]);

            stats = new EvaluationStats();
            gameState.LoadPosition("2bqkbR1/p3pp2/3p3n/P2p3p/p3P2P/1r1P1N2/1PQB1P2/1N2K3 b - - 2 17");
            evaluator.Evaluate(0, GameStage.Opening, 0, out stats);
            Assert.AreEqual(0, stats.RookRankScore[0]);
            Assert.AreEqual(0, stats.RookRankScore[1]);
        }

        [TestMethod]
        public void RookOpenFileTest()
        {
            int rookOpenFileScore = (int)evConfigPO.GetField("rookOpenFile");

            EvaluationStats stats = new EvaluationStats();
            gameState.LoadPosition("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            evaluator.Evaluate(0, GameStage.Opening, 0, out stats);
            Assert.AreEqual(0, stats.RookOpenFileScore[0]);
            Assert.AreEqual(0, stats.RookOpenFileScore[1]);

            stats = new EvaluationStats();
            gameState.LoadPosition("1nbqkbn1/3pppp1/1p4r1/p1P3r1/2P3pP/P3PN2/2RP1P2/1NBQKB1R w K - 5 14");
            evaluator.Evaluate(0, GameStage.Endgame, 0, out stats);
            Assert.AreEqual(0, stats.RookOpenFileScore[0]);
            Assert.AreEqual(0, stats.RookOpenFileScore[1]);

            stats = new EvaluationStats();
            gameState.LoadPosition("1nbqkbn1/2pppppr/1p1r4/p7/2P3pP/PP3N2/2RPPP2/1NBQKB1R b K - 1 8");
            evaluator.Evaluate(0, GameStage.Middlegame, 0, out stats);
            Assert.AreEqual(0, stats.RookOpenFileScore[0]);
            Assert.AreEqual(rookOpenFileScore * 2, stats.RookOpenFileScore[1]);

            stats = new EvaluationStats();
            gameState.LoadPosition("2bqkbR1/p3pp2/3p3n/P2p3p/p3P2P/1r1P1N2/1PQB1P2/1N2K3 b - - 2 17");
            evaluator.Evaluate(0, GameStage.Middlegame, 0, out stats);
            Assert.AreEqual(rookOpenFileScore, stats.RookOpenFileScore[0]);
            Assert.AreEqual(rookOpenFileScore, stats.RookOpenFileScore[1]);

            stats = new EvaluationStats();
            gameState.LoadPosition("2bqkb2/p4p2/3pp3/P2p1R1p/p3P2P/3P1N2/1Q1B1P2/1N2K3 b - - 0 20");
            evaluator.Evaluate(0, GameStage.Middlegame, 0, out stats);
            Assert.AreEqual(rookOpenFileScore, stats.RookOpenFileScore[0]);
            Assert.AreEqual(0, stats.RookOpenFileScore[1]);

            stats = new EvaluationStats();
            gameState.LoadPosition("2bqkb1r/p1r1p3/2n2n1R/1p1p2pR/P6P/2QP1N2/1PP1BPP1/1NB1K3 w k - 1 18");
            evaluator.Evaluate(0, GameStage.Opening, 0, out stats);
            Assert.AreEqual(rookOpenFileScore * 2, stats.RookOpenFileScore[0]);
            Assert.AreEqual(rookOpenFileScore * 2, stats.RookOpenFileScore[1]);
        }

        [TestMethod]
        public void EvaluateKPKEndgame()
        {
            EvaluationStats stats = new EvaluationStats();
            gameState.LoadPosition("8/8/8/5kp1/8/6K1/8/8 w - - 0 4");
            evaluator.Evaluate(0, GameStage.Endgame, 0, out stats);
            Assert.AreEqual(0, stats.EndgameKPK[1]);

            gameState.LoadPosition("8/8/8/6p1/7k/8/6K1/8 w - - 0 4");
            evaluator.Evaluate(0, GameStage.Endgame, 0, out stats);
            Assert.AreEqual(0, stats.EndgameKPK[1]);

            gameState.LoadPosition("8/6p1/8/8/6k1/8/6K1/8 w - - 6 4");
            evaluator.Evaluate(0, GameStage.Endgame, 0, out stats);
            Assert.AreNotEqual(0, stats.EndgameKPK[1]);
        }
    }
}
