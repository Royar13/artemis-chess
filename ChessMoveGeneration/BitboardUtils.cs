using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core
{
    public static class BitboardUtils
    {
        private static Random rnd = new Random();
        private readonly static int[] index64 = {
            0,  1, 48,  2, 57, 49, 28,  3,
           61, 58, 50, 42, 38, 29, 17,  4,
           62, 55, 59, 36, 53, 51, 43, 22,
           45, 39, 33, 30, 24, 18, 12,  5,
           63, 47, 56, 27, 60, 41, 37, 16,
           54, 35, 52, 21, 44, 32, 23, 11,
           46, 26, 40, 15, 34, 20, 31, 10,
           25, 14, 19,  9, 13,  8,  7,  6
        };

        public const ulong NOT_A_FILE = 0xFEFEFEFEFEFEFEFE;
        public const ulong NOT_H_FILE = 0x7F7F7F7F7F7F7F7F;
        public const ulong NOT_AB_FILES = 0xFCFCFCFCFCFCFCFC;
        public const ulong NOT_GH_FILES = 0x3F3F3F3F3F3F3F3F;
        public static readonly ulong[] SECOND_RANK = { 0xFF00, 0x00FF000000000000 };

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

        /// <summary>
        /// Gets the least significant bit
        /// </summary>
        /// <param name="bb"></param>
        /// <returns></returns>
        public static ulong GetLSB(ulong bb)
        {
            return bb & (~bb + 1);
        }

        public static ulong RandomUInt64()
        {
            var buffer = new byte[sizeof(ulong)];
            rnd.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }

        /// <summary>
        /// Gets the index of the least significant bit
        /// </summary>
        /// <param name="bb"></param>
        /// <returns></returns>
        public static int BitScanForward(ulong bb)
        {
            ulong lsb = GetLSB(bb);
            return BitScanForwardSingleBit(lsb);
        }

        private static int BitScanForwardSingleBit(ulong bit)
        {
            ulong debruijn64 = 0x03f79d71b4cb0a89;
            return index64[(bit * debruijn64) >> 58];
        }

        public static int PopLSB(ref ulong bb)
        {
            ulong lsb = GetLSB(bb);
            bb ^= lsb;
            return BitScanForwardSingleBit(lsb);
        }

        public static ulong OneEast(ulong bb)
        {
            return (bb << 1) & NOT_A_FILE;
        }

        public static ulong OneWest(ulong bb)
        {
            return (bb >> 1) & NOT_H_FILE;
        }

        public static int GetFile(int sqInd)
        {
            return sqInd % 8;
        }

        public static int GetRank(int sqInd)
        {
            return sqInd / 8;
        }
    }
}
