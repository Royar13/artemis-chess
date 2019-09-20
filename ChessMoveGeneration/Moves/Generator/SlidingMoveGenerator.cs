using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.Moves.Generator
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

        protected override IEnumerable<Move> GetMovesFromSquare(ulong sq)
        {
            ulong reversedOccupancy = ~gameState.Occupancy[gameState.Turn];
            ulong attacks = GetAttacksFromSquare(gameState.Turn, sq) & reversedOccupancy;
            while (attacks > 0)
            {
                ulong to = BitboardUtils.GetLSB(attacks);
                Move move = new Move(gameState, sq, to, pieceType);
                yield return move;
                attacks ^= to;
            }
        }
    }
}
