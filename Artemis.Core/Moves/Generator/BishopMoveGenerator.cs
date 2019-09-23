using Artemis.Core.Moves.MagicBitboards;
using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.Moves.Generator
{
    class BishopMoveGenerator : SlidingMoveGenerator
    {
        public BishopMoveGenerator(GameState gameState, MagicBitboardsData magic) : base(gameState, magic, PieceType.Bishop)
        {
        }

        protected override ulong GetAttacksFromSquare(int pl, ulong sq)
        {
            int sqInd = BitboardUtils.BitScanForward(sq);
            ulong attacks = magic.GetAttacks(sqInd, gameState.FullOccupancy, false);
            return attacks;
        }
    }
}
