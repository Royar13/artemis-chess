using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.Moves.MagicBitboards
{
    public static class SlidingPieceUtils
    {
        public static ulong GetRookMask(int sqInd)
        {
            ulong mask = 0;
            int file = BitboardUtils.GetFile(sqInd);
            int rank = BitboardUtils.GetRank(sqInd);
            for (int r = rank + 1; r <= 6; r++)
            {
                mask |= (ulong)1 << (r * 8 + file);
            }
            for (int r = rank - 1; r >= 1; r--)
            {
                mask |= (ulong)1 << (r * 8 + file);
            }
            for (int f = file + 1; f <= 6; f++)
            {
                mask |= (ulong)1 << (rank * 8 + f);
            }
            for (int f = file - 1; f >= 1; f--)
            {
                mask |= (ulong)1 << (rank * 8 + f);
            }
            return mask;
        }

        public static ulong GetBishopMask(int sqInd)
        {
            ulong mask = 0;
            int file = BitboardUtils.GetFile(sqInd);
            int rank = BitboardUtils.GetRank(sqInd);
            for (int f = file + 1, r = rank + 1; f <= 6 && r <= 6; f++, r++)
            {
                mask |= (ulong)1 << (r * 8 + f);
            }
            for (int f = file - 1, r = rank + 1; f >= 1 && r <= 6; f--, r++)
            {
                mask |= (ulong)1 << (r * 8 + f);
            }
            for (int f = file + 1, r = rank - 1; f <= 6 && r >= 1; f++, r--)
            {
                mask |= (ulong)1 << (r * 8 + f);
            }
            for (int f = file - 1, r = rank - 1; f >= 1 && r >= 1; f--, r--)
            {
                mask |= (ulong)1 << (r * 8 + f);
            }
            return mask;
        }

        public static ulong GetRookAtt(int sqInd, ulong blockers)
        {
            ulong att = 0;
            int file = BitboardUtils.GetFile(sqInd);
            int rank = BitboardUtils.GetRank(sqInd);
            for (int r = rank + 1; r <= 7; r++)
            {
                ulong pos = (ulong)1 << (r * 8 + file);
                att |= pos;
                if ((blockers & pos) > 0) break;
            }
            for (int r = rank - 1; r >= 0; r--)
            {
                ulong pos = (ulong)1 << (r * 8 + file);
                att |= pos;
                if ((blockers & pos) > 0) break;
            }
            for (int f = file + 1; f <= 7; f++)
            {
                ulong pos = (ulong)1 << (rank * 8 + f);
                att |= pos;
                if ((blockers & pos) > 0) break;
            }
            for (int f = file - 1; f >= 0; f--)
            {
                ulong pos = (ulong)1 << (rank * 8 + f);
                att |= pos;
                if ((blockers & pos) > 0) break;
            }
            return att;
        }

        public static ulong GetBishopAtt(int sqInd, ulong blockers)
        {
            ulong att = 0;
            int file = BitboardUtils.GetFile(sqInd);
            int rank = BitboardUtils.GetRank(sqInd);
            for (int f = file + 1, r = rank + 1; f <= 7 && r <= 7; f++, r++)
            {
                ulong pos = (ulong)1 << (r * 8 + f);
                att |= pos;
                if ((blockers & pos) > 0) break;
            }
            for (int f = file + 1, r = rank - 1; f <= 7 && r >= 0; f++, r--)
            {
                ulong pos = (ulong)1 << (r * 8 + f);
                att |= pos;
                if ((blockers & pos) > 0) break;
            }
            for (int f = file - 1, r = rank + 1; f >= 0 && r <= 7; f--, r++)
            {
                ulong pos = (ulong)1 << (r * 8 + f);
                att |= pos;
                if ((blockers & pos) > 0) break;
            }
            for (int f = file - 1, r = rank - 1; f >= 0 && r >= 0; f--, r--)
            {
                ulong pos = (ulong)1 << (r * 8 + f);
                att |= pos;
                if ((blockers & pos) > 0) break;
            }
            return att;
        }
    }
}
