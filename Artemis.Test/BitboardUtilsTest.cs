using System;
using System.Diagnostics;
using Artemis.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Artemis.Test
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

            ulong bb5 = BitboardUtils.GetLSB(0);
            ulong expectedBb5 = 0;

            Assert.AreEqual(expectedBb1, bb1);
            Assert.AreEqual(expectedBb2, bb2);
            Assert.AreEqual(expectedBb3, bb3);
            Assert.AreEqual(expectedBb4, bb4);
            Assert.AreEqual(expectedBb5, bb5);
        }

        [TestMethod]
        public void GetMSBTest()
        {
            ulong bb1 = BitboardUtils.GetMSB(0b01110100011100);
            ulong expectedBb1 = 0b1000000000000;

            ulong bb2 = BitboardUtils.GetMSB(0b111);
            ulong expectedBb2 = 0b100;

            ulong bb3 = BitboardUtils.GetMSB(0b1001110010);
            ulong expectedBb3 = 0b1000000000;

            ulong bb4 = BitboardUtils.GetMSB(0b1000000);
            ulong expectedBb4 = 0b1000000;

            ulong bb5 = BitboardUtils.GetMSB(0);
            ulong expectedBb5 = 0;

            ulong bb6 = BitboardUtils.GetMSB(0x30AB067088F2);
            ulong expectedBb6 = 0x200000000000;


            Assert.AreEqual(expectedBb1, bb1);
            Assert.AreEqual(expectedBb2, bb2);
            Assert.AreEqual(expectedBb3, bb3);
            Assert.AreEqual(expectedBb4, bb4);
            Assert.AreEqual(expectedBb5, bb5);
            Assert.AreEqual(expectedBb6, bb6);
        }

        [TestMethod]
        public void SparsePopcountTest()
        {
            ulong bb1 = 0b10001001011101;
            Assert.AreEqual(7, BitboardUtils.SparsePopcount(bb1));

            ulong bb2 = 0b1111101;
            Assert.AreEqual(6, BitboardUtils.SparsePopcount(bb2));

            ulong bb3 = 0;
            Assert.AreEqual(0, BitboardUtils.SparsePopcount(bb3));
        }

        [TestMethod]
        public void PopcountCompareTest()
        {
            for (int i = 0; i < 10000; i++)
            {
                ulong bb = BitboardUtils.RandomBitstring();
                Assert.AreEqual(BitboardUtils.Popcount(bb), BitboardUtils.SparsePopcount(bb));
            }
        }

        [TestMethod]
        public void PopcountSpeedTest()
        {
            Stopwatch stopwatch = new Stopwatch();
            int amount = 100000000;
            stopwatch.Start();
            for (int i = 0; i < amount; i++)
            {
                ulong bb = BitboardUtils.RandomBitstring();
                BitboardUtils.SparsePopcount(bb);
            }
            stopwatch.Stop();
            long sparsePopcountTime = stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();
            for (int i = 0; i < amount; i++)
            {
                ulong bb = BitboardUtils.RandomBitstring();
                BitboardUtils.Popcount(bb);
            }
            stopwatch.Stop();
            long popcountTime = stopwatch.ElapsedMilliseconds;
            Console.WriteLine($"Popcount: {popcountTime}ms,\nSparse Popcount: {sparsePopcountTime}ms");
        }

        [TestMethod]
        public void SparsePopcountSpeedTest()
        {
            Stopwatch stopwatch = new Stopwatch();
            int amount = 100000000;
            stopwatch.Start();
            for (int i = 0; i < amount; i++)
            {
                ulong bb = BitboardUtils.RandomBitstring() & BitboardUtils.RandomBitstring() & BitboardUtils.RandomBitstring();
                BitboardUtils.Popcount(bb);
            }
            stopwatch.Stop();
            long popcountTime = stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();
            for (int i = 0; i < amount; i++)
            {
                ulong bb = BitboardUtils.RandomBitstring() & BitboardUtils.RandomBitstring() & BitboardUtils.RandomBitstring();
                BitboardUtils.SparsePopcount(bb);
            }
            stopwatch.Stop();
            long sparsePopcountTime = stopwatch.ElapsedMilliseconds;
            Console.WriteLine($"Popcount: {popcountTime}ms,\nSparse Popcount: {sparsePopcountTime}ms");
        }

        [TestMethod]
        public void SparsePopcountSpeedTestV2()
        {
            ulong[] values = { 0x10000000010, 0x100000, 0x10000000000, 0x10010000000 };
            Stopwatch stopwatch = new Stopwatch();
            int amount = 1000000000;
            stopwatch.Start();
            for (int i = 0, j = 0; i < amount; i++, j++, j %= values.Length)
            {
                BitboardUtils.Popcount(values[j]);
            }
            stopwatch.Stop();
            long popcountTime = stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();
            for (int i = 0, j = 0; i < amount; i++, j++, j %= values.Length)
            {
                BitboardUtils.SparsePopcount(values[j]);
            }
            stopwatch.Stop();
            long sparsePopcountTime = stopwatch.ElapsedMilliseconds;
            Console.WriteLine($"Popcount: {popcountTime}ms,\nSparse Popcount: {sparsePopcountTime}ms");
        }
    }
}
