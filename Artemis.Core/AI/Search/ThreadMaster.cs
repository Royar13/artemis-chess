using Artemis.Core.AI.Transposition;
using Artemis.Core.FormatConverters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        private ConcurrentDictionary<ulong, bool> searchedNodes = new ConcurrentDictionary<ulong, bool>();

        public ThreadMaster(GameState gameState, TranspositionTable transpositionTable, IEngineConfig config)
        {
            this.gameState = gameState;
            this.transpositionTable = transpositionTable;
            this.config = config;
            for (int t = 0; t < THREADS_NUM; t++)
            {
                searchThreads[t] = new SearchThread(this, transpositionTable, searchedNodes, config, gameState.ZobristHashUtils);
            }
        }

        public async Task<PVList> Search(CancellationToken ct)
        {
            List<Task<PVList>> tasks = new List<Task<PVList>>();
            string fen = fenConverter.Convert(gameState);
            for (int t = 0; t < THREADS_NUM; t++)
            {
                SearchThread thread = searchThreads[t];
                thread.LoadPosition(fen);
                Task<PVList> engineTask = Task.Run(() => thread.Search(1, ct));
                tasks.Add(engineTask);
            }

            PVList[] foundPV = await Task.WhenAll(tasks);
            foundPV = foundPV.Where(p => p != null).ToArray();

            PVList bestPV = foundPV[0];
            for (int i = 1; i < foundPV.Length; i++)
            {
                if (foundPV[i] > bestPV)
                {
                    bestPV = foundPV[i];
                }
            }

            searchedNodes.Clear();
            Console.WriteLine(bestPV);
            return bestPV;
        }
    }
}
