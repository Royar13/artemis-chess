using System;
using System.Collections.Generic;
using System.Text;

namespace ChessMoveGeneration.Moves.Generator
{
    class RookMoveGenerator : SlidingMoveGenerator
    {
        public RookMoveGenerator(GameState gameState, MagicBitboardsData magic) : base(gameState, magic, PieceType.Rook)
        {
        }

        protected override ulong GetAttacksFromSquare(int pl, ulong sq)
        {
            int sqInd = BitboardUtils.BitScanForward(sq);
            ulong attacks = magic.GetAttacks(sqInd, gameState.FullOccupancy, true);
            return attacks;
        }
    }
}
