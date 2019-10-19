using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core
{
    public class GameResultData
    {
        public GameResult Result { get; } = GameResult.Ongoing;
        public GameResultReason Reason { get; }
        public int Winner { get; }

        public GameResultData()
        {
        }

        public GameResultData(GameResult result, GameResultReason reason) : this(result, reason, 0)
        {
        }

        public GameResultData(GameResult result, GameResultReason reason, int winner)
        {
            Result = result;
            Reason = reason;
            Winner = winner;
        }
    }
}
