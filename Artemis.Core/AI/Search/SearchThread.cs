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
        ThreadMaster master;
        GameState gameState;
        PVSearch pvSearch;
        QuiescenceSearch quietSearch;
        KillerMoves killerMoves = new KillerMoves();
        PositionEvaluator evaluator;
        MoveEvaluator moveEvaluator;
        IEngineConfig config;
        PVList pv;

        public SearchThread(ThreadMaster master, TranspositionTable transpositionTable, IEngineConfig config)
        {
            this.master = master;
            gameState = new GameState();
            EvaluationConfig evConfig = new EvaluationConfig();
            evaluator = new PositionEvaluator(gameState, evConfig);
            moveEvaluator = new MoveEvaluator(evConfig);
            quietSearch = new QuiescenceSearch(gameState, evaluator, moveEvaluator);
            pvSearch = new PVSearch(gameState, transpositionTable, killerMoves, evaluator, moveEvaluator, quietSearch);
            this.config = config;
        }

        public void LoadPosition(string fen)
        {
            gameState.LoadFEN(fen);
        }

        public void SearchRecursive(int depth, CancellationToken ct)
        {
            pv = pvSearch.Calculate(depth, pv, ct);
            bool con = !ct.IsCancellationRequested && (!config.ConstantDepth || depth <= config.Depth);
            int nextDepth = 0;
            if (!ct.IsCancellationRequested)
            {
                nextDepth = master.UpdatePV(pv, depth);
            }

            if (con)
            {
                SearchRecursive(nextDepth, ct);
            }
            else
            {
                pv = null;
                killerMoves.Clear();
            }

            ct.ThrowIfCancellationRequested();
        }
    }
}
