using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Artemis.Core.Moves.MagicBitboards;

namespace Artemis.Core.Moves.Generator
{
    public abstract class MoveGenerator : IMoveGenerator
    {
        protected GameState gameState;
        protected MagicBitboardsData magic;
        protected PieceType pieceType;
        protected GenerationMode generationMode;

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
        /// Gets an enumerable of the moves from a square.
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="sq"></param>
        /// <returns></returns>
        protected abstract IEnumerable<Move> GetMovesFromSquare(ulong sq);

        public IEnumerable<Move> GenerateMoves(GenerationMode generationMode = GenerationMode.Normal)
        {
            this.generationMode = generationMode;
            ulong piece = gameState.Pieces[gameState.Turn, (int)pieceType];
            while (piece > 0)
            {
                ulong from = BitboardUtils.GetLSB(piece);
                IEnumerable<Move> moves = GetMovesFromSquare(from);
                foreach (Move move in moves)
                {
                    yield return move;
                }
                piece ^= from;
            }
        }
    }
}
