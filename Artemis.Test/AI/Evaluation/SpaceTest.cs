using System;
using Artemis.Core.AI;
using Artemis.Core.AI.Evaluation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Artemis.Test.AI.Evaluation
{
    [TestClass]
    public class SpaceTest : PositionEvaluatorTestBase
    {
        int spaceScore;

        [TestInitialize]
        public override void TestInit()
        {
            base.TestInit();

            spaceScore = (int)evConfigPO.GetField("space");
        }

        [TestMethod]
        public void SpaceTest_InitialPosition()
        {
            EvaluationStats stats = new EvaluationStats();
            gameState.LoadPosition("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            evaluator.Evaluate(0, GameStage.Middlegame, 0, out stats);
            Assert.AreEqual(0, stats.SpaceScore[0]);
            Assert.AreEqual(0, stats.SpaceScore[1]);
        }

        [TestMethod]
        public void SpaceTest_PawnsAdvanced()
        {
            EvaluationStats stats = new EvaluationStats();
            gameState.LoadPosition("r1bqkbnr/1pp2p2/2n1pP1p/p2p3p/8/1P1P1N2/P1P1P1P1/RNBQKB1R b KQkq - 0 8");
            evaluator.Evaluate(0, GameStage.Middlegame, 0, out stats);
            Assert.AreEqual(6 * spaceScore, stats.SpaceScore[0]);
            Assert.AreEqual(7 * spaceScore, stats.SpaceScore[1]);

            stats = new EvaluationStats();
            gameState.LoadPosition("r1b2knr/1p2bP2/2pPp2P/p7/1n6/3P3p/P3P3/RNBQKB1R w KQ - 0 19");
            evaluator.Evaluate(0, GameStage.Opening, 0, out stats);
            Assert.AreEqual(14 * spaceScore, stats.SpaceScore[0]);
            Assert.AreEqual(8 * spaceScore, stats.SpaceScore[1]);
        }
    }
}
