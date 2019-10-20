using System;
using Artemis.Core.AI;
using Artemis.Core.AI.Evaluation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Artemis.Test.AI.Evaluation
{
    [TestClass]
    public class KnightSquareTest : PositionEvaluatorTestBase
    {
        private double knightPawnProtectorModifier;
        private double knightOutpostModifier;
        private int[] knightSquare;

        [TestInitialize]
        public override void TestInit()
        {
            base.TestInit();

            knightPawnProtectorModifier = (double)evConfigPO.GetField("knightPawnProtectorModifier");
            knightOutpostModifier = (double)evConfigPO.GetField("knightOutpostModifier");
            knightSquare = (int[])evConfigPO.GetField("knightSquare");
        }

        [TestMethod]
        public void NoKnight()
        {
            EvaluationStats stats = new EvaluationStats();
            gameState.LoadPosition("r1bq1b1r/pp6/4pkp1/P1pp1pp1/3P4/2PB2P1/P2QKP1P/R1B4R b - - 0 17");
            evaluator.Evaluate(0, GameStage.Opening, 0, out stats);
            Assert.AreEqual(0, stats.KnightSquareScore[0]);
            Assert.AreEqual(0, stats.KnightSquareScore[1]);
        }

        [TestMethod]
        public void UndefendedKnightTest()
        {
            EvaluationStats stats = new EvaluationStats();
            gameState.LoadPosition("r1bqkb1r/pp1n1pp1/4p2p/n1ppP1N1/3P4/2PB4/PP2NPPP/R1BQK2R b KQkq - 1 9");
            evaluator.Evaluate(0, GameStage.Middlegame, 0, out stats);
            Assert.AreEqual(knightSquare[12] + knightSquare[38], stats.KnightSquareScore[0]);
            Assert.AreEqual(knightSquare[24] + knightSquare[11], stats.KnightSquareScore[1]);
        }

        [TestMethod]
        public void UndefendedKnightOutpostTest()
        {
            EvaluationStats stats = new EvaluationStats();
            gameState.LoadPosition("r1bq1b1r/pp3k2/4pNp1/P1pp1pp1/3P4/2PB2P1/P2nKP1P/R1BQ3R b - - 0 16");
            evaluator.Evaluate(0, GameStage.Endgame, 0, out stats);
            Assert.AreEqual((int)(knightSquare[45] * knightOutpostModifier), stats.KnightSquareScore[0]);
            Assert.AreEqual((int)(knightSquare[51] * knightOutpostModifier), stats.KnightSquareScore[1]);
        }

        [TestMethod]
        public void DefendedKnightTest()
        {
            EvaluationStats stats = new EvaluationStats();
            gameState.LoadPosition("r1bqkb1r/2p1pppp/8/pNpp4/2PPn3/5N2/PP2PPPP/R1B1KB1R w KQkq - 0 8");
            evaluator.Evaluate(0, GameStage.Middlegame, 0, out stats);
            Assert.AreEqual((int)(knightSquare[21] * knightPawnProtectorModifier * knightPawnProtectorModifier +
                knightSquare[33] * knightPawnProtectorModifier), stats.KnightSquareScore[0]);
            Assert.AreEqual((int)(knightSquare[36] * knightPawnProtectorModifier), stats.KnightSquareScore[1]);

            stats = new EvaluationStats();
            gameState.LoadPosition("1rbqkb1r/4p1pp/3p1p2/p1ppN3/2PP4/7P/PP1KPPP1/R4B1R b k - 0 12");
            evaluator.Evaluate(0, GameStage.Middlegame, 0, out stats);
            Assert.AreEqual((int)(knightSquare[36] * knightPawnProtectorModifier), stats.KnightSquareScore[0]);
            Assert.AreEqual(0, stats.KnightSquareScore[1]);
        }

        [TestMethod]
        public void DefendedKnightOutpostTest()
        {
            EvaluationStats stats = new EvaluationStats();
            gameState.LoadPosition("r1b1kb1r/4p3/2q3p1/pNpp1p1p/2PPnP1P/4P3/PP4P1/R1B1KB1R w KQkq - 0 15");
            evaluator.Evaluate(0, GameStage.Endgame, 0, out stats);
            Assert.AreEqual((int)(knightSquare[33] * knightPawnProtectorModifier * knightOutpostModifier), stats.KnightSquareScore[0]);
            Assert.AreEqual((int)(knightSquare[36] * knightPawnProtectorModifier * knightPawnProtectorModifier * knightOutpostModifier), stats.KnightSquareScore[1]);
        }
    }
}
