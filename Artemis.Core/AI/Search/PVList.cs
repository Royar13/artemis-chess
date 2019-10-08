using Artemis.Core.Moves;
using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.AI.Search
{
    public class PVList
    {
        public PVNode First { get; private set; }
        public int Depth;
        public int Score;

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
            string scoreStr = Score > 0 ? "+" + Score : Score.ToString();
            string str = $"({Depth}) {scoreStr} {line}";
            return str;
        }

        public static bool operator >(PVList a, PVList b)
        {
            return a.Depth > b.Depth || (a.Depth == b.Depth && a.Score > b.Score);
        }

        public static bool operator <(PVList a, PVList b)
        {
            return !(a > b);
        }
    }
}
