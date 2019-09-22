using Artemis.Core.AI.Search;
using Artemis.Core.Moves;
using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.AI
{
    public class ArtemisEngine
    {
        IMoveSearch pvSearch;
        GameState gameState;

        public ArtemisEngine(GameState gameState)
        {
            this.gameState = gameState;
        }

        public Move CalculateMove()
        {
            throw new NotImplementedException();
        }
    }
}
