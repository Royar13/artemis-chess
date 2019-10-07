using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.AI.Transposition
{
    public class TranspositionTable
    {
        private ConcurrentDictionary<ulong, TranspositionNode> table = new ConcurrentDictionary<ulong, TranspositionNode>();

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

        public bool TryGetValue(ulong key, out TranspositionNode node)
        {
            return table.TryGetValue(key, out node);
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
