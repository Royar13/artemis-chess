using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.AI.Evaluation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Artemis.Test.AI.Evaluation
{
    [TestClass]
    public abstract class PositionEvaluatorTestBase
    {
        protected GameState gameState;
        protected EvaluationConfig evConfig;
        protected PrivateObject evConfigPO;
        protected PositionEvaluatorWithStats evaluator;

        [TestInitialize]
        public virtual void TestInit()
        {
            gameState = GameStateBuilder.Build();
            evConfig = new EvaluationConfig();
            evConfigPO = new PrivateObject(evConfig);
            evaluator = new PositionEvaluatorWithStats(gameState, evConfig);
        }
    }
}
