using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.Moves.Generator
{
    interface IMoveGenerator
    {
        ulong GenerateAttacks(int pl);
        IEnumerable<Move> GenerateMoves();
    }
}
