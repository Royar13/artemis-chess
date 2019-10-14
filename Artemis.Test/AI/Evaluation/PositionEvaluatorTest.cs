using System;
using Artemis.Core;
using Artemis.Core.AI;
using Artemis.Core.AI.Evaluation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Artemis.Test.AI.Evaluation
{
    [TestClass]
    public class PositionEvaluatorTest
    {
        GameState gameState;
        EvaluationConfig evConfig;
        PrivateObject evConfigPO;
        PositionEvaluatorWithStats evaluator;

        [TestInitialize]
        public void TestInit()
        {
            gameState = GameStateBuilder.Build();
            evConfig = new EvaluationConfig();
            evConfigPO = new PrivateObject(evConfig);
            evaluator = new PositionEvaluatorWithStats(gameState, evConfig);
        }

        [TestMethod]
        public void EvaluationTest()
        {
            EvaluationStats stats1 = new EvaluationStats();
            gameState.LoadPosition("8/6p1/8/8/6k1/8/6K1/8 w - - 6 4");
            int eval1 = evaluator.Evaluate(0, GameStage.Endgame, out stats1);
            Console.WriteLine(stats1);
        }

        [TestMethod]
        public void RookRankTest()
        {
            int[] rookRankScores = evConfigPO.GetField("rookRank") as int[];

            EvaluationStats stats = new EvaluationStats();
            gameState.LoadPosition("1rbqkbn1/p1p1ppp1/3p3r/Pp5p/Rn1PP2P/3B1N2/1PP2PPR/1NBQK3 b - - 1 9");
            evaluator.Evaluate(0, GameStage.Middlegame, out stats);
            Assert.AreEqual(0, stats.RookRankScore[0]);
            Assert.AreEqual(0, stats.RookRankScore[1]);

            stats = new EvaluationStats();
            gameState.LoadPosition("2bqkbn1/p3ppp1/2pp3r/P6p/p2PP2P/1r1P1N2/1PQ2PPR/1NB1K3 w - - 0 13");
            evaluator.Evaluate(0, GameStage.Endgame, out stats);
            Assert.AreEqual(0, stats.RookRankScore[0]);
            Assert.AreEqual(rookRankScores[5], stats.RookRankScore[1]);

            stats = new EvaluationStats();
            gameState.LoadPosition("2bqkbn1/p3ppp1/2pp4/P2P3p/p3P2P/1r1P1N2/1PQB1PrR/1N2K3 w - - 0 15");
            evaluator.Evaluate(0, GameStage.Middlegame, out stats);
            Assert.AreEqual(0, stats.RookRankScore[0]);
            Assert.AreEqual(rookRankScores[5] + rookRankScores[6], stats.RookRankScore[1]);

            stats = new EvaluationStats();
            gameState.LoadPosition("2bqkbR1/p3pp2/3p3n/P2p3p/p3P2P/1r1P1N2/1PQB1P2/1N2K3 b - - 2 17");
            evaluator.Evaluate(0, GameStage.Endgame, out stats);
            Assert.AreEqual(rookRankScores[7], stats.RookRankScore[0]);
            Assert.AreEqual(rookRankScores[5], stats.RookRankScore[1]);
        }

        [TestMethod]
        public void EvaluateKPKEndgame()
        {
            EvaluationStats stats = new EvaluationStats();
            gameState.LoadPosition("8/8/8/5kp1/8/6K1/8/8 w - - 0 4");
            evaluator.Evaluate(0, GameStage.Endgame, out stats);
            Assert.AreEqual(0, stats.EndgameKPK[1]);

            gameState.LoadPosition("8/8/8/6p1/7k/8/6K1/8 w - - 0 4");
            evaluator.Evaluate(0, GameStage.Endgame, out stats);
            Assert.AreEqual(0, stats.EndgameKPK[1]);

            gameState.LoadPosition("8/6p1/8/8/6k1/8/6K1/8 w - - 6 4");
            evaluator.Evaluate(0, GameStage.Endgame, out stats);
            Assert.AreNotEqual(0, stats.EndgameKPK[1]);
        }
    }
}
