using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.AI.Transposition
{
    public class TranspositionTable
    {
        private Dictionary<ulong, TranspositionNode> table = new Dictionary<ulong, TranspositionNode>();

        public void Add(ulong key, TranspositionNode node)
        {
            table[key] = node;
        }

        public void Update(ulong key, TranspositionNode existingNode, TranspositionNode node)
        {
            if (node.Depth > existingNode.Depth)
            {
                table[key] = node;
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
