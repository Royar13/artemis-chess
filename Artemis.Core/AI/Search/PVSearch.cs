using System;
using System.Collections.Generic;
using System.Text;
using Artemis.Core.Moves;

namespace Artemis.Core.AI.Search
{
    /// <summary>
    /// Principal Variation Search
    /// </summary>
    class PVSearch : IMoveSearch
    {
        GameState gameState;
        int searchDepth;

        public PVSearch(GameState gameState, int searchDepth)
        {
            this.gameState = gameState;
            this.searchDepth = searchDepth;
        }

        public Move CalculateMove()
        {
            throw new NotImplementedException();
        }

        private int Search()
        {
            throw new NotImplementedException();
        }
    }
}
