using System;
using System.Collections.Generic;
using System.Text;

namespace ChessMoveGeneration
{
    public class MagicBitboardsData
    {
        public ulong[] Magics = new ulong[64];

        public void Initialize()
        {
            for (int i = 0; i < 64; i++)
            {
                FindMagic(i);
            }
        }

        public void FindMagic(int sqInd)
        {
            ulong rmask = GetRookMask(sqInd);
            ulong bmask = GetBishopMask(sqInd);
        }

        public ulong GetRookMask(int sqInd)
        {
            ulong mask = 0;
            int file = sqInd % 8;
            int rank = sqInd / 8;
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

        public ulong GetBishopMask(int sqInd)
        {
            ulong mask = 0;
            int file = sqInd % 8;
            int rank = sqInd / 8;
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
    }
}
