using ChessMoveGeneration.Moves;
using ChessMoveGeneration.Moves.Generator;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessMoveGeneration
{
    public class GameState
    {
        const string DEFAULT_FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        /// <summary>
        /// A bitboard containing each piece type.
        /// First index: 0=White, 1=Black.
        /// Second index: PieceType.
        /// </summary>
        public ulong[,] Pieces = new ulong[2, 6];

        /// <summary>
        /// A bitboard containing all the pieces.
        /// 0=White, 1=Black.
        /// </summary>
        public ulong[] Occupancy = new ulong[2];

        /// <summary>
        /// A bitboard containing the pieces of both sides.
        /// </summary>
        public ulong FullOccupancy
        {
            get
            {
                return Occupancy[0] | Occupancy[1];
            }
        }

        public int Turn { get; private set; }
        List<IrrevState> IrrevStates = new List<IrrevState>();
        List<Move> MovesHistory = new List<Move>();

        MagicBitboardsData magic = new MagicBitboardsData();
        MoveGeneratorBuilder moveGeneratorBuilder;
        /// <summary>
        /// Move generators for different piece types, indexed by the piece type
        /// </summary>
        IMoveGenerator[] moveGenerators;

        public GameState() : this(DEFAULT_FEN)
        {
        }

        public GameState(string fen)
        {
            moveGeneratorBuilder = new MoveGeneratorBuilder(this, magic);
            for (int i = 0; i < 6; i++)
            {
                moveGenerators[i] = moveGeneratorBuilder.Build((PieceType)i);
            }
            LoadFEN(fen);
        }

        public void Initialize()
        {
            magic.Initialize();
        }

        private void LoadFEN(string fen)
        {
            ulong currPos = 1;
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
                        int pieceType = (int)NotationToPieceType(char.ToUpper(c));
                        Pieces[pl, pieceType] |= currPos;
                        currPos = currPos << 1;
                    }
                }
            }
        }

        public static PieceType NotationToPieceType(char notation)
        {
            switch (notation)
            {
                case 'R':
                    return PieceType.Rook;
                case 'N':
                    return PieceType.Knight;
                case 'B':
                    return PieceType.Bishop;
                case 'Q':
                    return PieceType.Queen;
                case 'K':
                    return PieceType.King;
                case 'P':
                    return PieceType.Pawn;
                default:
                    throw new ArgumentException("No such piece notation");
            }
        }

        public IrrevState GetIrrevState()
        {
            return IrrevStates.Last();
        }

        private void ChangeTurn()
        {
            Turn = 1 - Turn;
        }

        public void MakeMove(Move move)
        {
            IrrevState irrevState = GetIrrevState().Copy();
            IrrevStates.Add(irrevState);
            move.Make();
            MovesHistory.Add(move);
            ChangeTurn();
        }

        public void UnmakeMove(Move move)
        {
            move.Unmake();
            MovesHistory.RemoveAt(MovesHistory.Count - 1);
            IrrevStates.RemoveAt(IrrevStates.Count - 1);
            ChangeTurn();
        }
    }
}
