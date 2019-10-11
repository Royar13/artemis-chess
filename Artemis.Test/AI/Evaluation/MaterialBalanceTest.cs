using System;
using Artemis.Core;
using Artemis.Core.AI.Evaluation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Artemis.Test.AI.Evaluation
{
    [TestClass]
    public class MaterialBalanceTest
    {
        GameState gameState;
        PositionEvaluator evaluator;
        PrivateObject evaluatorPO;

        [TestInitialize]
        public void TestInit()
        {
            gameState = GameStateBuilder.Build();
            evaluator = new PositionEvaluator(gameState, new EvaluationConfig());
            evaluatorPO = new PrivateObject(evaluator);
        }

        private int GetMaterialBalance()
        {
            return (int)evaluatorPO.Invoke("GetMaterialBalance");
        }

        [TestMethod]
        public void MaterialBalance()
        {
            gameState.LoadPosition("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            Assert.AreEqual(0, GetMaterialBalance());

            gameState.LoadPosition("r1b1kb1r/p3pppp/5n2/3N4/6P1/4BP2/P1PKP2P/q4B1R b kq - 0 12");
            Assert.AreEqual(1300, GetMaterialBalance());

            gameState.LoadPosition("r1b1kb1r/p3pppp/5n2/3N4/6P1/4BP2/P1PKP2P/q4B1R w kq - 0 12");
            Assert.AreEqual(-1300, GetMaterialBalance());
        }
    }
}
