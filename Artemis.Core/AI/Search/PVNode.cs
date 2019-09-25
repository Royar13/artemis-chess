using Artemis.Core.Moves;
using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.AI.Search
{
    public class PVNode
    {
        public Move Move;
        public PVNode Next;

        public PVNode(Move move, PVNode next = null)
        {
            Move = move;
            Next = next;
        }
    }
}
