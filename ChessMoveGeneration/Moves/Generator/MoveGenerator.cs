using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ChessMoveGeneration.Moves.Generator
{
    public abstract class MoveGenerator : IMoveGenerator
    {
        protected GameState gameState;
        protected MagicBitboardsData magic;
        protected PieceType pieceType;

        public MoveGenerator(GameState gameState, MagicBitboardsData magic, PieceType pieceType)
        {
            this.gameState = gameState;
            this.magic = magic;
            this.pieceType = pieceType;
        }

        /// <summary>
        /// Get a bitboard of the attacks, including friendly attacks (defenders).
        /// </summary>
        /// <param name="pl"></param>
        /// <returns></returns>
        public abstract ulong GenerateAttacks(int pl);

        /// <summary>
        /// Gets an enumerable of the attacks, not including friendly attacks (defenders).
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="sq"></param>
        /// <returns></returns>
        protected abstract IEnumerable<ulong> GetSequentialAttacksFromSquare(int pl, ulong sq);

        public IEnumerable<Move> GenerateMoves()
        {
            ulong piece = gameState.Pieces[gameState.Turn, (int)pieceType];
            while (piece > 0)
            {
                ulong from = BitboardUtils.GetLSB(piece);
                IEnumerable<ulong> attacks = GetSequentialAttacksFromSquare(gameState.Turn, from);
                foreach (ulong to in attacks)
                {
                    Move move = new Move(gameState, from, to, pieceType);
                    yield return move;
                }
                piece ^= from;
            }
        }
    }
}
