using Artemis.Core.Moves.PregeneratedAttacks;
using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.Moves.Generator
{
    class BishopMoveGenerator : MoveGenerator
    {
        public BishopMoveGenerator(GameState gameState, PregeneratedAttacksData pregeneratedAttacks) : base(gameState, pregeneratedAttacks, PieceType.Bishop)
        {
        }

        public override ulong GenerateAttacksFromSquare(int sqInd)
        {
            return pregeneratedAttacks.GetBishopAttacks(sqInd, gameState.FullOccupancy);
        }
    }
}
