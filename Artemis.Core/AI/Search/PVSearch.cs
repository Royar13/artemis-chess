using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artemis.Core.AI.Evaluation;
using Artemis.Core.AI.Transposition;
using Artemis.Core.Moves;

namespace Artemis.Core.AI.Search
{
    /// <summary>
    /// Principal Variation Search
    /// </summary>
    class PVSearch : IMoveSearch
    {
        GameState gameState;
        TranspositionTable transpositionTable;
        PositionEvaluator evaluator;
        int searchDepth;

        public PVSearch(GameState gameState, TranspositionTable transpositionTable, PositionEvaluator evaluator)
        {
            this.gameState = gameState;
            this.transpositionTable = transpositionTable;
            this.evaluator = evaluator;
        }

        public void Calculate(int depth)
        {
            searchDepth = depth;
            Search(depth, ArtemisEngine.INITIAL_ALPHA, ArtemisEngine.INITIAL_BETA);
        }

        private int Search(int depth, int alpha, int beta)
        {
            ulong hash = gameState.GetIrrevState().ZobristHash;
            TranspositionNode node = null;
            if (transpositionTable.TryGetValue(hash, out node))
            {
                if (node.Depth >= depth)
                {
                    switch (node.NodeType)
                    {
                        case NodeType.PVNode:
                            return node.Score;
                        case NodeType.CutNode:
                            alpha = Math.Max(alpha, node.Score);
                            break;
                        case NodeType.AllNode:
                            beta = Math.Min(beta, node.Score);
                            break;
                    }

                    if (alpha >= beta)
                    {
                        return node.Score;
                    }
                }
            }

            if (depth == 0)
            {
                return evaluator.Evaluate(depth);
            }

            int originalAlpha = alpha;
            List<Move> moves = gameState.GetMoves();
            if (node != null && node.BestMove != null)
            {
                Move pvMove = moves.First(m => m.Equals(node.BestMove));
                moves.Remove(pvMove);
                moves.Insert(0, pvMove);
            }

            Move bestMove = null;
            bool fullSearch = true;
            bool cutoff = false;
            bool hasMoves = false;
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
                        score = -Search(depth - 1, -beta, -alpha);
                    }
                    else
                    {
                        score = -Search(depth - 1, -alpha - 1, -alpha);
                        if (score > alpha)
                        {
                            score = -Search(depth - 1, -beta, -alpha);
                        }
                    }

                    if (score >= beta)
                    {
                        //alpha-beta cutoff
                        alpha = beta;
                        bestMove = move;
                        cutoff = true;
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
            if (nodeType != NodeType.AllNode || node != null)
            {
                TranspositionNode updatedNode = new TranspositionNode(nodeType, alpha, depth, bestMove);
                transpositionTable.AddOrUpdate(hash, updatedNode);
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
