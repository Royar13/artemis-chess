using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.AI.Evaluation
{
    public class PositionEvaluator
    {
        GameState gameState;
        List<IEvaluator> evaluators = new List<IEvaluator>();
        public const int CHECKMATE_SCORE = 1000000;

        public PositionEvaluator(GameState gameState)
        {
            this.gameState = gameState;
            evaluators.Add(new MaterialBalance(gameState));
        }

        public int Evaluate(int depth)
        {
            GameResult result = gameState.GetResult();
            if (result != GameResult.Ongoing)
            {
                if (result == GameResult.Checkmate)
                {
                    return -CHECKMATE_SCORE - depth;
                }
                else
                {
                    return 0;
                }
            }

            int score = 0;
            foreach (IEvaluator evaluator in evaluators)
            {
                score += evaluator.GetScore();
            }
            return score;
        }
    }
}
