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
        PositionEvaluator evaluator;

        [TestInitialize]
        public void TestInit()
        {
            gameState = GameStateBuilder.Build();
            evConfig = new EvaluationConfig();
            evaluator = new PositionEvaluator(gameState, evConfig);
        }

        [TestMethod]
        public void TestMethod1()
        {
            gameState.LoadPosition("2kr3r/1ppq1pp1/2npbn1p/p1b1p3/2B1P3/P1PP1N1P/1P1N1PP1/R1BQR1K1 w - - 1 11");
            int eval1 = evaluator.Evaluate(0, GameStage.Middlegame);

            gameState.LoadPosition("r4rk1/1ppq1pp1/2npbn1p/p1b1p3/2B1P3/P1PP1N1P/1P1N1PP1/R1BQR1K1 w - - 1 11");
            int eval2 = evaluator.Evaluate(0, GameStage.Middlegame);
        }
    }
}
