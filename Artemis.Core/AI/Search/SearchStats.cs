using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.AI.Search
{
    public class SearchStats
    {
        public long Nodes = 0;
        public long PVNodes = 0;
        public long Time;
        public int NPS
        {
            get
            {
                int nps = (int)Math.Round(Nodes / ((double)Time / 1000));
                return nps;
            }
        }
        public int AlphaBetaCutoffs = 0;
        public int NullMoveCutoffs = 0;
        public int TTHits = 0;
    }
}
