using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.Moves
{
    public class PromotionMove : Move
    {
        PieceType promotionType;

        public PromotionMove(GameState gameState, ulong from, ulong to, PieceType promotionType) : base(gameState, from, to, PieceType.Pawn)
        {
            this.promotionType = promotionType;
        }

        public override void Make()
        {
            gameState.Pieces[gameState.Turn, (int)MovedPieceType] ^= From;
            gameState.Pieces[gameState.Turn, (int)promotionType] ^= To;
            gameState.Occupancy[gameState.Turn] ^= From | To;

            CalculateCapture();
        }

        public override void Unmake()
        {
            gameState.Pieces[1 - gameState.Turn, (int)MovedPieceType] ^= From;
            gameState.Pieces[1 - gameState.Turn, (int)promotionType] ^= To;
            gameState.Occupancy[1 - gameState.Turn] ^= From | To;

            CalculateUncapture();
        }

        public override GameAction GetAction()
        {
            int? capture = null;
            if ((gameState.Occupancy[1 - gameState.Turn] & To) > 0)
            {
                capture = BitboardUtils.BitScanForward(To);
            }
            GameAction action = new GameAction(gameState, this, BitboardUtils.BitScanForward(From), BitboardUtils.BitScanForward(To), capture, promotionType);
            return action;
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
