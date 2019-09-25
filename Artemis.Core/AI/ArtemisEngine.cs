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
        PVSearch pvSearch;
        QuiescenceSearch quietSearch;
        TranspositionTable transpositionTable = new TranspositionTable();
        KillerMoves killerMoves = new KillerMoves();
        PositionEvaluator evaluator;
        MoveEvaluator moveEvaluator;

        public ArtemisEngine(GameState gameState)
        {
            this.gameState = gameState;
            EvaluationConfig config = new EvaluationConfig();
            evaluator = new PositionEvaluator(gameState, config);
            moveEvaluator = new MoveEvaluator(config);
            quietSearch = new QuiescenceSearch(gameState, evaluator, moveEvaluator);
            pvSearch = new PVSearch(gameState, transpositionTable, killerMoves, evaluator, moveEvaluator, quietSearch);
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
                throw new Exception("Principal Variation is empty");
            }
        }
    }
}
