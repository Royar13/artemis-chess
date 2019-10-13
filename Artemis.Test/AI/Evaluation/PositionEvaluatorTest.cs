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
            gameState.LoadPosition("5nkb/8/8/8/3K4/8/8/8 w - - 1 11");
            int eval1 = evaluator.Evaluate(0, GameStage.Endgame, out stats1);
            Console.WriteLine(stats1);
        }
    }
}
