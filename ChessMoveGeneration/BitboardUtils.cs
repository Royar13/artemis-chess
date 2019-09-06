using System;
using System.Collections.Generic;
using System.Text;

namespace ChessMoveGeneration
{
    static class BitboardUtils
    {
        /// <summary>
        /// Get the bitboard representation of a square
        /// </summary>
        /// <param name="square">Square between 0 (a1) and 63 (h8)</param>
        /// <returns></returns>
        public static UInt64 GetBitboard(int square)
        {
            UInt64 bitboard = 1;
            bitboard = bitboard << square;
            return bitboard;
        }

        /// <summary>
        /// Get the square index (0-63) relative to a1
        /// </summary>
        /// <param name="bitboard"></param>
        /// <returns></returns>
        public static int GetSquareIndex(UInt64 bitboard)
        {
            int index = 0;
            UInt64 pos = 1;
            while ((bitboard & pos) == 0)
            {
                pos = pos << 1;
                index++;
            }
            return index;
        }

        public static string PositionToString(UInt64 bitboard)
        {
            int index = GetSquareIndex(bitboard);
            int rank = index / 8;
            int file = index - rank * 8;
            char c = (char)(97 + file);
            return c.ToString() + (rank + 1);
        }
    }
}
