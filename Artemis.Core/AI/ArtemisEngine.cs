using Artemis.Core.AI.Evaluation;
using Artemis.Core.AI.Search;
using Artemis.Core.AI.Search.Heuristics;
using Artemis.Core.AI.Transposition;
using Artemis.Core.Moves;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Artemis.Core.AI
{
    public class ArtemisEngine
    {
        GameState gameState;
        PVSearch pvSearch;
        QuiescenceSearch quietSearch;
        TranspositionTable transpositionTable = new TranspositionTable();
        KillerMoves killerMoves = new KillerMoves();
        PositionEvaluator evaluator;
        MoveEvaluator moveEvaluator;
        CancellationTokenSource internalCts;
        CancellationTokenSource linkedCts;
        public IEngineConfig Config;
        public const int INITIAL_ALPHA = -PositionEvaluator.CHECKMATE_SCORE * 2;
        public const int INITIAL_BETA = -INITIAL_ALPHA;
        public const int MAX_DEPTH = 20;

        public ArtemisEngine(GameState gameState, IEngineConfig config)
        {
            this.gameState = gameState;
            Config = config;
            EvaluationConfig evConfig = new EvaluationConfig();
            evaluator = new PositionEvaluator(gameState, evConfig);
            moveEvaluator = new MoveEvaluator(evConfig);
            quietSearch = new QuiescenceSearch(gameState, evaluator, moveEvaluator);
            pvSearch = new PVSearch(gameState, transpositionTable, killerMoves, evaluator, moveEvaluator, quietSearch);
        }

        public async Task<Move> Calculate(CancellationToken ct)
        {
            using (internalCts = new CancellationTokenSource())
            using (linkedCts = CancellationTokenSource.CreateLinkedTokenSource(internalCts.Token, ct))
            {
                PVList pv = null;
                //Iterative Deepening Search
                Task engineTask = Task.Run(() =>
                {
                    bool con = true;
                    int depth = 1;
                    int reachedDepth = 0;
                    do
                    {
                        PVList newPV = pvSearch.Calculate(depth, pv, linkedCts.Token);
                        if (!linkedCts.IsCancellationRequested)
                        {
                            pv = newPV;
                            reachedDepth = depth;
                        }
                        con = !linkedCts.IsCancellationRequested && (!Config.ConstantDepth || depth <= Config.Depth);
                        depth++;
                    } while (con);
                    Console.WriteLine($"Depth: {reachedDepth}, Line: {pv}");
                    linkedCts.Token.ThrowIfCancellationRequested();
                });
                List<Task> tasks = new List<Task> { engineTask };
                Task timeoutTask = null;
                if (!Config.ConstantDepth)
                {
                    timeoutTask = Task.Delay(Config.TimeLimit, linkedCts.Token);
                    tasks.Add(timeoutTask);
                }

                try
                {
                    await Task.WhenAny(tasks);

                    if (!Config.ConstantDepth)
                    {
                        internalCts.Cancel();
                        await engineTask;
                    }
                }
                catch (OperationCanceledException)
                {
                }

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
}
