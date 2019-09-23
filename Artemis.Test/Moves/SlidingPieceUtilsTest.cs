using System;
using Artemis.Core.Moves.MagicBitboards;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Moves
{
    [TestClass]
    public class SlidingPieceUtilsTest
    {
        [TestMethod]
        public void GetRookMaskTest()
        {
            ulong mask1 = SlidingPieceUtils.GetRookMask(0);
            ulong expectedMask1 = 0x000101010101017E;

            ulong mask2 = SlidingPieceUtils.GetRookMask(25);
            ulong expectedMask2 = 0x000202027C020200;

            Assert.AreEqual(expectedMask1, mask1);
            Assert.AreEqual(expectedMask2, mask2);
        }

        [TestMethod]
        public void GetBishopMaskTest()
        {
            ulong mask1 = SlidingPieceUtils.GetBishopMask(63);
            ulong expectedMask1 = 0x0040201008040200;

            ulong mask2 = SlidingPieceUtils.GetBishopMask(34);
            ulong expectedMask2 = 0x00100A000A102000;

            Assert.AreEqual(expectedMask1, mask1);
            Assert.AreEqual(expectedMask2, mask2);
        }

        [TestMethod]
        public void GetRookAttTest()
        {
            ulong att1 = SlidingPieceUtils.GetRookAtt(36, 0x0000004010001000);
            ulong expectedAtt1 = 0x1010106F10000000;

            ulong att2 = SlidingPieceUtils.GetRookAtt(58, 0x0200000000000004);
            ulong expectedAtt2 = 0xFA04040404040404;

            Assert.AreEqual(expectedAtt1, att1);
            Assert.AreEqual(expectedAtt2, att2);
        }

        [TestMethod]
        public void GetBishopAttTest()
        {
            ulong att1 = SlidingPieceUtils.GetBishopAtt(36, 0x8040000000440000);
            ulong expectedAtt1 = 0x0244280028440000;

            ulong att2 = SlidingPieceUtils.GetBishopAtt(22, 0x0004001000000010);
            ulong expectedAtt2 = 0x00000010A000A010;

            Assert.AreEqual(expectedAtt1, att1);
            Assert.AreEqual(expectedAtt2, att2);
        }
    }
}
