using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.Moves.MagicBitboards
{
    public class MagicBitboardsData
    {
        private ulong[] rookMasks = new ulong[64];
        private ulong[] bishopMasks = new ulong[64];
        private ulong[] rookMagics = new ulong[64]
        {
            36030171410608261,18015635464556546,36074427288391680,144119724636113409,36033197217331200,144123056523182128,36030996050624768,9403517121514719616,90212732185362448,9304577842539143168,563019790844416,9282481824923263296,5770271382569600,1153484463150731268,281492441809408,4611826758063251712,3554171349368960,10380797417040977920,9800996072733051392,141288586414080,2270492652732672,1730086495215158272,9011599584006665,1731636255756910657,9318299474898006144,9801099576881448192,459367853485785360,2451092899737108608,577591060995768448,4400195174528,2533296274276356,317252054303745,4647750274704736400,27304447147253888,9007618022445058,9223410520181182464,8070504412696545280,2306124492931351552,36609347815507969,4611793230508328196,4644477776199680,4505525383479296,4508032044179457,8796361490560,9250677342983225360,576465150383521920,365391907739926536,9224005917186588707,35463620460800,2328369941020934720,2454466229325266976,9223943785049751680,3530962862564966528,2815301838962816,612771042720498432,
            596887775687737856,10088344778539270209,293877623564599425,585469051616116809,12103562185146377,563018749976842,18858832164028457,846763557652484,324831761418748034
        };
        private ulong[] bishopMagics = new ulong[64]
        {
            1164286074038534160,307379477170651136,54364257768178314,289360710612353288,9236056827647500616,9441939625088127008,18578465192150276,5138018928181285,1729435102204728320,72076294334710160,36038179442163715,9242609135246114888,163538130773762112,162130754851045440,13835058622486807700,18006687092896,2308095427771975690,9224288188008505600,2251886921056560,9333710382378918016,156500233097592842,289637759629460484,151200104712208,2449993400116839488,149049800629683208,3459345090487781376,73187926388837376,684855007706628352,3459046057533521920,2308668756249281080,1731950990967965704,74451230870735392,10390385500962687744,74314487683090448,39651171959040,1837503866699055185,54043745825456160,4505807261802656,36601179268581888,2252386079417856,2306986536069107205,2523142825239974660,9799870173392457728,4504704558041088,144684873762800128,2832505195479072,9009537881342464,1459621485773063232,14998113831712981568,635553221593120,1729666214513411108,1171114071312368140,144689150409572368,72092787608666113,
            4657852364261425664,182398069900451904,1175758398965420144,18333823834066954,4629700696120755201,648518372413477897,211106501625344,4612812211601015040,18837768766465,291911620672749697
        };
        private ulong[][] rookAttacksTable = new ulong[64][];
        private ulong[][] bishopAttacksTable = new ulong[64][];

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
                HashBlockers(i, true);
                HashBlockers(i, false);
            }
        }

        public ulong GetAttacks(int sqInd, ulong occupancy, bool rook)
        {
            ulong attacks;
            if (rook)
            {
                ulong hashed = ApplyMagic(occupancy & rookMasks[sqInd], rookMagics[sqInd], RBits[sqInd]);
                attacks = rookAttacksTable[sqInd][hashed];
            }
            else
            {
                ulong hashed = ApplyMagic(occupancy & bishopMasks[sqInd], bishopMagics[sqInd], BBits[sqInd]);
                attacks = bishopAttacksTable[sqInd][hashed];
            }
            return attacks;
        }

        private void CalculateMagics()
        {
            for (int i = 0; i < 64; i++)
            {
                ulong[] rookHashTable, bishopHashTable;
                rookMagics[i] = FindMagic(i, true, out rookHashTable);
                bishopMagics[i] = FindMagic(i, false, out bishopHashTable);
                rookAttacksTable[i] = rookHashTable;
                bishopAttacksTable[i] = bishopHashTable;
            }
        }

        private void HashBlockers(int sqInd, bool rook)
        {
            ulong mask = SaveMask(sqInd, rook);
            int[] bitsArr = rook ? RBits : BBits;
            int bits = bitsArr[sqInd];
            int combinations = 1 << bits;
            ulong[] blockers = new ulong[combinations];
            ulong[] attacks = new ulong[combinations];

            for (int i = 0; i < combinations; i++)
            {
                blockers[i] = GetBlockersByIndex(i, mask);
                attacks[i] = rook ? SlidingPieceUtils.GetRookAtt(sqInd, blockers[i]) : SlidingPieceUtils.GetBishopAtt(sqInd, blockers[i]);
            }

            ulong magic = rook ? rookMagics[sqInd] : bishopMagics[sqInd];
            ulong[][] attacksTable = rook ? rookAttacksTable : bishopAttacksTable;
            attacksTable[sqInd] = new ulong[combinations];

            for (int i = 0; i < combinations; i++)
            {
                ulong hashed = ApplyMagic(blockers[i], magic, bits);
                attacksTable[sqInd][hashed] = attacks[i];
            }
        }

        private ulong SaveMask(int sqInd, bool rook)
        {
            ulong mask;
            if (rook)
            {
                mask = rookMasks[sqInd] = SlidingPieceUtils.GetRookMask(sqInd);
            }
            else
            {
                mask = bishopMasks[sqInd] = SlidingPieceUtils.GetBishopMask(sqInd);
            }
            return mask;
        }

        private ulong FindMagic(int sqInd, bool rook, out ulong[] hashTable)
        {
            ulong mask = SaveMask(sqInd, rook);
            int[] bitsArr = rook ? RBits : BBits;
            int bits = bitsArr[sqInd];
            int combinations = 1 << bits;
            ulong[] blockers = new ulong[combinations];
            ulong[] attacks = new ulong[combinations];
            hashTable = new ulong[combinations];

            for (int i = 0; i < combinations; i++)
            {
                blockers[i] = GetBlockersByIndex(i, mask);
                attacks[i] = rook ? SlidingPieceUtils.GetRookAtt(sqInd, blockers[i]) : SlidingPieceUtils.GetBishopAtt(sqInd, blockers[i]);
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

        private ulong GetBlockersByIndex(int index, ulong mask)
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

        private ulong ApplyMagic(ulong blockers, ulong magic, int bits)
        {
            return (blockers * magic) >> (64 - bits);
        }

        private ulong RandomUInt64FewBits()
        {
            return BitboardUtils.RandomBitstring() & BitboardUtils.RandomBitstring() & BitboardUtils.RandomBitstring();
        }
    }
}
