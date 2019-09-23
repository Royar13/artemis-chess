using System;
using System.Collections.Generic;
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

        protected virtual void CalculateCapture()
        {
            if ((gameState.Occupancy[1 - gameState.Turn] & To) > 0)
            {
                int pl = 1 - gameState.Turn;
                gameState.Occupancy[pl] ^= To;
                capturedPieceType = gameState.GetPieceBySquare(pl, To);
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

        /// <summary>
        /// Should be called after the move is made.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            char pieceNotation = MovedPieceType.ToNotation();
            string str = pieceNotation == 'P' ? "" : pieceNotation.ToString();
            if (capturedPieceType != null)
            {
                if (pieceNotation == 'P')
                {
                    str += From.PosToString()[0];
                }
                str += "x";
            }
            str += To.PosToString();
            /*if (gameState.IsCheck())
            {
                str += "+";
            }*/
            return str;
        }
    }
}
