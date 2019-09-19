using System;
using System.Collections.Generic;
using System.Text;

namespace ChessMoveGeneration.Moves.Generator
{
    public abstract class SlidingMoveGenerator : MoveGenerator
    {
        public SlidingMoveGenerator(GameState gameState, MagicBitboardsData magic, PieceType pieceType) : base(gameState, magic, pieceType)
        {
        }

        /// <summary>
        /// Gets a bitboard of attacks.
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="sq"></param>
        /// <returns></returns>
        protected abstract ulong GetAttacksFromSquare(int pl, ulong sq);

        public override ulong GenerateAttacks(int pl)
        {
            ulong attacks = 0;
            ulong piece = gameState.Pieces[pl, (int)pieceType];
            while (piece > 0)
            {
                ulong lsb = BitboardUtils.GetLSB(piece);
                attacks |= GetAttacksFromSquare(pl, lsb);
                piece ^= lsb;
            }
            return attacks;
        }

        protected override IEnumerable<ulong> GetSequentialAttacksFromSquare(int pl, ulong sq)
        {
            ulong reversedOccupancy = ~gameState.Occupancy[pl];
            ulong attacks = GetAttacksFromSquare(pl, sq) & reversedOccupancy;
            while (attacks > 0)
            {
                ulong to = BitboardUtils.GetLSB(attacks);
                yield return to;
                attacks ^= to;
            }
        }
    }
}
