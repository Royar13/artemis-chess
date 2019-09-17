﻿using System;
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

        public ulong GetRookAtt(int sqInd, ulong blockers)
        {
            ulong att = 0;
            int file = sqInd % 8;
            int rank = sqInd / 8;
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

        public ulong GetBishopAtt(int sqInd, ulong blockers)
        {
            ulong att = 0;
            int file = sqInd % 8;
            int rank = sqInd / 8;
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

        public ulong GetBlockersByIndex(ulong index, ulong mask)
        {
            ulong blockers = 0;
            for (ulong i = 1; mask > 0; i <<= 1)
            {
                ulong lsb = BitboardUtils.GetLSB(mask);
                if ((index & i) > 0)
                {
                    blockers |= lsb;
                }
                mask ^= lsb;
            }
            return blockers;
        }
    }
}
