using System;
using System.Collections.Generic;
using System.Text;

namespace ChessMoveGeneration.Moves.Generator
{
    interface IMoveGenerator
    {
        ulong GenerateAttacks(int pl);
        IEnumerable<Move> GenerateMoves();
    }
}
