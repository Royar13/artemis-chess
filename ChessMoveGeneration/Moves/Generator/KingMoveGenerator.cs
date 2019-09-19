using System;
using System.Collections.Generic;
using System.Text;

namespace ChessMoveGeneration.Moves.Generator
{
    public class KingMoveGenerator : MoveGenerator
    {
        public KingMoveGenerator(GameState gameState, MagicBitboardsData magic) : base(gameState, magic, PieceType.King)
        {
        }

        public override ulong GenerateAttacks(int pl)
        {
            ulong king = gameState.Pieces[pl, (int)PieceType.King];
            ulong attacks = king << 1 & BitboardUtils.NOT_A_FILE;
            attacks |= king >> 1 & BitboardUtils.NOT_H_FILE;
            attacks |= king << 9 & BitboardUtils.NOT_A_FILE;
            attacks |= king << 7 & BitboardUtils.NOT_H_FILE;
            attacks |= king >> 7 & BitboardUtils.NOT_A_FILE;
            attacks |= king >> 9 & BitboardUtils.NOT_H_FILE;
            attacks |= king << 8;
            attacks |= king >> 8;
            return attacks;
        }

        protected override IEnumerable<Move> GetMovesFromSquare(ulong sq)
        {
            ulong reversedOccupancy = ~gameState.Occupancy[gameState.Turn];
            ulong to = sq << 1 & BitboardUtils.NOT_A_FILE & reversedOccupancy;
            if (to != 0)
                yield return new Move(gameState, sq, to, pieceType);

            to = sq >> 1 & BitboardUtils.NOT_H_FILE & reversedOccupancy;
            if (to != 0)
                yield return new Move(gameState, sq, to, pieceType);

            to = sq << 9 & BitboardUtils.NOT_A_FILE & reversedOccupancy;
            if (to != 0)
                yield return new Move(gameState, sq, to, pieceType);

            to = sq << 7 & BitboardUtils.NOT_H_FILE & reversedOccupancy;
            if (to != 0)
                yield return new Move(gameState, sq, to, pieceType);

            to = sq >> 7 & BitboardUtils.NOT_A_FILE & reversedOccupancy;
            if (to != 0)
                yield return new Move(gameState, sq, to, pieceType);

            to = sq >> 9 & BitboardUtils.NOT_H_FILE & reversedOccupancy;
            if (to != 0)
                yield return new Move(gameState, sq, to, pieceType);

            to = sq << 8 & reversedOccupancy;
            if (to != 0)
                yield return new Move(gameState, sq, to, pieceType);

            to = sq >> 8 & reversedOccupancy;
            if (to != 0)
                yield return new Move(gameState, sq, to, pieceType);
        }
    }
}
