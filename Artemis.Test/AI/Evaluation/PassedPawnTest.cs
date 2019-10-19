using System;
using Artemis.Core.AI;
using Artemis.Core.AI.Evaluation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Artemis.Test.AI.Evaluation
{
    [TestClass]
    public class PassedPawnTest : PositionEvaluatorTestBase
    {
        int[] passedPawnRank;
        double passedPawnDefenderModifier;

        [TestInitialize]
        public override void TestInit()
        {
            base.TestInit();

            passedPawnRank = evConfigPO.GetField("passedPawnRank") as int[];
            passedPawnDefenderModifier = (double)evConfigPO.GetField("passedPawnDefenderModifier");
        }

        [TestMethod]
        public void NoPassedPawnTest()
        {
            EvaluationStats stats = new EvaluationStats();
            gameState.LoadPosition("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            evaluator.Evaluate(0, GameStage.Middlegame, 0, out stats);
            Assert.AreEqual(0, stats.PassedPawnScore[0]);
            Assert.AreEqual(0, stats.PassedPawnScore[1]);

            stats = new EvaluationStats();
            gameState.LoadPosition("rnbq1bnr/p3kBp1/8/1p2N3/Pp2p1pP/8/2PP1P2/RNBQK2R w KQ - 1 10");
            evaluator.Evaluate(0, GameStage.Endgame, 0, out stats);
            Assert.AreEqual(0, stats.PassedPawnScore[0]);
            Assert.AreEqual(0, stats.PassedPawnScore[1]);
        }

        [TestMethod]
        public void PassedPawnTest_EdgePawns()
        {
            EvaluationStats stats = new EvaluationStats();
            gameState.LoadPosition("r2q1bnr/Qb1nk3/6B1/4N2P/Pp2p1p1/8/2PP1P2/RNB1K2R b KQ - 0 15");
            evaluator.Evaluate(0, GameStage.Opening, 0, out stats);
            Assert.AreEqual(passedPawnRank[3] + passedPawnRank[4], stats.PassedPawnScore[0]);
            Assert.AreEqual(0, stats.PassedPawnScore[1]);

            stats = new EvaluationStats();
            gameState.LoadPosition("rnbqkbnr/p4pp1/8/4N2p/1pB1p1PP/p2P4/2P2P2/RNBQK2R w KQkq - 0 10");
            evaluator.Evaluate(0, GameStage.Endgame, 0, out stats);
            Assert.AreEqual(0, stats.PassedPawnScore[0]);
            Assert.AreEqual((int)(passedPawnRank[5] * passedPawnDefenderModifier), stats.PassedPawnScore[1]);
        }

        [TestMethod]
        public void PassedPawnTest_Regular()
        {
            EvaluationStats stats = new EvaluationStats();
            gameState.LoadPosition("rnb1kbnr/p4p1p/2P5/3PN1p1/Pp6/6P1/4p2P/RNBQK2R b KQkq - 0 15");
            evaluator.Evaluate(0, GameStage.Middlegame, 0, out stats);
            Assert.AreEqual((int)(passedPawnRank[5] * passedPawnDefenderModifier) + passedPawnRank[4], stats.PassedPawnScore[0]);
            Assert.AreEqual(passedPawnRank[6] + passedPawnRank[4], stats.PassedPawnScore[1]);

            stats = new EvaluationStats();
            gameState.LoadPosition("rn2k1nr/p3b1pp/2P2p2/1P1PN3/8/1Q4P1/7P/RNB1K2R b KQkq - 0 19");
            evaluator.Evaluate(0, GameStage.Middlegame, 0, out stats);
            Assert.AreEqual((int)(passedPawnRank[5] * passedPawnDefenderModifier * passedPawnDefenderModifier) + passedPawnRank[4],
                stats.PassedPawnScore[0]);
            Assert.AreEqual(0, stats.PassedPawnScore[1]);
        }
    }
}
