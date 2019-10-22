using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Artemis.Core.AI.Evaluation;
using Artemis.Core.AI.Search.Heuristics;
using Artemis.Core.AI.Transposition;
using Artemis.Core.Moves;

namespace Artemis.Core.AI.Search
{
    /// <summary>
    /// Principal Variation Search
    /// </summary>
    class PVSearch
    {
        ArtemisEngine engine;
        GameState gameState;
        TranspositionTable transpositionTable;
        KillerMoves killerMoves;
        PositionEvaluator evaluator;
        MoveEvaluator moveEvaluator;
        QuiescenceSearch quietSearch;
        int searchDepth;
        PVNode currentPVNode;
        bool searchingPV;
        CancellationToken ct;
        ConcurrentDictionary<ulong, bool> searchedNodes;
        IEngineConfig config;
        SearchStats searchStats;

        public PVSearch(ArtemisEngine engine, GameState gameState, TranspositionTable transpositionTable, KillerMoves killerMoves, PositionEvaluator evaluator, MoveEvaluator moveEvaluator,
            QuiescenceSearch quietSearch, ConcurrentDictionary<ulong, bool> searchedNodes, IEngineConfig config)
        {
            this.engine = engine;
            this.gameState = gameState;
            this.transpositionTable = transpositionTable;
            this.killerMoves = killerMoves;
            this.evaluator = evaluator;
            this.moveEvaluator = moveEvaluator;
            this.quietSearch = quietSearch;
            this.searchedNodes = searchedNodes;
            this.config = config;
        }

        public PVList Calculate(int depth, PVList prevPV, CancellationToken ct, SearchStats searchStats)
        {
            searchDepth = depth;
            searchingPV = false;
            this.searchStats = searchStats;
            this.ct = ct;
            if (searchDepth > 1 && prevPV != null)
            {
                searchingPV = true;
                currentPVNode = prevPV.First;
            }
            PVList pvList = new PVList();
            int score = Search(depth, 0, ArtemisEngine.INITIAL_ALPHA, ArtemisEngine.INITIAL_BETA, pvList, false);
            pvList.Score = score;
            pvList.Depth = depth;
            return pvList;
        }

