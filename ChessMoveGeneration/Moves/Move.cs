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

        private void CapturePiece(ulong sq, IrrevState state)
        {
            int pl = 1 - gameState.Turn;
            gameState.Occupancy[pl] ^= sq;
            for (int i = 0; i < 5; i++)
            {
                if ((gameState.Pieces[pl, i] & sq) > 0)
                {
                    capturedPieceType = (PieceType)i;
                    gameState.Pieces[pl, i] ^= sq;
                    break;
                }
            }
        }
    }
}
