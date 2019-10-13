using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core
{
    public static class BitboardUtils
    {
#if DEBUG
        private static Random rnd = new Random(42);
#else
        private static Random rnd = new Random();
#endif
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

        public const ulong A_FILE = 0x0101010101010101;
        public const ulong NOT_A_FILE = 0xFEFEFEFEFEFEFEFE;
        public const ulong NOT_H_FILE = 0x7F7F7F7F7F7F7F7F;
        public const ulong NOT_AB_FILES = 0xFCFCFCFCFCFCFCFC;
        public const ulong NOT_GH_FILES = 0x3F3F3F3F3F3F3F3F;
        public const ulong CENTER_MASK = 0x0000001818000000;
        public const ulong EXTENDED_CENTER_MASK = 0x00003C3C3C3C0000;
        public static readonly ulong[] FIRST_RANK = { 0xFF, 0xFF00000000000000 };
        public static readonly ulong[] SECOND_RANK = { 0xFF00, 0x00FF000000000000 };

        /// <summary>
        /// Get the bitboard representation of a square
        /// </summary>
        /// <param name="sqInd">Square between 0 (a1) and 63 (h8)</param>
        /// <returns></returns>
        public static ulong GetBitboard(int sqInd)
        {
            return 1UL << sqInd;
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

        /// <summary>
        /// Gets the most significant bit
        /// </summary>
        /// <param name="bb"></param>
        /// <returns></returns>
        public static ulong GetMSB(ulong bb)
        {
            bb |= bb >> 1;
            bb |= bb >> 2;
            bb |= bb >> 4;
            bb |= bb >> 8;
            bb |= bb >> 16;
            bb |= bb >> 32;
            bb = bb & ~(bb >> 1);
            return bb;
        }

        public static ulong RandomBitstring()
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
            return GetSquareInd(lsb);
        }

        /// <summary>
        /// Gets the index of the most significant bit
        /// </summary>
        /// <param name="bb"></param>
        /// <returns></returns>
        public static int BitScanBackward(ulong bb)
        {
            ulong msb = GetMSB(bb);
            return GetSquareInd(msb);
        }

        public static int GetSquareInd(ulong bit)
        {
            ulong debruijn64 = 0x03f79d71b4cb0a89;
            return index64[(bit * debruijn64) >> 58];
        }

        public static int PopLSB(ref ulong bb)
        {
            ulong lsb = GetLSB(bb);
            bb ^= lsb;
            return GetSquareInd(lsb);
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

        public static int SparsePopcount(ulong bb)
        {
            int count = 0;
            while (bb > 0)
            {
                bb &= bb - 1;
                count++;
            }
            return count;
        }

        public static int Popcount(ulong bb)
        {
            bb = bb - ((bb >> 1) & 0x5555555555555555UL);
            bb = (bb & 0x3333333333333333UL) + ((bb >> 2) & 0x3333333333333333UL);
            return (int)(unchecked(((bb + (bb >> 4)) & 0xF0F0F0F0F0F0F0FUL) * 0x101010101010101UL) >> 56);
        }

        public static ulong GetFileMask(int index)
        {
            return A_FILE << index;
        }

        public static int GetLeastAdvanced(int pl, ulong pieces)
        {
            return pl == 0 ? BitScanForward(pieces) : BitScanBackward(pieces);
        }

        public static int GetMostAdvanced(int pl, ulong pieces)
        {
            return pl == 0 ? BitScanBackward(pieces) : BitScanForward(pieces);
        }

        public static int DistanceToCenter(int sq)
        {
            int file = GetFile(sq);
            int rank = GetRank(sq);
            int fileDistance = (int)Math.Abs(3.5 - file);
            int rankDistance = (int)Math.Abs(3.5 - rank);
            return Math.Max(fileDistance, rankDistance);
        }

        public static int Distance(int sq1, int sq2)
        {
            int file1 = GetFile(sq1);
            int rank1 = GetRank(sq1);
            int file2 = GetFile(sq2);
            int rank2 = GetRank(sq2);
            int distance = Math.Abs(file2 - file1) + Math.Abs(rank2 - rank1);
            return distance;
        }

        public static int MirrorRank(int sq)
        {
            int file = GetFile(sq);
            int mirroredSq = 56 - sq + 2 * file;
            return mirroredSq;
        }
    }
}
