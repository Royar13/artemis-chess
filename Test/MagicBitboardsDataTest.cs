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
    }
}
