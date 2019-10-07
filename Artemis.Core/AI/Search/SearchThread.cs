using Artemis.Core.AI.Evaluation;
using Artemis.Core.AI.Search.Heuristics;
using Artemis.Core.AI.Transposition;
using System;
using System.Collections.Concurrent;
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

        public SearchThread(ThreadMaster master, TranspositionTable transpositionTable, ConcurrentDictionary<ulong, bool> searchedNodes, IEngineConfig config, ZobristHashUtils zobristHashUtils)
        {
            this.master = master;
            gameState = new GameState(zobristHashUtils);
            EvaluationConfig evConfig = new EvaluationConfig();
            evaluator = new PositionEvaluator(gameState, evConfig);
            moveEvaluator = new MoveEvaluator(evConfig);
            quietSearch = new QuiescenceSearch(gameState, evaluator, moveEvaluator);
            pvSearch = new PVSearch(gameState, transpositionTable, killerMoves, evaluator, moveEvaluator, quietSearch, searchedNodes);
            this.config = config;
        }

        public void LoadPosition(string fen)
        {
            gameState.LoadFEN(fen);
        }

        public PVList Search(int startDepth, CancellationToken ct)
        {
            PVList pv = null;
            bool con;
            int depth = startDepth;
            do
            {
                PVList newPV = pvSearch.Calculate(depth, pv, ct);
                if (!ct.IsCancellationRequested)
                {
                    pv = newPV;
                }
                depth++;
                con = !ct.IsCancellationRequested && (!config.ConstantDepth || depth <= config.Depth);
            } while (con);

            killerMoves.Clear();
            return pv;
        }
    }
}
