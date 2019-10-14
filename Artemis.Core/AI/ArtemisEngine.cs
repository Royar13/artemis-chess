using Artemis.Core.AI.Evaluation;
using Artemis.Core.AI.Opening;
using Artemis.Core.AI.Search;
using Artemis.Core.AI.Search.Heuristics;
using Artemis.Core.AI.Transposition;
using Artemis.Core.Moves;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private EvaluationConfig evConfig;
        private PositionEvaluator evaluator;
        private OpeningBook openingBook;
        private ThreadMaster threadMaster;
        public IEngineConfig Config;
        public const int INITIAL_ALPHA = -PositionEvaluator.CHECKMATE_SCORE * 2;
        public const int INITIAL_BETA = -INITIAL_ALPHA;
        public const int MAX_DEPTH = 50;
        public GameStage GameStage = GameStage.Opening;

        public ArtemisEngine(GameState gameState, IEngineConfig config, OpeningBook openingBook)
        {
            this.gameState = gameState;
            gameState.NewPositionLoaded += GameState_NewPositionLoaded;
            evConfig = new EvaluationConfig();
            evaluator = new PositionEvaluator(gameState, evConfig);
            Config = config;
            this.openingBook = openingBook;
            threadMaster = new ThreadMaster(this, gameState, transpositionTable, config);
        }

        private void GameState_NewPositionLoaded(object sender, EventArgs e)
        {
            GameStage = GameStage.Opening;
        }

        public async Task<Move> Calculate(CancellationToken ct)
        {
            Move openingMove;
            if (Config.UseOpeningBook && openingBook.TryGetMove(gameState, out openingMove))
            {
                return openingMove;
            }
            UpdateGameStage();
            PVList pv;
            using (internalCts = new CancellationTokenSource())
            using (linkedCts = CancellationTokenSource.CreateLinkedTokenSource(internalCts.Token, ct))
            {
                Task<PVList> searchTask = threadMaster.Search(linkedCts.Token);
                Stopwatch watch = new Stopwatch();
                watch.Start();
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
                watch.Stop();
                Console.WriteLine($"Time: {watch.ElapsedMilliseconds}");
            }

            transpositionTable.Clear();
            if (pv != null && pv.First != null)
            {
                pv.First.Move.SetGameState(gameState);
                return pv.First.Move;
            }
            else
            {
                throw new Exception("Principal Variation is empty");
            }
        }

        private void UpdateGameStage()
        {
            int materialCount = 0;
            for (int pl = 0; pl <= 1; pl++)
            {
                for (int i = 0; i < 5; i++)
                {
                    if ((PieceType)i != PieceType.Pawn)
                    {
                        materialCount += BitboardUtils.SparsePopcount(gameState.Pieces[pl, i]) * evConfig.GetPieceValue((PieceType)i);
                    }
                }
            }
            if (materialCount < 1800)
            {
                GameStage = GameStage.Endgame;
            }
            else if (GameStage != GameStage.Middlegame)
            {
                int developedPieces = 0;
                for (int pl = 0; pl <= 1; pl++)
                {
                    ulong firstRank = BitboardUtils.FIRST_RANK[pl];
                    ulong undevelopedKnights = firstRank & gameState.Pieces[pl, (int)PieceType.Knight];
                    ulong undevelopedBishops = firstRank & gameState.Pieces[pl, (int)PieceType.Bishop];
                    developedPieces += 4 - BitboardUtils.Popcount(undevelopedKnights | undevelopedBishops);
                    if (Math.Abs(evaluator.EvaluateRooksConnected(pl)) > 0)
                    {
                        developedPieces += 2;
                    }
                }
                if (developedPieces >= 7)
                {
                    GameStage = GameStage.Middlegame;
                }
            }
        }
    }
}
