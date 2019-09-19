using System;
using System.Collections.Generic;
using System.Text;

namespace ChessMoveGeneration.Moves.Generator
{
    class QueenMoveGenerator : SlidingMoveGenerator
    {
        public QueenMoveGenerator(GameState gameState, MagicBitboardsData magic) : base(gameState, magic, PieceType.Queen)
        {
        }

        protected override ulong GetAttacksFromSquare(int pl, ulong sq)
        {
            int sqInd = BitboardUtils.BitScanForward(sq);
            ulong rookAttacks = magic.GetAttacks(sqInd, gameState.FullOccupancy, true);
            ulong bishopAttacks = magic.GetAttacks(sqInd, gameState.FullOccupancy, false);
            ulong attacks = rookAttacks | bishopAttacks;
            return attacks;
        }
    }
}
