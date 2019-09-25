using Artemis.Core.AI.Evaluation;
using Artemis.Core.AI.Search;
using Artemis.Core.AI.Search.Heuristics;
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
        public const int MAX_DEPTH = 10;
        GameState gameState;
        IMoveSearch pvSearch;
        TranspositionTable transpositionTable = new TranspositionTable();
        PositionEvaluator evaluator;
        KillerMoves killerMoves = new KillerMoves();

        public ArtemisEngine(GameState gameState)
        {
            this.gameState = gameState;
            evaluator = new PositionEvaluator(gameState, new EvaluationConfig());
            pvSearch = new PVSearch(gameState, transpositionTable, evaluator, killerMoves);
        }

        public async Task<Move> Calculate()
        {
            PVList pv = null;
            //Iterative Deepening Search
            var task = Task.Run(() =>
            {
                for (int i = 1; i <= 5; i++)
                {
                    pv = pvSearch.Calculate(i, pv);
                    pv.Print();
                }
            });
            await task;

            transpositionTable.Clear();
            killerMoves.Clear();
            if (pv.First != null)
            {
                return pv.First.Move;
            }
            else
            {
                throw new Exception("Failed to find the searched position in the transposition table");
            }
        }
    }
}
