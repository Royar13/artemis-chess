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
        /// <summary>
        /// Do the players still have castling rights. 
        /// The first index is 0=White, 1=Black, the second is 0=Queenside,1=Kingside.
        /// </summary>
        public bool[,] CastlingAllowed = new bool[2, 2] { { true, true }, { true, true } };
        /// <summary>
        /// The position behind a pawn that moved 2 squares last move
        /// </summary>
        public ulong EnPassantCapture;
        public bool IsCheck = false;

        public IrrevState()
        {
        }

        public IrrevState(bool[,] castlingAllowed)
        {
            CastlingAllowed = castlingAllowed;
        }

        public IrrevState Copy()
        {
            return new IrrevState((bool[,])CastlingAllowed.Clone());
        }
    }
}
