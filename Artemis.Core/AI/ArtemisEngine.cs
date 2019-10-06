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
        CancellationTokenSource internalCts;
        CancellationTokenSource linkedCts;
        private int currentDepth;
        private int currentDepthThreads;
        private static readonly object pvLock = new object();
        private int pvDepth;
        private PVList foundPV;
        private const int THREADS_NUM = 4;
        private SearchThread[] searchThreads = new SearchThread[THREADS_NUM];
        public IEngineConfig Config;
        public const int INITIAL_ALPHA = -PositionEvaluator.CHECKMATE_SCORE * 2;
        public const int INITIAL_BETA = -INITIAL_ALPHA;
        public const int MAX_DEPTH = 20;
        public readonly TranspositionTable transpositionTable = new TranspositionTable();

        public ArtemisEngine(GameState gameState, IEngineConfig config)
        {
            this.gameState = gameState;
            Config = config;

            for (int t = 0; t < THREADS_NUM; t++)
            {
                searchThreads[t] = new SearchThread(gameState, transpositionTable);
            }
        }

        public async Task<Move> Calculate(CancellationToken ct)
        {
            foundPV = null;
            using (internalCts = new CancellationTokenSource())
            using (linkedCts = CancellationTokenSource.CreateLinkedTokenSource(internalCts.Token, ct))
            {
                //Iterative Deepening Search
                for (int depth = 1; depth < Config.MinimalDepthMultithreading; depth++)
                {
                    foundPV = searchThreads[0].Calculate(depth, foundPV, linkedCts.Token);
                }

                List<Task> tasks = new List<Task>();
                pvDepth = Config.MinimalDepthMultithreading - 1;
                currentDepth = Config.MinimalDepthMultithreading + 1;
                currentDepthThreads = THREADS_NUM / 2;
                int threadDepth = Config.MinimalDepthMultithreading;
                for (int t = 0; t < THREADS_NUM; t++)
                {
                    if (t >= THREADS_NUM / 2)
                    {
                        threadDepth = Config.MinimalDepthMultithreading + 1;
                    }
                    Task engineTask = Task.Run(() => SearchThread(threadDepth));
                    tasks.Add(engineTask);
                }

                if (!Config.ConstantDepth)
                {
                    Task timeoutTask = Task.Delay(Config.TimeLimit, linkedCts.Token);
                    tasks.Add(timeoutTask);
                }

                try
                {
                    await Task.WhenAny(tasks);

                    if (!Config.ConstantDepth)
                    {
                        internalCts.Cancel();
                        await Task.WhenAll(tasks);
                    }
                }
                catch (OperationCanceledException)
                {
                }

                transpositionTable.Clear();
                killerMoves.Clear();
                if (foundPV.First != null)
                {
                    Console.WriteLine($"Depth: {pvDepth}, Line: {foundPV}");
                    return foundPV.First.Move;
                }
                else
                {
                    throw new Exception("Principal Variation is empty");
                }
            }
        }

        /// <summary>
        /// Updates the PV when a thread finished a search.
        /// </summary>
        /// <param name="pv"></param>
        /// <param name="depth"></param>
        /// <returns>The depth at which the thread should search next</returns>
        public int UpdatePV(PVList pv, int depth)
        {
            int searchDepth;
            lock (pvLock)
            {
                if (depth > pvDepth)
                {
                    foundPV = pv;
                    pvDepth = depth;
                }
                if (currentDepthThreads == THREADS_NUM / 2)
                {
                    currentDepth++;
                    currentDepthThreads = 1;
                }
                else
                {
                    currentDepthThreads++;
                }
                searchDepth = currentDepth;
            }
            return searchDepth;
        }

        private void SearchThread(int depth)
        {
            PVList newPV = pvSearch.Calculate(depth, foundPV, linkedCts.Token);
            bool con = !linkedCts.IsCancellationRequested && (!Config.ConstantDepth || depth <= Config.Depth);
            if (!linkedCts.IsCancellationRequested)
            {
                int nextDepth = UpdatePV(newPV, depth);
                if (con)
                {
                    SearchThread(nextDepth);
                }
            }

            linkedCts.Token.ThrowIfCancellationRequested();
        }
    }
}
