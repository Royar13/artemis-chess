using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.Moves.PregeneratedAttacks
{
    public class PregeneratedAttacksData
    {
        private MagicBitboardsData magic = new MagicBitboardsData();
        private ulong[] knightAttacks = new ulong[64];
        private ulong[] kingAttacks = new ulong[64];
        public bool IsInitialized { get; private set; } = false;

        public void Initialize()
        {
            magic.Initialize();
            for (int i = 0; i < 64; i++)
            {
                knightAttacks[i] = CalculateKnightAttacks(i);
                kingAttacks[i] = CalculateKingAttacks(i);
            }
            IsInitialized = true;
        }

        private ulong CalculateKnightAttacks(int sqInd)
        {
            ulong sq = BitboardUtils.GetBitboard(sqInd);
            ulong attacks = 0;
            attacks |= sq << 17 & BitboardUtils.NOT_A_FILE;
            attacks |= sq << 10 & BitboardUtils.NOT_AB_FILES;
            attacks |= sq >> 6 & BitboardUtils.NOT_AB_FILES;
            attacks |= sq >> 15 & BitboardUtils.NOT_A_FILE;
            attacks |= sq << 15 & BitboardUtils.NOT_H_FILE;
            attacks |= sq << 6 & BitboardUtils.NOT_GH_FILES;
            attacks |= sq >> 10 & BitboardUtils.NOT_GH_FILES;
            attacks |= sq >> 17 & BitboardUtils.NOT_H_FILE;
            return attacks;
        }

        private ulong CalculateKingAttacks(int sqInd)
        {
            ulong sq = BitboardUtils.GetBitboard(sqInd);
            ulong attacks = sq << 1 & BitboardUtils.NOT_A_FILE;
            attacks |= sq >> 1 & BitboardUtils.NOT_H_FILE;
            attacks |= sq << 9 & BitboardUtils.NOT_A_FILE;
            attacks |= sq << 7 & BitboardUtils.NOT_H_FILE;
            attacks |= sq >> 7 & BitboardUtils.NOT_A_FILE;
            attacks |= sq >> 9 & BitboardUtils.NOT_H_FILE;
            attacks |= sq << 8;
            attacks |= sq >> 8;
            return attacks;
        }

        public ulong GetBishopAttacks(int sqInd, ulong occupancy)
        {
            return magic.GetAttacks(sqInd, occupancy, false);
        }

        public ulong GetRookAttacks(int sqInd, ulong occupancy)
        {
            return magic.GetAttacks(sqInd, occupancy, true);
        }

        public ulong GetKnightAttacks(int sqInd)
        {
            return knightAttacks[sqInd];
        }

        public ulong GetKingAttacks(int sqInd)
        {
            return kingAttacks[sqInd];
        }
    }
}
