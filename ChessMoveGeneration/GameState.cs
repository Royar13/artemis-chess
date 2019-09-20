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
        public const int BOARD_SIZE = 8;
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
        IMoveGenerator[] moveGenerators = new IMoveGenerator[6];

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
                        int pieceType = (int)c.ToPieceType();
                        Pieces[pl, pieceType] |= currPos;
                        currPos = currPos << 1;
                    }
                }
            }
        }

        public PieceType GetPieceBySquare(int pl, ulong sq)
        {
            for (int i = 0; i < 6; i++)
            {
                if ((Pieces[pl, i] & sq) > 0)
                {
                    return (PieceType)i;
                }
            }
            throw new Exception("No piece was found on the square");
        }

        public List<Move> GetMoves()
        {
            List<Move> moves = new List<Move>();
            foreach (IMoveGenerator generator in moveGenerators)
            {
                moves.AddRange(generator.GenerateMoves());
            }
            return moves;
        }

        /// <summary>
        /// For use by the GUI only.
        /// </summary>
        /// <returns></returns>
        public List<Move> GetLegalMoves()
        {
            List<Move> moves = GetMoves();
            List<Move> legalMoves = new List<Move>();
            foreach (Move move in moves)
            {
                MakeMove(move);
                if (move.IsLegal())
                {
                    legalMoves.Add(move);
                }
                UnmakeMove(move);
            }
            return legalMoves;
        }

        public bool IsCheck()
        {
            ulong king = Pieces[Turn, (int)PieceType.King];
            return IsAttacked(Turn, king);
        }

        public bool IsAttacked(int pl, ulong bb)
        {
            for (int i = 0; i < 6; i++)
            {
                ulong attacks = moveGenerators[i].GenerateAttacks(1 - pl);
                if ((bb & attacks) > 0)
                {
                    return true;
                }
            }
            return false;
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

        /// <summary>
        /// Gets a list of tuples with piece information: square index, player, piece type.
        /// For use by the GUI.
        /// </summary>
        /// <returns></returns>
        public List<Tuple<int, int, PieceType>> GetPiecesList()
        {
            List<Tuple<int, int, PieceType>> pieces = new List<Tuple<int, int, PieceType>>();
            for (int pl = 0; pl < 2; pl++)
            {
                for (int i = 0; i < 6; i++)
                {
                    ulong piecesBB = Pieces[pl, i];
                    while (piecesBB > 0)
                    {
                        int sqInd = BitboardUtils.PopLSB(ref piecesBB);
                        Tuple<int, int, PieceType> piece = new Tuple<int, int, PieceType>(sqInd, pl, (PieceType)i);
                        pieces.Add(piece);

                    }
                }
            }
            return pieces;
        }
    }
}
