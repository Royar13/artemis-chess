using System;

namespace ChessMoveGeneration
{
    public class GameState
    {
        const string DEFAULT_FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        /// <summary>
        /// A bitboard containing the Rooks.
        /// 0=White, 1=Black.
        /// </summary>
        public UInt64[] Rooks = new UInt64[2];
        /// <summary>
        /// A bitboard containing the Knights.
        /// 0=White, 1=Black.
        /// </summary>
        public UInt64[] Knights = new UInt64[2];
        /// <summary>
        /// A bitboard containing the Bishops.
        /// 0=White, 1=Black.
        /// </summary>
        public UInt64[] Bishops = new UInt64[2];
        /// <summary>
        /// A bitboard containing the Queen.
        /// 0=White, 1=Black.
        /// </summary>
        public UInt64[] Queen = new UInt64[2];
        /// <summary>
        /// A bitboard containing the King.
        /// 0=White, 1=Black.
        /// </summary>
        public UInt64[] King = new UInt64[2];
        /// <summary>
        /// A bitboard containing the Pawns.
        /// 0=White, 1=Black.
        /// </summary>
        public UInt64[] Pawns = new UInt64[2];
        /// <summary>
        /// A bitboard containing all the pieces.
        /// 0=White, 1=Black.
        /// </summary>
        public UInt64[] Pieces = new UInt64[2];

        public int Turn;
        /// <summary>
        /// Do the players still have castling rights. 
        /// The first index is 0=White, 1=Black, the second is 0=Queenside,1=Kingside.
        /// </summary>
        public bool[,] CastlingAllowed = new bool[2, 2];
        /// <summary>
        /// The position behind a pawn that moved 2 squares last move
        /// </summary>
        public UInt64? EnPassantCapture;

        public GameState() : this(DEFAULT_FEN)
        {
        }

        public GameState(string fen)
        {
            LoadFEN(fen);
        }

        private void LoadFEN(string fen)
        {
            UInt64 currPos = 1;
            string[] parts = fen.Split(' ');
            string[] ranks = parts[0].Split('/');
            for (int i = ranks.Length - 1; i >= 0; i--)
            {
                string rank = ranks[i];
                for (int j = 0; j < rank.Length; j++)
                {
                    char c = rank[j];
                    if (char.IsDigit(c))
                    {
                        int shift = (int)char.GetNumericValue(c);
                        currPos = currPos << shift;
                    }
                    else
                    {
                        int pl = char.IsLower(c) ? 1 : 0;
                        UInt64[] pieces = GetPiecesByType(char.ToUpper(c));
                        pieces[pl] = pieces[pl] ^ currPos;
                        currPos = currPos << 1;
                    }
                }
            }
        }

        public UInt64[] GetPiecesByType(char notation)
        {
            switch (notation)
            {
                case 'R':
                    return Rooks;
                case 'N':
                    return Knights;
                case 'B':
                    return Bishops;
                case 'Q':
                    return Queen;
                case 'K':
                    return King;
                case 'P':
                    return Pawns;
                default:
                    throw new ArgumentException("No such piece notation");
            }
        }
    }
}
