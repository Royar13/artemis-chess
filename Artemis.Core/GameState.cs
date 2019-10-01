using Artemis.Core.AI.Transposition;
using Artemis.Core.FormatConverters;
using Artemis.Core.Moves;
using Artemis.Core.Moves.Generator;
using Artemis.Core.Moves.MagicBitboards;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Artemis.Core
{
    public class GameState
    {
        private List<IrrevState> irrevStates = new List<IrrevState>();
        private MagicBitboardsData magic = new MagicBitboardsData();
        private MoveGeneratorBuilder moveGeneratorBuilder;
        private IFormatConverter fenConverter = new FENConverter();
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
        public ZobristHashUtils ZobristHashUtils { get; }

        /// <summary>
        /// Move generators for different piece types, indexed by the piece type
        /// </summary>
        public IMoveGenerator[] MoveGenerators = new IMoveGenerator[6];

        public GameState() : this(DEFAULT_FEN)
        {
        }

        public GameState(string fen)
        {
            magic.Initialize();
            ZobristHashUtils = new ZobristHashUtils(this);
            moveGeneratorBuilder = new MoveGeneratorBuilder(this, magic);
            for (int i = 0; i < 6; i++)
            {
                MoveGenerators[i] = moveGeneratorBuilder.Build((PieceType)i);
            }
            LoadFEN(fen);
        }

        public void LoadFEN(string fen)
        {
            fenConverter.Load(fen, this);
        }

        public void Reset()
        {
            Pieces = new ulong[2, 6];
            Occupancy = new ulong[2];
            IrrevState irrevState = new IrrevState(this);
            irrevStates = new List<IrrevState>() { irrevState };
        }

        public PieceType? GetPieceBySquare(int pl, ulong sq)
        {
            for (int i = 0; i < 6; i++)
            {
                if ((Pieces[pl, i] & sq) > 0)
                {
                    return (PieceType)i;
                }
            }
            return null;
        }

        public List<Move> GetMoves(GenerationMode generationMode = GenerationMode.Normal)
        {
            List<Move> moves = new List<Move>();
            foreach (IMoveGenerator generator in MoveGenerators)
            {
                moves.AddRange(generator.GenerateMoves(generationMode));
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

        /// <summary>
        /// Does the current player have a legal move.
        /// </summary>
        /// <returns></returns>
        public bool HasLegalMove()
        {
            foreach (IMoveGenerator generator in MoveGenerators)
            {
                foreach (Move move in generator.GenerateMoves())
                {
                    MakeMove(move);
                    if (move.IsLegal())
                    {
                        UnmakeMove(move);
                        return true;
                    }
                    UnmakeMove(move);
                }
            }
            return false;
        }

        public bool IsCheck()
        {
            IrrevState irrevState = GetIrrevState();
            if (irrevState.IsCheck == null)
            {
                ulong king = Pieces[Turn, (int)PieceType.King];
                irrevState.IsCheck = IsAttacked(Turn, king, false);
            }
            return irrevState.IsCheck.Value;
        }

        /// <summary>
        /// Is the bitboard mask attacked by the player's pieces.
        /// </summary>
        /// <param name="pl">Player</param>
        /// <param name="bb">Bitboard mask</param>
        /// <param name="includeKing">Whether to check attacks by the king</param>
        /// <returns></returns>
        public bool IsAttacked(int pl, ulong bb, bool includeKing = true)
        {
            int max = includeKing ? 6 : 5;
            for (int i = 0; i < max; i++)
            {
                ulong attacks = MoveGenerators[i].GenerateAttacks(1 - pl);
                if ((bb & attacks) > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public IrrevState GetIrrevState()
        {
            return irrevStates.Last();
        }

        public void ChangeTurn()
        {
            Turn = 1 - Turn;
        }

        public void MakeMove(Move move)
        {
            IrrevState irrevState = GetIrrevState().Copy();
            irrevStates.Add(irrevState);
            move.Make();
            ChangeTurn();
            ZobristHashUtils.UpdateTurn(ref GetIrrevState().ZobristHash);
        }

        public void UnmakeMove(Move move)
        {
            move.Unmake();
            irrevStates.RemoveAt(irrevStates.Count - 1);
            ChangeTurn();
        }

        /// <summary>
        /// Gets the result of the game.
        /// For use by the GUI.
        /// </summary>
        /// <returns></returns>
        public GameResult GetResult()
        {
            if (HasLegalMove())
            {
                return GameResult.Ongoing;
            }
            else if (IsCheck())
            {
                return GameResult.Checkmate;
            }
            else
            {
                return GameResult.Stalemate;
            }
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
