using System;
using ChessMoveGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
    [TestClass]
    public class BitboardUtilsTest
    {
        [TestMethod]
        public void GetLSBTest()
        {
            ulong bb1 = BitboardUtils.GetLSB(0b01110100011100);
            ulong expectedBb1 = 0b100;

            ulong bb2 = BitboardUtils.GetLSB(0b111);
            ulong expectedBb2 = 0b1;

            ulong bb3 = BitboardUtils.GetLSB(0b1001110010);
            ulong expectedBb3 = 0b10;

            ulong bb4 = BitboardUtils.GetLSB(0b1000000);
            ulong expectedBb4 = 0b1000000;

            Assert.AreEqual(expectedBb1, bb1);
            Assert.AreEqual(expectedBb2, bb2);
            Assert.AreEqual(expectedBb3, bb3);
            Assert.AreEqual(expectedBb4, bb4);
        }
    }
}
