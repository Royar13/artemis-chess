using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.Moves
{
    public class EnPassantMove : Move
    {
        ulong enPassantCapture;
        public EnPassantMove(GameState gameState, ulong from, ulong to) : base(gameState, from, to, PieceType.Pawn)
        {
        }

        protected override void CalculateCapture()
        {
            enPassantCapture = GetEnPassantTarget();
            gameState.Occupancy[1 - gameState.Turn] ^= enPassantCapture;
            capturedPieceType = PieceType.Pawn;
            gameState.Pieces[1 - gameState.Turn, (int)capturedPieceType] ^= enPassantCapture;
        }

        protected override void CalculateUncapture()
        {
            gameState.Pieces[gameState.Turn, (int)capturedPieceType] |= enPassantCapture;
            gameState.Occupancy[gameState.Turn] |= enPassantCapture;
        }

        private ulong GetEnPassantTarget()
        {
            return gameState.Turn == 0 ? To >> 8 : To << 8;
        }

        public override GameAction GetAction()
        {
            int target = BitboardUtils.BitScanForward(GetEnPassantTarget());
            GameAction action = new GameAction(gameState, this, BitboardUtils.BitScanForward(From), BitboardUtils.BitScanForward(To), target);
            return action;
        }
    }
}
