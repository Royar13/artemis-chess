using Artemis.Core.Moves.PregeneratedAttacks;
using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.Moves.Generator
{
    class QueenMoveGenerator : MoveGenerator
    {
        public QueenMoveGenerator(GameState gameState, PregeneratedAttacksData pregeneratedAttacks) : base(gameState, pregeneratedAttacks, PieceType.Queen)
        {
        }

        public override ulong GenerateAttacksFromSquare(int sqInd)
        {
            ulong rookAttacks = pregeneratedAttacks.GetRookAttacks(sqInd, gameState.FullOccupancy);
            ulong bishopAttacks = pregeneratedAttacks.GetBishopAttacks(sqInd, gameState.FullOccupancy);
            ulong attacks = rookAttacks | bishopAttacks;
            return attacks;
        }
    }
}
