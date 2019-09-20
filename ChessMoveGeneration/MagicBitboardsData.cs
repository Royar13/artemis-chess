using System;
using System.Collections.Generic;
using System.Text;

namespace ChessMoveGeneration
{
    public class MagicBitboardsData
    {
        public ulong[] RookMasks = new ulong[64];
        public ulong[] BishopMasks = new ulong[64];
        public ulong[] RookMagics = new ulong[64];
        public ulong[] BishopMagics = new ulong[64];
        public ulong[][] RookAttacksTable = new ulong[64][];
        public ulong[][] BishopAttacksTable = new ulong[64][];

        int[] RBits = new int[64] {
          12, 11, 11, 11, 11, 11, 11, 12,
          11, 10, 10, 10, 10, 10, 10, 11,
          11, 10, 10, 10, 10, 10, 10, 11,
          11, 10, 10, 10, 10, 10, 10, 11,
          11, 10, 10, 10, 10, 10, 10, 11,
          11, 10, 10, 10, 10, 10, 10, 11,
          11, 10, 10, 10, 10, 10, 10, 11,
          12, 11, 11, 11, 11, 11, 11, 12
        };

        int[] BBits = new int[64] {
          6, 5, 5, 5, 5, 5, 5, 6,
          5, 5, 5, 5, 5, 5, 5, 5,
          5, 5, 7, 7, 7, 7, 5, 5,
          5, 5, 7, 9, 9, 7, 5, 5,
          5, 5, 7, 9, 9, 7, 5, 5,
          5, 5, 7, 7, 7, 7, 5, 5,
          5, 5, 5, 5, 5, 5, 5, 5,
          6, 5, 5, 5, 5, 5, 5, 6
        };

        public void Initialize()
        {
            for (int i = 0; i < 64; i++)
            {
                ulong[] rookHashTable, bishopHashTable;
                RookMagics[i] = FindMagic(i, true, out rookHashTable);
                BishopMagics[i] = FindMagic(i, false, out bishopHashTable);
                RookAttacksTable[i] = rookHashTable;
                BishopAttacksTable[i] = bishopHashTable;
            }
        }

        public ulong GetAttacks(int sqInd, ulong occupancy, bool rook)
        {
            ulong attacks;
            if (rook)
            {
                ulong hashed = ApplyMagic(occupancy & RookMasks[sqInd], RookMagics[sqInd], RBits[sqInd]);
                attacks = RookAttacksTable[sqInd][hashed];
            }
            else
            {
                ulong hashed = ApplyMagic(occupancy & BishopMasks[sqInd], BishopMagics[sqInd], BBits[sqInd]);
                attacks = BishopAttacksTable[sqInd][hashed];
            }
            return attacks;
        }

        public ulong FindMagic(int sqInd, bool rook, out ulong[] hashTable)
        {
            ulong mask = rook ? GetRookMask(sqInd) : GetBishopMask(sqInd);
            int[] bitsArr = rook ? RBits : BBits;
            int bits = bitsArr[sqInd];
            int combinations = 1 << bits;
            ulong[] blockers = new ulong[combinations];
            ulong[] attacks = new ulong[combinations];
            hashTable = new ulong[combinations];

            for (int i = 0; i < combinations; i++)
            {
                blockers[i] = GetBlockersByIndex(i, mask);
                attacks[i] = rook ? GetRookAtt(sqInd, blockers[i]) : GetBishopAtt(sqInd, blockers[i]);
            }

            for (int k = 0; k < 100000000; k++)
            {
                ulong magic = RandomUInt64FewBits();
                ulong[] attacksTable = new ulong[combinations];
                bool fail = false;
                for (int i = 0; i < combinations && !fail; i++)
                {
                    ulong hashed = ApplyMagic(blockers[i], magic, bits);
                    if (attacksTable[hashed] == 0)
                    {
                        attacksTable[hashed] = attacks[i];
                    }
                    else if (attacksTable[hashed] != attacks[i])
                    {
                        fail = true;
                    }
                }
                if (!fail)
                {
                    hashTable = attacksTable;
                    return magic;
                }
            }
            throw new Exception("Failed to find magic");
        }

        public ulong GetRookMask(int sqInd)
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
            RookMasks[sqInd] = mask;
            return mask;
        }

        public ulong GetBishopMask(int sqInd)
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
            BishopMasks[sqInd] = mask;
            return mask;
        }

        public ulong GetRookAtt(int sqInd, ulong blockers)
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

        public ulong GetBishopAtt(int sqInd, ulong blockers)
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

        public ulong GetBlockersByIndex(int index, ulong mask)
        {
            ulong blockers = 0;
            for (ulong i = 1; mask > 0; i <<= 1)
            {
                ulong lsb = BitboardUtils.GetLSB(mask);
                if (((ulong)index & i) > 0)
                {
                    blockers |= lsb;
                }
                mask ^= lsb;
            }
            return blockers;
        }

        public ulong ApplyMagic(ulong blockers, ulong magic, int bits)
        {
            return (blockers * magic) >> (64 - bits);
        }

        public ulong RandomUInt64FewBits()
        {
            return BitboardUtils.RandomUInt64() & BitboardUtils.RandomUInt64() & BitboardUtils.RandomUInt64();
        }
    }
}
