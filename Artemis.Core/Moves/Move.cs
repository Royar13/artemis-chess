using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Artemis.Core.Moves
{
    public class Move
    {
        protected GameState gameState;
        protected IrrevState irrevState;
        public readonly ulong From;
        public readonly ulong To;
        public readonly PieceType MovedPieceType;
        protected PieceType? capturedPieceType;

        private readonly ulong[] startingRookSquare = { 0x0100000000000001, 0x8000000000000080 };

        public Move(GameState gameState, ulong from, ulong to, PieceType movedPieceType)
        {
            this.gameState = gameState;
            From = from;
            To = to;
            MovedPieceType = movedPieceType;
        }

        public virtual void Make()
        {
            SetIrrevState();
            MakePieceMovement();
            UpdateCastlingRights();
            UpdateEnPassant();
            CalculateCapture();
        }

        public virtual void Unmake()
        {
            ulong move = From | To;
            gameState.Pieces[1 - gameState.Turn, (int)MovedPieceType] ^= move;
            gameState.Occupancy[1 - gameState.Turn] ^= move;
            CalculateUncapture();
        }

        protected void SetIrrevState()
        {
            irrevState = gameState.GetIrrevState();
        }

        protected virtual void MakePieceMovement()
        {
            ulong move = From | To;
            gameState.Pieces[gameState.Turn, (int)MovedPieceType] ^= move;
            gameState.Occupancy[gameState.Turn] ^= move;
            gameState.ZobristHashUtils.UpdatePiecePos(ref irrevState.ZobristHash, gameState.Turn, MovedPieceType, From, To);
        }

        protected virtual void UpdateCastlingRights()
        {
            if (MovedPieceType == PieceType.King)
            {
                DisableCastling(gameState.Turn, 0);
                DisableCastling(gameState.Turn, 1);
            }
            else if (MovedPieceType == PieceType.Rook)
            {
                for (int i = 0; i <= 1; i++)
                {
                    if ((From & startingRookSquare[i] & BitboardUtils.FIRST_RANK[gameState.Turn]) > 0)
                    {
                        DisableCastling(gameState.Turn, i);
                        break;
                    }
                }
            }
        }

        protected void UpdateEnPassant()
        {
            if (MovedPieceType == PieceType.Pawn)
            {
                if (gameState.Turn == 0 && (To >> 16) == From)
                {
                    irrevState.EnPassantCapture = To >> 8;
                    gameState.ZobristHashUtils.UpdateEnPassant(ref irrevState.ZobristHash, irrevState.EnPassantCapture);
                }
                else if (gameState.Turn == 1 && (To << 16) == From)
                {
                    irrevState.EnPassantCapture = To << 8;
                    gameState.ZobristHashUtils.UpdateEnPassant(ref irrevState.ZobristHash, irrevState.EnPassantCapture);
                }
            }
        }

        /// <summary>
        /// Checks that the move didn't put the king in check.
        /// Should be called after the move is made.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsLegal()
        {
            ulong king = gameState.Pieces[1 - gameState.Turn, (int)PieceType.King];
            return !gameState.IsAttacked(1 - gameState.Turn, king);
        }

        public bool IsCapture()
        {
            return (gameState.Occupancy[1 - gameState.Turn] & To) > 0;
        }

        public virtual PieceType GetCapturedPieceType()
        {
            if (capturedPieceType == null)
            {
                capturedPieceType = gameState.GetPieceBySquare(1 - gameState.Turn, To);
            }
            return capturedPieceType.Value;
        }

        protected virtual void CalculateCapture()
        {
            if (IsCapture())
            {
                int pl = 1 - gameState.Turn;
                gameState.Occupancy[pl] ^= To;
                GetCapturedPieceType();
                gameState.Pieces[pl, (int)capturedPieceType] ^= To;
                gameState.ZobristHashUtils.UpdatePiece(ref irrevState.ZobristHash, pl, capturedPieceType.Value, To);

                //disable castling if necessary
                if (capturedPieceType == PieceType.Rook)
                {
                    for (int i = 0; i <= 1; i++)
                    {
                        if ((To & startingRookSquare[i] & BitboardUtils.FIRST_RANK[pl]) > 0)
                        {
                            DisableCastling(pl, i);
                            break;
                        }
                    }
                }
            }
        }

        protected virtual void CalculateUncapture()
        {
            if (capturedPieceType != null)
            {
                gameState.Pieces[gameState.Turn, (int)capturedPieceType] |= To;
                gameState.Occupancy[gameState.Turn] |= To;
            }
        }

        protected void DisableCastling(int pl, int side)
        {
            if (irrevState.CastlingAllowed[pl, side])
            {
                irrevState.CastlingAllowed[pl, side] = false;
                gameState.ZobristHashUtils.DisableCastling(ref irrevState.ZobristHash, pl, side);
            }
        }

        /// <summary>
        /// Should be called after the move is made.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsQuiet()
        {
            return (gameState.Occupancy[gameState.Turn] & To) == 0;
        }

        /// <summary>
        /// Returns a GameAction object which describes the move, for use by the GUI.
        /// It should be called before the move is made.
        /// </summary>
        /// <returns></returns>
        public virtual GameAction GetAction()
        {
            int? capture = null;
            if ((gameState.Occupancy[1 - gameState.Turn] & To) > 0)
            {
                capture = BitboardUtils.BitScanForward(To);
            }
            GameAction action = new GameAction(gameState, this, BitboardUtils.BitScanForward(From), BitboardUtils.BitScanForward(To), capture);
            return action;
        }

        public override string ToString()
        {
            string str = From.PosToString() + To.PosToString();
            return str;
        }

        /// <summary>
        /// Should be called before the move is made.
        /// </summary>
        /// <returns></returns>
        public virtual string GetPgnNotation()
        {
            char pieceNotation = MovedPieceType.ToNotation();
            StringBuilder builder = new StringBuilder();
            builder.Append(MovedPieceType == PieceType.Pawn ? "" : pieceNotation.ToString());
            if (MovedPieceType == PieceType.Rook || MovedPieceType == PieceType.Knight)
            {
                ulong otherPiece = gameState.Pieces[gameState.Turn, (int)MovedPieceType] ^ From;
                if (otherPiece != 0)
                {
                    Move collisionMove = gameState.MoveGenerators[(int)MovedPieceType].GenerateMoves().FirstOrDefault(m => m.From == otherPiece && m.To == To);
                    if (collisionMove != null)
                    {
                        string fromSq = From.PosToString();
                        if (fromSq[0] != otherPiece.PosToString()[0])
                        {
                            builder.Append(fromSq[0]);
                        }
                        else
                        {
                            builder.Append(fromSq[1]);
                        }
                    }
                }
            }
            if (IsCapture())
            {
                if (MovedPieceType == PieceType.Pawn)
                {
                    builder.Append(From.PosToString()[0]);
                }
                builder.Append('x');
            }
            builder.Append(To.PosToString());
            return builder.ToString();
        }

        public override bool Equals(object obj)
        {
            Move otherMove = (Move)obj;
            return From == otherMove.From && To == otherMove.To;
        }

        public void SetGameState(GameState gameState)
        {
            this.gameState = gameState;
        }
    }
}
