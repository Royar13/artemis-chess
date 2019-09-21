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
        /// Bitboard array containing the rook movement (for both colors):
        /// 0=Queenside castling, 1=Kingside castling.
        /// </summary>
        readonly ulong[] rookMoves = { 0x0900000000000009, 0xA0000000000000A0 };
        ulong rookMove;
        readonly string[] notation = { "O-O-O", "O-O" };
        readonly ulong[] castlingPath = { 0x1C0000000000001C, 0x7000000000000070 };

        public CastlingMove(GameState gameState, ulong from, ulong to) : base(gameState, from, to, PieceType.King)
        {
            dir = to > from ? 1 : 0;
        }

        public override bool IsLegal()
        {
            return !gameState.IsAttacked(1 - gameState.Turn, castlingPath[dir] & BitboardUtils.FIRST_RANK[1 - gameState.Turn]);
        }

        public override void Make()
        {
            base.Make();

            rookMove = rookMoves[dir] & BitboardUtils.FIRST_RANK[gameState.Turn];
            gameState.Pieces[gameState.Turn, (int)PieceType.Rook] ^= rookMove;
            gameState.Occupancy[gameState.Turn] ^= rookMove;
        }

        public override void Unmake()
        {
            base.Unmake();

            gameState.Pieces[1 - gameState.Turn, (int)PieceType.Rook] ^= rookMove;
            gameState.Occupancy[1 - gameState.Turn] ^= rookMove;
        }

        protected override void UpdateCastlingRights()
        {
            IrrevState irrevState = gameState.GetIrrevState();
            irrevState.CastlingAllowed[gameState.Turn, 0] = false;
            irrevState.CastlingAllowed[gameState.Turn, 1] = false;
        }

        public override GameAction GetAction()
        {
            ulong rook = rookMoves[dir] & BitboardUtils.FIRST_RANK[gameState.Turn];
            int rookFrom = BitboardUtils.BitScanForward(rook & gameState.Occupancy[gameState.Turn]);
            int rookTo = BitboardUtils.BitScanForward(rook & ~gameState.Occupancy[gameState.Turn]);
            GameAction rookAction = new GameAction(gameState, this, rookFrom, rookTo);
            GameAction action = new GameAction(gameState, this, BitboardUtils.BitScanForward(From), BitboardUtils.BitScanForward(To),
                null, null, rookAction);
            return action;
        }

        public override string ToString()
        {
            return notation[dir];
        }
    }
}
