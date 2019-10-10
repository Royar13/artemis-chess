using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.Moves.Generator
{
    public interface IMoveGenerator
    {
        ulong GenerateAttacks(int pl);
        ulong GenerateAttacksFromSquare(int sqInd);
        IEnumerable<Move> GenerateMoves(GenerationMode generationMode = GenerationMode.Normal);
    }
}
