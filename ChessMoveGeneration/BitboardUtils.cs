using System;
using System.Collections.Generic;
using System.Text;

namespace ChessMoveGeneration
{
    public static class BitboardUtils
    {
        /// <summary>
        /// Get the bitboard representation of a square
        /// </summary>
        /// <param name="sqInd">Square between 0 (a1) and 63 (h8)</param>
        /// <returns></returns>
        public static ulong GetBitboard(int sqInd)
        {
            ulong bitboard = 1;
            bitboard = bitboard << sqInd;
            return bitboard;
        }

        /// <summary>
        /// Get the square index (0-63) relative to a1
        /// </summary>
        /// <param name="bitboard"></param>
        /// <returns></returns>
        public static int GetSquareIndex(ulong bitboard)
        {
            int index = 0;
            ulong pos = 1;
            while ((bitboard & pos) == 0)
            {
                pos = pos << 1;
                index++;
            }
            return index;
        }

        public static string PositionToString(ulong bitboard)
        {
            int index = GetSquareIndex(bitboard);
            int rank = index / 8;
            int file = index - rank * 8;
            char c = (char)(97 + file);
            return c.ToString() + (rank + 1);
        }

        public static ulong GetLSB(ulong bb)
        {
            return bb & (~bb + 1);
        }
    }
}
