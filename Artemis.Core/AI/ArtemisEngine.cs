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
        private TranspositionTable transpositionTable = new TranspositionTable();
        private ThreadMaster threadMaster;
        public IEngineConfig Config;
        public const int INITIAL_ALPHA = -PositionEvaluator.CHECKMATE_SCORE * 2;
        public const int INITIAL_BETA = -INITIAL_ALPHA;
        public const int MAX_DEPTH = 20;

        public ArtemisEngine(GameState gameState, IEngineConfig config)
        {
            this.gameState = gameState;
            Config = config;
            threadMaster = new ThreadMaster(gameState, transpositionTable, config);
        }

        public async Task<Move> Calculate(CancellationToken ct)
        {
            PVList pv;
            using (internalCts = new CancellationTokenSource())
            using (linkedCts = CancellationTokenSource.CreateLinkedTokenSource(internalCts.Token, ct))
            {
                Task<PVList> searchTask = threadMaster.Search(linkedCts.Token);

                if (!Config.ConstantDepth)
                {
                    Task timeoutTask = Task.Delay(Config.TimeLimit, linkedCts.Token);
                    try
                    {
                        await timeoutTask;
                        internalCts.Cancel();
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }

                pv = await searchTask;
            }

            transpositionTable.Clear();
            if (pv.First != null)
            {
                pv.First.Move.SetGameState(gameState);
                return pv.First.Move;
            }
            else
            {
                throw new Exception("Principal Variation is empty");
            }
        }
    }
}
