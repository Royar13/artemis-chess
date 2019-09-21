using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.Moves
{
    public class Move
    {
        protected GameState gameState;
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
            ulong move = From | To;
            gameState.Pieces[gameState.Turn, (int)MovedPieceType] ^= move;
            gameState.Occupancy[gameState.Turn] ^= move;

            UpdateCastlingRights();

            if ((gameState.Occupancy[1 - gameState.Turn] & To) > 0)
            {
                CapturePiece(To, gameState.GetIrrevState());
            }
        }

        public virtual void Unmake()
        {
            ulong move = From | To;
            gameState.Pieces[1 - gameState.Turn, (int)MovedPieceType] ^= move;
            gameState.Occupancy[1 - gameState.Turn] ^= move;
            if (capturedPieceType != null)
            {
                gameState.Pieces[gameState.Turn, (int)capturedPieceType] |= To;
                gameState.Occupancy[gameState.Turn] |= To;
            }
        }

        protected virtual void UpdateCastlingRights()
        {
            if (MovedPieceType == PieceType.King)
            {
                IrrevState irrevState = gameState.GetIrrevState();
                irrevState.CastlingAllowed[gameState.Turn, 0] = false;
                irrevState.CastlingAllowed[gameState.Turn, 1] = false;
            }
            else if (MovedPieceType == PieceType.Rook)
            {
                IrrevState irrevState = gameState.GetIrrevState();
                for (int i = 0; i <= 1; i++)
                {
                    if ((From & startingRookSquare[i] & BitboardUtils.FIRST_RANK[gameState.Turn]) > 0)
                    {
                        irrevState.CastlingAllowed[gameState.Turn, i] = false;
                        break;
                    }
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

        private void CapturePiece(ulong sq, IrrevState irrevState)
        {
            int pl = 1 - gameState.Turn;
            gameState.Occupancy[pl] ^= sq;
            capturedPieceType = gameState.GetPieceBySquare(pl, sq);
            gameState.Pieces[pl, (int)capturedPieceType] ^= sq;

            //disable castling if necessary
            if (capturedPieceType == PieceType.Rook)
            {
                for (int i = 0; i <= 1; i++)
                {
                    if ((sq & startingRookSquare[i] & BitboardUtils.FIRST_RANK[pl]) > 0)
                    {
                        irrevState.CastlingAllowed[pl, i] = false;
                        break;
                    }
                }
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
