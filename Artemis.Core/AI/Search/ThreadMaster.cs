using Artemis.Core.AI.Transposition;
using Artemis.Core.FormatConverters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Artemis.Core.AI.Search
{
    public class ThreadMaster
    {
        private GameState gameState;
        private TranspositionTable transpositionTable;
        private IEngineConfig config;
        private FENConverter fenConverter = new FENConverter();
        private const int THREADS_NUM = 4;
        private SearchThread[] searchThreads = new SearchThread[THREADS_NUM];
        private int currentDepth;
        private int currentDepthThreads;
        private static readonly object pvLock = new object();
        private int pvDepth;
        private PVList foundPV;

        public ThreadMaster(GameState gameState, TranspositionTable transpositionTable, IEngineConfig config)
        {
            this.gameState = gameState;
            this.transpositionTable = transpositionTable;
            this.config = config;
            for (int t = 0; t < THREADS_NUM; t++)
            {
                searchThreads[t] = new SearchThread(this, transpositionTable, config);
            }
        }

        public async Task<PVList> Search(CancellationToken ct)
        {
            pvDepth = 0;
            currentDepth = 2;
            currentDepthThreads = THREADS_NUM / 2;
            int threadDepth = 1;
            List<Task> tasks = new List<Task>();
            string fen = fenConverter.Convert(gameState);
            for (int t = 0; t < THREADS_NUM; t++)
            {
                if (t >= THREADS_NUM / 2)
                {
                    threadDepth = 2;
                }
                searchThreads[t].LoadPosition(fen);
                Task engineTask = Task.Run(() => searchThreads[t].SearchRecursive(threadDepth, ct));
                tasks.Add(engineTask);
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
            }

            Console.WriteLine($"Depth: {pvDepth}, Line: {foundPV}");
            return foundPV;
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
    }
}
