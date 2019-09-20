using System;
using System.Collections.Generic;
using System.Text;

namespace ChessMoveGeneration.Moves
{
    public class Move
    {
        private GameState gameState;
        public readonly ulong From;
        public readonly ulong To;
        public readonly PieceType MovedPieceType;
        private PieceType? capturedPieceType;

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
            if ((gameState.Occupancy[1 - gameState.Turn] & To) > 0)
            {
                CapturePiece(To, gameState.GetIrrevState());
            }
        }

        public virtual void Unmake()
        {
            ulong move = From | To;
            gameState.Pieces[gameState.Turn, (int)MovedPieceType] ^= move;
            gameState.Occupancy[gameState.Turn] ^= move;
            if (capturedPieceType != null)
            {
                gameState.Pieces[1 - gameState.Turn, (int)capturedPieceType] |= To;
                gameState.Occupancy[1 - gameState.Turn] |= To;
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

        private void CapturePiece(ulong sq, IrrevState state)
        {
            int pl = 1 - gameState.Turn;
            gameState.Occupancy[pl] ^= sq;
            capturedPieceType = gameState.GetPieceBySquare(pl, sq);
            gameState.Pieces[pl, (int)capturedPieceType] ^= sq;
        }

        /// <summary>
        /// Returns a GameAction object which describes the move, for use by the GUI.
        /// It should be called before the move is made.
        /// </summary>
        /// <returns></returns>
        public GameAction GetAction()
        {
            int? capture = null;
            PieceType? captureType = null;
            if ((gameState.Occupancy[1 - gameState.Turn] & To) > 0)
            {
                capture = BitboardUtils.BitScanForward(To);
                captureType = gameState.GetPieceBySquare(1 - gameState.Turn, To);
            }
            GameAction action = new GameAction(BitboardUtils.BitScanForward(From), BitboardUtils.BitScanForward(To), capture, captureType);
            return action;
        }
    }
}
