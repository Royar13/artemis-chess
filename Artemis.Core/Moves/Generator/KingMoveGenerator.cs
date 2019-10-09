using Artemis.Core.Moves.PregeneratedAttacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Artemis.Core.Moves.Generator
{
    public class KingMoveGenerator : MoveGenerator
    {
        readonly ulong[] castlingKingTarget = { 0x0400000000000004, 0x4000000000000040 };
        readonly ulong[] castlingEmptySquares = { 0x0E0000000000000E, 0x6000000000000060 };

        public KingMoveGenerator(GameState gameState, PregeneratedAttacksData pregeneratedAttacks) : base(gameState, pregeneratedAttacks, PieceType.King)
        {
        }

        public override ulong GenerateAttacksFromSquare(int sqInd)
        {
            return pregeneratedAttacks.GetKingAttacks(sqInd);
        }

        protected override IEnumerable<Move> GenerateMovesFromSquare(int sqInd, GenerationMode generationMode = GenerationMode.Normal)
        {
            ulong sq = BitboardUtils.GetBitboard(sqInd);
            IEnumerable<Move> moves = base.GenerateMovesFromSquare(sqInd, generationMode);

            if (generationMode == GenerationMode.Normal)
            {
                IEnumerable<Move> castling = GenerateCastlingMoves(sq);
                moves = castling.Concat(moves);
            }

            return moves;
        }

        protected IEnumerable<Move> GenerateCastlingMoves(ulong sq)
        {
            IrrevState irrevState = gameState.GetIrrevState();
            for (int i = 0; i <= 1; i++)
            {
                if (IsCastlingAllowed(irrevState, i))
                {
                    ulong target = castlingKingTarget[i] & BitboardUtils.FIRST_RANK[gameState.Turn];
                    Move move = new CastlingMove(gameState, sq, target);
                    yield return move;
                }
            }
        }

        protected bool IsCastlingAllowed(IrrevState irrevState, int side)
        {
            return irrevState.CastlingAllowed[gameState.Turn, side] &&
                    (castlingEmptySquares[side] & BitboardUtils.FIRST_RANK[gameState.Turn] & gameState.FullOccupancy) == 0;
        }
    }
}
