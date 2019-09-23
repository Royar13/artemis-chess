﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.AI.Transposition
{
    public class TranspositionTable
    {
        private Dictionary<ulong, TranspositionNode> table = new Dictionary<ulong, TranspositionNode>();

        public void AddOrUpdate(ulong key, TranspositionNode node)
        {
            table[key] = node;
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