        private int Search(int depth, int ply, int alpha, int beta, PVList pvList, bool nullMoveReduction)
        {
            if (ct.IsCancellationRequested)
            {
                return 0;
            }

            searchStats.Nodes++;

            if (gameState.IsDraw())
            {
                int score = evaluator.GetDrawScore(engine.EngineColor);
                if (score > alpha)
                {
                    pvList.Replace(new PVList());
                }
                return score;
            }

            if (gameState.IsCheck() && depth < ArtemisEngine.MAX_DEPTH)
            {
                //check extension
                depth += 1;
            }

            int originalAlpha = alpha;
            ulong hash = gameState.GetIrrevState().ZobristHash;
            TTHit ttHit = transpositionTable.TryGetValue(hash, depth, alpha, beta, pvList);
            if (ttHit.HitType == HitType.Hit)
            {
                searchStats.TTHits++;
                return ttHit.Score;
            }
            TranspositionNode ttNode = ttHit.TTNode;

            if (depth == 0 || ply == ArtemisEngine.MAX_DEPTH)
            {
                int score = quietSearch.Search(alpha, beta);
                if (score > alpha)
                {
                    pvList.Replace(new PVList());
                }
                return score;
            }

            bool lmrCandidatePosition = depth >= 3 && !searchingPV && !gameState.IsCheck();

            Move bestMove = null;
            bool cutoff = false;
            PVList newPV = new PVList();

            if (ttHit.HitType != HitType.AvoidNullMove && ply > 0 && depth > config.NullMoveDepthReduction + 1
                && !nullMoveReduction && engine.GameStage != GameStage.Endgame && !gameState.IsCheck())
            {
                //null move pruning
                gameState.MakeNullMove();
                int nextDepth = depth - 1 - config.NullMoveDepthReduction;
                int score = -Search(nextDepth, ply + 1, -beta, -beta + 1, newPV, true);
                gameState.UnmakeNullMove();
                if (score >= beta)
                {
                    searchStats.NullMoveCutoffs++;
                    searchStats.AlphaBetaCutoffs++;
                    TranspositionNode newNode = new TranspositionNode(NodeType.CutNode, score, nextDepth + 1, null, null);
                    SaveNode(hash, ttNode, newNode);
                    //alpha-beta cutoff
                    return beta;
                }
            }

            List<Move> moves = gameState.GetMoves();
            Move pvMove = null;
            if (searchingPV)
            {
                if (currentPVNode != null)
                {
                    pvMove = currentPVNode.Move;
                    currentPVNode = currentPVNode.Next;
                }
                else
                {
                    searchingPV = false;
                }
            }
            Move hashMove = null;
            if (ttNode != null)
            {
                hashMove = ttNode.BestMove;
            }
            Move[] killers = killerMoves.GetKillerMoves(ply);
            moves = moves.OrderByDescending(m => moveEvaluator.EvaluateMove(m, pvMove, hashMove, killers).Score).ToList();

            int originalLen = moves.Count;
            int moveCount = 0;
            for (int i = 0; i < moves.Count && !cutoff; i++)
            {
                Move move = moves[i];
                gameState.MakeMove(move);
                if (move.IsLegal())
                {
                    int nextDepth = depth - 1;
                    bool lmrReduction = false;
                    if (lmrCandidatePosition && moveCount >= 4 && move.IsQuiet() && !gameState.IsCheck())
                    {
                        lmrReduction = true;
                        nextDepth -= 1;
                    }

                    int score;
                    if (moveCount == 0 || searchDepth == 1)
                    {
                        score = -Search(nextDepth, ply + 1, -beta, -alpha, newPV, false);
                    }
                    else
                    {
                        ulong moveHash = gameState.GetIrrevState().ZobristHash;
                        if (!config.Multithreading || i >= originalLen || searchedNodes.TryAdd(moveHash, true))
                        {
                            score = -Search(nextDepth, ply + 1, -alpha - 1, -alpha, newPV, false);
                            searchedNodes.TryRemove(moveHash, out _);

                            if (score > alpha)
                            {
                                if (lmrReduction)
                                {
                                    nextDepth++;
                                }
                                score = -Search(nextDepth, ply + 1, -beta, -alpha, newPV, false);
                            }
                        }
                        else
                        {
                            moves.Add(move);
                            gameState.UnmakeMove(move);
                            continue;
                        }
                    }

                    if (score >= beta)
                    {
                        //alpha-beta cutoff
                        searchStats.AlphaBetaCutoffs++;
                        alpha = beta;
                        bestMove = move;
                        cutoff = true;
                        killerMoves.AddMove(move, ply);
                    }
                    else if (score > alpha)
                    {
                        alpha = score;
                        bestMove = move;
                    }

                    moveCount++;
                }
                gameState.UnmakeMove(move);
            }

            if (moveCount == 0)
            {
                //player has no legal moves
                if (gameState.IsCheck())
                {
                    return -PositionEvaluator.CHECKMATE_SCORE - depth;
                }
                else
                {
                    return 0;
                }
            }

            NodeType nodeType = GetNodeType(originalAlpha, beta, alpha);
            PVList nodePV = null;
            if (nodeType == NodeType.PVNode)
            {
                searchStats.PVNodes++;
                PVNode node = new PVNode(bestMove);
                newPV.AddFirst(node);
                nodePV = newPV;
                pvList.Replace(newPV);
            }
            TranspositionNode updatedNode = new TranspositionNode(nodeType, alpha, depth, bestMove, nodePV);
            SaveNode(hash, ttNode, updatedNode);

            return alpha;
        }

        private void SaveNode(ulong hash, TranspositionNode existingNode, TranspositionNode newNode)
        {
            if (existingNode != null)
            {
                transpositionTable.Update(hash, existingNode, newNode);
            }
            else
            {
                transpositionTable.Add(hash, newNode);
            }
        }

        private NodeType GetNodeType(int alpha, int beta, int score)
        {
            if (score >= beta)
            {
                return NodeType.CutNode;
            }
            else if (score > alpha)
            {
                return NodeType.PVNode;
            }
            else
            {
                return NodeType.AllNode;
            }
        }
    }
}
