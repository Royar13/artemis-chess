using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Artemis.Core.Moves.PregeneratedAttacks;

namespace Artemis.Core.Moves.Generator
{
    public abstract class MoveGenerator : IMoveGenerator
    {
        protected GameState gameState;
        protected PregeneratedAttacksData pregeneratedAttacks;
        protected PieceType pieceType;

        public MoveGenerator(GameState gameState, PregeneratedAttacksData pregeneratedAttacks, PieceType pieceType)
        {
            this.gameState = gameState;
            this.pregeneratedAttacks = pregeneratedAttacks;
            this.pieceType = pieceType;
        }

        /// <summary>
        /// Get a bitboard of the attacks, including friendly attacks (defenders).
        /// </summary>
        /// <param name="pl"></param>
        /// <returns></returns>
        public virtual ulong GenerateAttacks(int pl)
        {
            ulong attacks = 0;
            ulong piece = gameState.Pieces[pl, (int)pieceType];
            while (piece > 0)
            {
                int from = BitboardUtils.PopLSB(ref piece);
                attacks |= GenerateAttacksFromSquare(from);
            }
            return attacks;
        }

        public abstract ulong GenerateAttacksFromSquare(int sqInd);

        public virtual ulong GenerateAttacksFromSquare(int sqInd, int pl)
        {
            return GenerateAttacksFromSquare(sqInd);
        }

        /// <summary>
        /// Gets an enumerable of the moves from a square.
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        protected virtual IEnumerable<Move> GenerateMovesFromSquare(int sqInd, GenerationMode generationMode = GenerationMode.Normal)
        {
            ulong attacks = GenerateAttacksFromSquare(sqInd);
            ulong mask = generationMode == GenerationMode.Normal ? ~gameState.Occupancy[gameState.Turn] : gameState.Occupancy[1 - gameState.Turn];
            ulong moves = attacks & mask;
            ulong from = BitboardUtils.GetBitboard(sqInd);
            while (moves > 0)
            {
                ulong to = gameState.Turn == 0 ? BitboardUtils.GetMSB(moves) : BitboardUtils.GetLSB(moves);
                Move move = new Move(gameState, from, to, pieceType);
                yield return move;
                moves ^= to;
            }
        }

        public IEnumerable<Move> GenerateMoves(GenerationMode generationMode = GenerationMode.Normal)
        {
            ulong piece = gameState.Pieces[gameState.Turn, (int)pieceType];
            while (piece > 0)
            {
                int from = BitboardUtils.PopLSB(ref piece);
                IEnumerable<Move> moves = GenerateMovesFromSquare(from, generationMode);
                foreach (Move move in moves)
                {
                    yield return move;
                }
            }
        }
    }
}
