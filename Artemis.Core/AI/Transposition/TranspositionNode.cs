﻿using Artemis.Core.AI.Search;
using Artemis.Core.Moves;
using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.AI.Transposition
{
    public class TranspositionNode
    {
        public NodeType NodeType { get; }
        public int Score { get; }
        public int Depth { get; }
        public Move BestMove { get; }
        public PVList PV { get; }

        public TranspositionNode(NodeType nodeType, int score, int depth, Move bestMove, PVList pv)
        {
            NodeType = nodeType;
            Score = score;
            Depth = depth;
            BestMove = bestMove;
            PV = pv;
        }
    }
}
