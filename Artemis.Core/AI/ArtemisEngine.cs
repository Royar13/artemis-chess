using Artemis.Core.AI.Evaluation;
using Artemis.Core.AI.Search;
using Artemis.Core.AI.Transposition;
using Artemis.Core.Moves;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Artemis.Core.AI
{
    public class ArtemisEngine
    {
        public const int INITIAL_ALPHA = -PositionEvaluator.CHECKMATE_SCORE * 2;
        public const int INITIAL_BETA = -INITIAL_ALPHA;
        GameState gameState;
        IMoveSearch pvSearch;
        TranspositionTable transpositionTable = new TranspositionTable();
        PositionEvaluator evaluator;

        public ArtemisEngine(GameState gameState)
        {
            this.gameState = gameState;
            evaluator = new PositionEvaluator(gameState);
            pvSearch = new PVSearch(gameState, transpositionTable, evaluator);
        }

        public async Task<Move> Calculate()
        {
            //Iterative Deepening Search
            var task = Task.Run(() =>
            {
                for (int i = 1; i <= 5; i++)
                {
                    pvSearch.Calculate(i);
                }
            });
            await task;

            TranspositionNode node;
            if (transpositionTable.TryGetValue(gameState.GetIrrevState().ZobristHash, out node))
            {
                transpositionTable.Clear();
                return node.BestMove;
            }
            else
            {
                throw new Exception("Failed to find the searched position in the transposition table");
            }
        }
    }
}
