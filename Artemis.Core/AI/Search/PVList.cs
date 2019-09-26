using Artemis.Core.Moves;
using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.AI.Search
{
    public class PVList
    {
        public PVNode First { get; private set; }

        public void AddFirst(PVNode node)
        {
            node.Next = First;
            First = node;
        }

        public void Replace(PVList lst)
        {
            First = lst.First;
        }

        public override string ToString()
        {
            List<Move> pv = new List<Move>();
            PVNode node = First;
            while (node != null)
            {
                pv.Add(node.Move);
                node = node.Next;
            }
            string line = string.Join(", ", pv);
            return line;
        }
    }
}
