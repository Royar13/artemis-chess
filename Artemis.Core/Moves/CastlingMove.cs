using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.Moves
{
    public class CastlingMove : Move
    {
        /// <summary>
        /// 0=Queenside,1=Kingside
        /// </summary>
        readonly int dir;
        /// <summary>
        /// Bitboard array containing the rook's initial square.
        /// 0=Queenside, 1=Kingside.
        /// </summary>
        readonly ulong[] rookFrom = { 0x1, 0x80 };
        /// <summary>
        /// Bitboard array containing the rook's target square.
        /// 0=Queenside, 1=Kingside.
        /// </summary>
        readonly ulong[] rookTo = { 0x8, 0x20 };
        ulong rookMove;
        readonly string[] notation = { "O-O-O", "O-O" };
        readonly ulong[] castlingPath = { 0x1C, 0x70 };

        public CastlingMove(GameState gameState, ulong from, ulong to) : base(gameState, from, to, PieceType.King)
        {
            dir = to > from ? 1 : 0;
        }

        public override bool IsLegal()
        {
            return !gameState.IsAttacked(1 - gameState.Turn, AdjustForPl(castlingPath[dir], 1 - gameState.Turn));
        }

        public override void Make()
        {
            SetIrrevState();
            MakePieceMovement();
            UpdateCastlingRights();
        }

        protected override void MakePieceMovement()
        {
            base.MakePieceMovement();

            ulong actualRookFrom = AdjustForPl(rookFrom[dir], gameState.Turn);
            ulong actualRookTo = AdjustForPl(rookTo[dir], gameState.Turn);
            rookMove = actualRookFrom | actualRookTo;
            gameState.Pieces[gameState.Turn, (int)PieceType.Rook] ^= rookMove;
            gameState.Occupancy[gameState.Turn] ^= rookMove;
            gameState.ZobristHashUtils.UpdatePiecePos(ref irrevState.ZobristHash, gameState.Turn, PieceType.Rook, actualRookFrom, actualRookTo);
        }

        public override void Unmake()
        {
            base.Unmake();

            gameState.Pieces[1 - gameState.Turn, (int)PieceType.Rook] ^= rookMove;
            gameState.Occupancy[1 - gameState.Turn] ^= rookMove;
        }

        protected override void UpdateCastlingRights()
        {
            DisableCastling(gameState.Turn, 0);
            DisableCastling(gameState.Turn, 1);
        }

        public override bool IsQuiet()
        {
            return true;
        }

        public override GameAction GetAction()
        {
            int rookFromInd = BitboardUtils.BitScanForward(AdjustForPl(rookFrom[dir], gameState.Turn));
            int rookToInd = BitboardUtils.BitScanForward(AdjustForPl(rookTo[dir], gameState.Turn));
            GameAction rookAction = new GameAction(gameState, this, rookFromInd, rookToInd);
            GameAction action = new GameAction(gameState, this, BitboardUtils.BitScanForward(From), BitboardUtils.BitScanForward(To),
                null, null, rookAction);
            return action;
        }

        public override string GetPgnNotation()
        {
            return notation[dir];
        }

        private ulong AdjustForPl(ulong bb, int pl)
        {
            if (pl == 1)
            {
                bb <<= 56;
            }
            return bb;
        }
    }
}
