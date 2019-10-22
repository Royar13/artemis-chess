using Artemis.Core.Moves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Artemis.Core.AI.Search.Heuristics
{
    public class KillerMoves
    {
        private ulong positionHash;
        private Move[][] killerMovesTable = new Move[ArtemisEngine.MAX_DEPTH][];

        public KillerMoves()
        {
            Reset();
        }

        public void AddMove(Move move, int ply)
        {
            if (move.IsQuiet() && (killerMovesTable[ply][0] == null || !killerMovesTable[ply][0].Equals(move)))
            {
                killerMovesTable[ply][1] = killerMovesTable[ply][0];
                killerMovesTable[ply][0] = move;
            }
        }

        public Move[] GetKillerMoves(int ply)
        {
            return killerMovesTable[ply];
        }

        private void Reset()
        {
            for (int i = 0; i < killerMovesTable.Length; i++)
            {
                killerMovesTable[i] = new Move[2];
            }
        }

        private void Shift(int amount = 2)
        {
            for (int i = 0; i < ArtemisEngine.MAX_DEPTH; i++)
            {
                if (i < ArtemisEngine.MAX_DEPTH - amount)
                {
                    killerMovesTable[i][0] = killerMovesTable[i + amount][0];
                    killerMovesTable[i][1] = killerMovesTable[i + amount][1];
                }
                else
                {
                    killerMovesTable[i] = new Move[2];
                }
            }
        }

        public void Prepare(List<IrrevState> irrevStates)
        {
            if (irrevStates.Count >= 3 && irrevStates[irrevStates.Count - 3].ZobristHash == positionHash)
            {
                Shift();
            }
            else
            {
                Reset();
            }
            positionHash = irrevStates.Last().ZobristHash;
        }
    }
}
