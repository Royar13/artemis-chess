using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.AI.Transposition
{
    public enum NodeType
    {
        PVNode,     //Exact score
        CutNode,    //Lower bound
        AllNode     //Upper bound
    }
}
