using Artemis.Core.AI.Transposition;
using Artemis.Core.Moves;
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
        private ZobristHashUtils zobristHashUtils;
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
        /// <summary>
        /// Number of plies since the last capture or pawn move.
        /// Used to determine a draw by the 50-move rule.
        /// </summary>
        public int HalfmoveClock;
        /// <summary>
        /// Number of plies since the last null move.
        /// Used to avoid mistakes in the repetitions count.
        /// </summary>
        public int PliesFromNull;

        public IrrevState(ZobristHashUtils zobristHashUtils)
        {
            this.zobristHashUtils = zobristHashUtils;
        }

        private IrrevState(ZobristHashUtils zobristHashUtils, bool[,] castlingAllowed, ulong zobristHash, int halfmoveClock, int pliesFromNull)
        {
            this.zobristHashUtils = zobristHashUtils;
            CastlingAllowed = castlingAllowed;
            ZobristHash = zobristHash;
            HalfmoveClock = halfmoveClock;
            PliesFromNull = pliesFromNull;
        }

        private IrrevState(ZobristHashUtils zobristHashUtils, bool[,] castlingAllowed, ulong enPassantCapture, ulong zobristHash,
            bool? isCheck, int halfmoveClock, int pliesFromNull)
        {
            this.zobristHashUtils = zobristHashUtils;
            CastlingAllowed = castlingAllowed;
            EnPassantCapture = enPassantCapture;
            ZobristHash = zobristHash;
            IsCheck = isCheck;
            HalfmoveClock = halfmoveClock;
            PliesFromNull = pliesFromNull;
        }

        /// <summary>
        /// Copy the state before a new move is applied.
        /// </summary>
        /// <returns></returns>
        public IrrevState CopyBeforeMove()
        {
            ulong hash = ZobristHash;
            zobristHashUtils.ResetHashBeforeMove(ref hash, this);
            return new IrrevState(zobristHashUtils, (bool[,])CastlingAllowed.Clone(), hash, HalfmoveClock, PliesFromNull);
        }

        /// <summary>
        /// Copy the state as is.
        /// </summary>
        /// <returns></returns>
        public IrrevState Copy()
        {
            return new IrrevState(zobristHashUtils, (bool[,])CastlingAllowed.Clone(), EnPassantCapture, ZobristHash,
                IsCheck, HalfmoveClock, PliesFromNull);
        }
    }
}
