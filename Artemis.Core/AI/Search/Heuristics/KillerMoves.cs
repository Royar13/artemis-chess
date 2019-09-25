using Artemis.Core.Moves;
using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.AI.Search.Heuristics
{
    public class KillerMoves
    {
        private Move[][] killerMovesTable = new Move[ArtemisEngine.MAX_DEPTH][];

        public KillerMoves()
        {
            Clear();
        }

        public void AddMove(Move move, int ply)
        {
            if (killerMovesTable[ply][0] == null)
            {
                killerMovesTable[ply][0] = move;
            }
            else if (killerMovesTable[ply][1] == null && !killerMovesTable[ply][0].Equals(move))
            {
                killerMovesTable[ply][1] = move;
            }
        }

        public Move[] GetKillerMoves(int ply)
        {
            return killerMovesTable[ply];
        }

        public void Clear()
        {
            for (int i = 0; i < killerMovesTable.Length; i++)
            {
                killerMovesTable[i] = new Move[2];
            }
        }
    }
}
