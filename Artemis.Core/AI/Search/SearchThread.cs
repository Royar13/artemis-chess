using Artemis.Core.AI.Evaluation;
using Artemis.Core.AI.Search.Heuristics;
using Artemis.Core.AI.Transposition;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Artemis.Core.AI.Search
{
    public class SearchThread
    {
        GameState gameState;
        PVSearch pvSearch;
        QuiescenceSearch quietSearch;
        KillerMoves killerMoves = new KillerMoves();
        PositionEvaluator evaluator;
        MoveEvaluator moveEvaluator;

        public SearchThread(GameState gameState, ArtemisEngine engine)
        {
            this.gameState = gameState;
            EvaluationConfig evConfig = new EvaluationConfig();
            evaluator = new PositionEvaluator(gameState, evConfig);
            moveEvaluator = new MoveEvaluator(evConfig);
            quietSearch = new QuiescenceSearch(gameState, evaluator, moveEvaluator);
            pvSearch = new PVSearch(gameState, transpositionTable, killerMoves, evaluator, moveEvaluator, quietSearch);
        }

        public void SearchRecursive(int depth)
        {

        }

        public PVList Calculate(int depth, PVList prevPV, CancellationToken ct)
        {
            return pvSearch.Calculate(depth, prevPV, ct);
        }
    }
}
