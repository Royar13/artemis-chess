using Artemis.Core.Moves.PregeneratedAttacks;
using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.Moves.Generator
{
    class KnightMoveGenerator : MoveGenerator
    {
        public KnightMoveGenerator(GameState gameState, PregeneratedAttacksData pregeneratedAttacks) : base(gameState, pregeneratedAttacks, PieceType.Knight)
        {
        }

        public override ulong GenerateAttacksFromSquare(int sqInd)
        {
            return pregeneratedAttacks.GetKnightAttacks(sqInd);
        }
    }
}
