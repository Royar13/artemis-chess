using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core
{
    /// <summary>
    /// Irreversible state - all the state data which can't be reversed back by a simple bit operation
    /// </summary>
    public class IrrevState
    {
        private GameState gameState;
        /// <summary>
        /// Do the players still have castling rights. 
        /// The first index is 0=White, 1=Black, the second is 0=Queenside,1=Kingside.
        /// </summary>
        public bool[,] CastlingAllowed = new bool[2, 2] { { true, true }, { true, true } };
        /// <summary>
        /// The position behind a pawn that moved 2 squares last move
        /// </summary>
        public ulong EnPassantCapture;
        public ulong ZobristHash;
        public bool? IsCheck;

        public IrrevState(GameState gameState)
        {
            this.gameState = gameState;
        }

        public IrrevState(GameState gameState, bool[,] castlingAllowed, ulong zobristHash)
        {
            this.gameState = gameState;
            CastlingAllowed = castlingAllowed;
            ZobristHash = zobristHash;
        }

        /// <summary>
        /// Copy the state before a new move is applied
        /// </summary>
        /// <returns></returns>
        public IrrevState Copy()
        {
            ulong hash = ZobristHash;
            gameState.ZobristHashUtils.ResetHashBeforeMove(ref hash, this);
            return new IrrevState(gameState, (bool[,])CastlingAllowed.Clone(), hash);
        }
    }
}
