using System;
using Artemis.Core;
using Artemis.Core.Moves.PregeneratedAttacks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Artemis.Test.Moves
{
    [TestClass]
    public class MagicBitboardsDataTest
    {
        MagicBitboardsData magic;
        PrivateObject magicPO;

        [TestInitialize]
        public void TestInit()
        {
            magic = new MagicBitboardsData();
            magicPO = new PrivateObject(magic);
        }

        [TestMethod]
        public void GetBlockersByIndexTest()
        {
            ulong blockers1 = (ulong)magicPO.Invoke("GetBlockersByIndex", 3, (ulong)0x0044280028440200);
            ulong expectedBlockers1 = 0x0000000000040200;

            ulong blockers2 = (ulong)magicPO.Invoke("GetBlockersByIndex", 57, (ulong)0x0044280028440200);
            ulong expectedBlockers2 = 0x0000080028000200;

            Assert.AreEqual(expectedBlockers1, blockers1);
            Assert.AreEqual(expectedBlockers2, blockers2);
        }

        [TestMethod]
        public void FindMagicsTest()
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    magicPO.Invoke("CalculateMagics");
                }
                catch (Exception)
                {
                    Assert.Fail("Failed to find magics");
                }
            }
        }
    }
}
