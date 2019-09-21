using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.Moves.Generator
{
    public class KingMoveGenerator : MoveGenerator
    {
        readonly ulong[] castlingKingTarget = { 0x0400000000000004, 0x4000000000000040 };
        readonly ulong[] castlingEmptySquares = { 0x0E0000000000000E, 0x6000000000000060 };

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

            List<Move> castling = GenerateCastlingMoves(sq);
            foreach (Move move in castling)
            {
                yield return move;
            }
        }

        protected List<Move> GenerateCastlingMoves(ulong sq)
        {
            List<Move> castlingMoves = new List<Move>();
            IrrevState irrevState = gameState.GetIrrevState();
            for (int i = 0; i <= 1; i++)
            {
                if (IsCastlingAllowed(irrevState, i))
                {
                    ulong target = castlingKingTarget[i] & BitboardUtils.FIRST_RANK[gameState.Turn];
                    Move move = new CastlingMove(gameState, sq, target);
                    castlingMoves.Add(move);
                }
            }
            return castlingMoves;
        }

        protected bool IsCastlingAllowed(IrrevState irrevState, int side)
        {
            return irrevState.CastlingAllowed[gameState.Turn, side] &&
                    (castlingEmptySquares[side] & BitboardUtils.FIRST_RANK[gameState.Turn] & gameState.FullOccupancy) == 0;
        }
    }
}
