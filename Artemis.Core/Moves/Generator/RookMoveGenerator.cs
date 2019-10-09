using Artemis.Core.Moves.PregeneratedAttacks;
using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.Moves.Generator
{
    class RookMoveGenerator : MoveGenerator
    {
        public RookMoveGenerator(GameState gameState, PregeneratedAttacksData pregeneratedAttacks) : base(gameState, pregeneratedAttacks, PieceType.Rook)
        {
        }

        public override ulong GenerateAttacksFromSquare(int sqInd)
        {
            return pregeneratedAttacks.GetRookAttacks(sqInd, gameState.FullOccupancy);
        }
    }
}
