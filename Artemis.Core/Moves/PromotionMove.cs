using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.Moves
{
    public class PromotionMove : Move
    {
        public PieceType PromotionType { get; }

        public PromotionMove(GameState gameState, ulong from, ulong to, PieceType promotionType) : base(gameState, from, to, PieceType.Pawn)
        {
            this.PromotionType = promotionType;
        }

        public override void Make()
        {
            SetIrrevState();
            MakePieceMovement();
            CalculateCapture();
        }

        public override void Unmake()
        {
            gameState.Pieces[1 - gameState.Turn, (int)MovedPieceType] ^= From;
            gameState.Pieces[1 - gameState.Turn, (int)PromotionType] ^= To;
            gameState.Occupancy[1 - gameState.Turn] ^= From | To;

            CalculateUncapture();
        }

        protected override void MakePieceMovement()
        {
            gameState.Pieces[gameState.Turn, (int)MovedPieceType] ^= From;
            gameState.Pieces[gameState.Turn, (int)PromotionType] ^= To;
            gameState.Occupancy[gameState.Turn] ^= From | To;
            gameState.ZobristHashUtils.UpdatePiece(ref irrevState.ZobristHash, gameState.Turn, MovedPieceType, From);
            gameState.ZobristHashUtils.UpdatePiece(ref irrevState.ZobristHash, gameState.Turn, PromotionType, To);
        }

        public override bool IsQuiet()
        {
            return false;
        }

        public override GameAction GetAction()
        {
            int? capture = null;
            if ((gameState.Occupancy[1 - gameState.Turn] & To) > 0)
            {
                capture = BitboardUtils.BitScanForward(To);
            }
            GameAction action = new GameAction(gameState, this, BitboardUtils.BitScanForward(From), BitboardUtils.BitScanForward(To), capture, PromotionType);
            return action;
        }

        public override string GetPgnNotation()
        {
            string str = base.GetPgnNotation();
            str += "=" + PromotionType.ToNotation();
            return str;
        }

        public override bool Equals(object obj)
        {
            if (obj is PromotionMove)
            {
                PromotionMove otherMove = (PromotionMove)obj;
                return From == otherMove.From && To == otherMove.To && PromotionType == otherMove.PromotionType;
            }
            else
            {
                return base.Equals(obj);
            }
        }

        public override int GetHashCode()
        {
            var hashCode = -1255755712;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + PromotionType.GetHashCode();
            return hashCode;
        }
    }
}
