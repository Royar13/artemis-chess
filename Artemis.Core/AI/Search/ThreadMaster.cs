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
        private int threadsNum;
        private SearchThread[] searchThreads;
        private ConcurrentDictionary<ulong, bool> searchedNodes = new ConcurrentDictionary<ulong, bool>();

        public ThreadMaster(ArtemisEngine engine, GameState gameState, TranspositionTable transpositionTable, IEngineConfig config)
        {
            this.gameState = gameState;
            this.transpositionTable = transpositionTable;
            this.config = config;
            threadsNum = Environment.ProcessorCount;
            searchThreads = new SearchThread[threadsNum];
            for (int t = 0; t < threadsNum; t++)
            {
                GameState threadState = new GameState(gameState.PregeneratedAttacks, gameState.ZobristHashUtils);
                searchThreads[t] = new SearchThread(engine, this, threadState, transpositionTable, searchedNodes, config);
            }
        }

        public async Task<PVList> Search(CancellationToken ct)
        {
            List<Task<PVList>> tasks = new List<Task<PVList>>();
            int currentThreadsNum = config.Multithreading ? threadsNum : 1;
            for (int t = 0; t < currentThreadsNum; t++)
            {
                SearchThread thread = searchThreads[t];
                thread.LoadState(gameState);
                Task<PVList> engineTask = Task.Run(() => thread.Search(1, ct));
                tasks.Add(engineTask);
            }

            PVList[] foundPV = await Task.WhenAll(tasks);
            foundPV = foundPV.Where(p => p != null && p.First != null).ToArray();

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
