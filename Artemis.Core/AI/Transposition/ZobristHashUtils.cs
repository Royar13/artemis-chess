using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.AI.Transposition
{
    public class ZobristHashUtils
    {
        private ulong[,] piecePosBitstring = new ulong[12, 64];
        private ulong blackTurnBitstring;
        private ulong[,] castlingBitstring = new ulong[2, 2];
        private ulong[] enPassantBitstring = new ulong[8];

        public ZobristHashUtils()
        {
            Initialize();
        }

        private int GetPieceIndex(int pl, PieceType pieceType)
        {
            return pl * 6 + (int)pieceType;
        }

        private void Initialize()
        {
            for (int i = 0; i < 12; i++)
            {
                for (int j = 0; j < 64; j++)
                {
                    piecePosBitstring[i, j] = BitboardUtils.RandomBitstring();
                }
            }
            blackTurnBitstring = BitboardUtils.RandomBitstring();
            for (int pl = 0; pl < 2; pl++)
            {
                castlingBitstring[pl, 0] = BitboardUtils.RandomBitstring();
                castlingBitstring[pl, 1] = BitboardUtils.RandomBitstring();
            }
            for (int file = 0; file < 8; file++)
            {
                enPassantBitstring[file] = BitboardUtils.RandomBitstring();
            }
        }

        public ulong GenerateHash(GameState gameState)
        {
            ulong hash = 0;
            for (int pl = 0; pl < 2; pl++)
            {
                for (int i = 0; i < 6; i++)
                {
                    int pInd = GetPieceIndex(pl, (PieceType)i);
                    ulong pieces = gameState.Pieces[pl, i];
                    while (pieces > 0)
                    {
                        int sqInd = BitboardUtils.PopLSB(ref pieces);
                        hash ^= piecePosBitstring[pInd, sqInd];
                    }
                }
            }

            IrrevState irrevState = gameState.GetIrrevState();
            for (int pl = 0; pl < 2; pl++)
            {
                if (!irrevState.CastlingAllowed[pl, 0])
                {
                    DisableCastling(ref hash, pl, 0);
                }
                if (!irrevState.CastlingAllowed[pl, 1])
                {
                    DisableCastling(ref hash, pl, 1);
                }
            }
            if (irrevState.EnPassantCapture != 0)
            {
                UpdateEnPassant(ref hash, irrevState.EnPassantCapture);
            }
            return hash;
        }

        /// <summary>
        /// Some data, like En Passant right, needs resetting before a new move is applied
        /// </summary>
        /// <returns></returns>
        public void ResetHashBeforeMove(ref ulong hash, IrrevState irrevState)
        {
            if (irrevState.EnPassantCapture != 0)
            {
                UpdateEnPassant(ref hash, irrevState.EnPassantCapture);
            }
        }

        public void UpdatePiecePos(ref ulong hash, int pl, PieceType pieceType, ulong from, ulong to)
        {
            int fromInd = BitboardUtils.BitScanForward(from);
            int toInd = BitboardUtils.BitScanForward(to);
            int pInd = GetPieceIndex(pl, pieceType);
            hash ^= piecePosBitstring[pInd, fromInd];
            hash ^= piecePosBitstring[pInd, toInd];
        }

        /// <summary>
        /// Adds or removes a piece on the given position, depending on whether there's already a piece there
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="pl"></param>
        /// <param name="pieceType"></param>
        /// <param name="pos"></param>
        public void UpdatePiece(ref ulong hash, int pl, PieceType pieceType, ulong pos)
        {
            int sqInd = BitboardUtils.BitScanForward(pos);
            int pInd = GetPieceIndex(pl, pieceType);
            hash ^= piecePosBitstring[pInd, sqInd];
        }

        public void DisableCastling(ref ulong hash, int pl, int side)
        {
            hash ^= castlingBitstring[pl, side];
        }

        public void UpdateEnPassant(ref ulong hash, ulong enPassantCapture)
        {
            int file = BitboardUtils.GetFile(BitboardUtils.BitScanForward(enPassantCapture));
            hash ^= enPassantBitstring[file];
        }

        public void UpdateTurn(ref ulong hash)
        {
            hash ^= blackTurnBitstring;
        }
    }
}
