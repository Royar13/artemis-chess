using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.AI.Opening
{
    public class MoveNode
    {
        public string Text { get; }
        public List<MoveNode> Responses { get; set; } = new List<MoveNode>();

        public MoveNode(string text)
        {
            Text = text;
        }

        public MoveNode(string text, List<MoveNode> responses) : this(text)
        {
            Responses = responses;
        }
    }
}
