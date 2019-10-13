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
        PositionEvaluatorWithStats evaluator;

        [TestInitialize]
        public void TestInit()
        {
            gameState = GameStateBuilder.Build();
            evConfig = new EvaluationConfig();
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
