using System;
using Artemis.Core;
using Artemis.Core.AI.Evaluation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Artemis.Test.AI.Evaluation
{
    [TestClass]
    public class MaterialBalanceTest
    {
        [TestMethod]
        public void MaterialBalance()
        {
            GameState gameState = new GameState();
            MaterialBalance mb = new MaterialBalance(gameState);

            gameState.LoadFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            Assert.AreEqual(0, mb.GetScore());

            gameState.LoadFEN("r1b1kb1r/p3pppp/5n2/3N4/6P1/4BP2/P1PKP2P/q4B1R b kq - 0 12");
            Assert.AreEqual(1300, mb.GetScore());

            gameState.LoadFEN("r1b1kb1r/p3pppp/5n2/3N4/6P1/4BP2/P1PKP2P/q4B1R w kq - 0 12");
            Assert.AreEqual(-1300, mb.GetScore());
        }
    }
}
