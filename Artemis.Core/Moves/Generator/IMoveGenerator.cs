using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.Moves.Generator
{
    public interface IMoveGenerator
    {
        ulong GenerateAttacks(int pl);
        ulong GenerateAttacksFromSquare(int sqInd);
        ulong GenerateAttacksFromSquare(int sqInd, int pl);
        IEnumerable<Move> GenerateMoves(GenerationMode generationMode = GenerationMode.Normal);
    }
}
