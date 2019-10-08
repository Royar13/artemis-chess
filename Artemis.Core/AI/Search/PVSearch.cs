using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        public PVSearch(GameState gameState, TranspositionTable transpositionTable, KillerMoves killerMoves, PositionEvaluator evaluator, MoveEvaluator moveEvaluator,
            QuiescenceSearch quietSearch, ConcurrentDictionary<ulong, bool> searchedNodes)
        {
            this.gameState = gameState;
            this.transpositionTable = transpositionTable;
            this.killerMoves = killerMoves;
            this.evaluator = evaluator;
            this.moveEvaluator = moveEvaluator;
            this.quietSearch = quietSearch;
            this.searchedNodes = searchedNodes;
        }

        public PVList Calculate(int depth, PVList prevPV, CancellationToken ct)
        {
            searchDepth = depth;
            searchingPV = false;
            this.ct = ct;
            if (searchDepth > 1 && prevPV != null)
            {
                searchingPV = true;
                currentPVNode = prevPV.First;
            }
            PVList pvList = new PVList();
            int score = Search(depth, ArtemisEngine.INITIAL_ALPHA, ArtemisEngine.INITIAL_BETA, pvList);
            pvList.Score = score;
            pvList.Depth = depth;
            return pvList;
        }

        private int Search(int depth, int alpha, int beta, PVList pvList)
        {
            if (ct.IsCancellationRequested)
            {
                return 0;
            }
            ulong hash = gameState.GetIrrevState().ZobristHash;
            TranspositionNode ttNode = null;
            if (transpositionTable.TryGetValue(hash, out ttNode))
            {
                if (ttNode.Depth >= depth)
                {
                    switch (ttNode.NodeType)
                    {
                        case NodeType.PVNode:
                            return ttNode.Score;
                        case NodeType.CutNode:
                            alpha = Math.Max(alpha, ttNode.Score);
                            break;
                        case NodeType.AllNode:
                            beta = Math.Min(beta, ttNode.Score);
                            break;
                    }

                    if (alpha >= beta)
                    {
                        return ttNode.Score;
                    }
                }
            }

            if (depth == 0)
            {
                return quietSearch.Search(alpha, beta);
            }

            int originalAlpha = alpha;
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
            Move[] killers = killerMoves.GetKillerMoves(searchDepth - depth);
            moves = moves.OrderByDescending(m => moveEvaluator.EvaluateMove(m, pvMove, hashMove, killers).Score).ToList();

            Move bestMove = null;
            bool fullSearch = true;
            bool cutoff = false;
            bool hasMoves = false;
            PVList newPV = new PVList();
            int originalLen = moves.Count;
            for (int i = 0; i < moves.Count && !cutoff; i++)
            {
                Move move = moves[i];
                gameState.MakeMove(move);
                if (move.IsLegal())
                {
                    hasMoves = true;
                    int score;
                    if (fullSearch || searchDepth == 1)
                    {
                        score = -Search(depth - 1, -beta, -alpha, newPV);
                    }
                    else
                    {
                        ulong moveHash = gameState.GetIrrevState().ZobristHash;
                        if (i >= originalLen || searchedNodes.TryAdd(moveHash, true))
                        {
                            score = -Search(depth - 1, -alpha - 1, -alpha, newPV);
                            searchedNodes.TryRemove(moveHash, out _);
                        }
                        else
                        {
                            moves.Add(move);
                            gameState.UnmakeMove(move);
                            continue;
                        }
                        if (score > alpha)
                        {
                            score = -Search(depth - 1, -beta, -alpha, newPV);
                        }
                    }

                    if (score >= beta)
                    {
                        //alpha-beta cutoff
                        alpha = beta;
                        bestMove = move;
                        cutoff = true;
                        killerMoves.AddMove(move, searchDepth - depth);
                    }
                    else if (score > alpha)
                    {
                        alpha = score;
                        bestMove = move;
                        fullSearch = false;
                    }
                }
                gameState.UnmakeMove(move);
            }

            if (!hasMoves)
            {
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
            if (nodeType == NodeType.PVNode)
            {
                PVNode node = new PVNode(bestMove);
                newPV.AddFirst(node);
                pvList.Replace(newPV);
            }
            TranspositionNode updatedNode = new TranspositionNode(nodeType, alpha, depth, bestMove);
            if (ttNode != null)
            {
                transpositionTable.Update(hash, ttNode, updatedNode);
            }
            else
            {
                transpositionTable.Add(hash, updatedNode);
            }

            return alpha;
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
