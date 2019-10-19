using Artemis.Core.AI.Search;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.AI.Transposition
{
    public class TranspositionTable
    {
        private ConcurrentDictionary<ulong, TranspositionNode> table = new ConcurrentDictionary<ulong, TranspositionNode>();
        private IEngineConfig engineConfig;

        public TranspositionTable(IEngineConfig engineConfig)
        {
            this.engineConfig = engineConfig;
        }

        public void Add(ulong key, TranspositionNode node)
        {
            table.TryAdd(key, node);
        }

        public void Update(ulong key, TranspositionNode existingNode, TranspositionNode updatedNode)
        {
            if (updatedNode.Depth > existingNode.Depth)
            {
                table.TryUpdate(key, updatedNode, existingNode);
            }
        }

        public TTHit TryGetValue(ulong key, int depth, int alpha, int beta, PVList currentPV)
        {
            TTHit hit = new TTHit();
            hit.HitType = HitType.Miss;
            bool found = table.TryGetValue(key, out hit.TTNode);
            if (found)
            {
                hit.HitType = HitType.Useless;
                if (hit.TTNode.Depth >= depth)
                {
                    switch (hit.TTNode.NodeType)
                    {
                        case NodeType.CutNode:
                            if (hit.TTNode.Score >= beta)
                            {
                                hit.HitType = HitType.Hit;
                                hit.Score = beta;
                            }
                            break;
                        case NodeType.AllNode:
                            if (hit.TTNode.Score <= alpha)
                            {
                                hit.HitType = HitType.Hit;
                                hit.Score = alpha;
                            }
                            break;
                        case NodeType.PVNode:
                            hit.HitType = HitType.Hit;
                            hit.Score = hit.TTNode.Score;
                            currentPV.Replace(hit.TTNode.PV);
                            break;
                    }
                }
                else if (hit.TTNode.Depth >= depth - engineConfig.NullMoveDepthReduction &&
                    hit.TTNode.NodeType != NodeType.CutNode && hit.TTNode.Score < beta)
                {
                    hit.HitType = HitType.AvoidNullMove;
                }
            }
            return hit;
        }

        public bool ContainsKey(ulong key)
        {
            return table.ContainsKey(key);
        }

        public void Clear()
        {
            table.Clear();
        }
    }
}
