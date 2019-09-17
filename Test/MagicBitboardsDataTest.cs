using System;
using ChessMoveGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
    [TestClass]
    public class MagicBitboardsDataTest
    {
        [TestMethod]
        public void GetRookMaskTest()
        {
            MagicBitboardsData magic = new MagicBitboardsData();
            ulong mask1 = magic.GetRookMask(0);
            ulong expectedMask1 = 0x000101010101017E;

            ulong mask2 = magic.GetRookMask(25);
            ulong expectedMask2 = 0x000202027C020200;

            Assert.AreEqual(expectedMask1, mask1);
            Assert.AreEqual(expectedMask2, mask2);
        }

        [TestMethod]
        public void GetBishopMaskTest()
        {
            MagicBitboardsData magic = new MagicBitboardsData();
            ulong mask1 = magic.GetBishopMask(63);
            ulong expectedMask1 = 0x0040201008040200;

            ulong mask2 = magic.GetBishopMask(34);
            ulong expectedMask2 = 0x00100A000A102000;

            Assert.AreEqual(expectedMask1, mask1);
            Assert.AreEqual(expectedMask2, mask2);
        }

        [TestMethod]
        public void GetRookAttTest()
        {
            MagicBitboardsData magic = new MagicBitboardsData();
            ulong att1 = magic.GetRookAtt(36, 0x0000004010001000);
            ulong expectedAtt1 = 0x1010106F10000000;

            ulong att2 = magic.GetRookAtt(58, 0x0200000000000004);
            ulong expectedAtt2 = 0xFA04040404040404;

            Assert.AreEqual(expectedAtt1, att1);
            Assert.AreEqual(expectedAtt2, att2);
        }

        [TestMethod]
        public void GetBishopAttTest()
        {
            MagicBitboardsData magic = new MagicBitboardsData();
            ulong att1 = magic.GetBishopAtt(36, 0x8040000000440000);
            ulong expectedAtt1 = 0x0244280028440000;

            ulong att2 = magic.GetBishopAtt(22, 0x0004001000000010);
            ulong expectedAtt2 = 0x00000010A000A010;

            Assert.AreEqual(expectedAtt1, att1);
            Assert.AreEqual(expectedAtt2, att2);
        }

        [TestMethod]
        public void GetBlockersByIndexTest()
        {
            MagicBitboardsData magic = new MagicBitboardsData();
            ulong blockers1 = magic.GetBlockersByIndex(3, 0x0044280028440200);
            ulong expectedBlockers1 = 0x0000000000040200;

            ulong blockers2 = magic.GetBlockersByIndex(57, 0x0044280028440200);
            ulong expectedBlockers2 = 0x0000080028000200;

            Assert.AreEqual(expectedBlockers1, blockers1);
            Assert.AreEqual(expectedBlockers2, blockers2);
        }
    }
}
