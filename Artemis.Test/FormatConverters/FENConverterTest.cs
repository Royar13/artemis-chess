using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Artemis.Core.FormatConverters;

namespace Artemis.Test.FormatConverters
{
    [TestClass]
    public class FENConverterTest
    {
        string[] validFens =
        {
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
            "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1",
            "rnbqkbnr/pp1ppppp/8/2p5/4P3/8/PPPP1PPP/RNBQKBNR w KQkq c6 0 2",
            "rnbqkbnr/pp1ppppp/8/2p5/4P3/5N2/PPPP1PPP/RNBQKB1R b KQkq - 1 2",
            "rnbqkbnr/pp1ppppp/8/2p5/4P3/5N2/PPPP1PPP/RNBQKB1R b - - 1 2",
            "r2q1b1r/1pp1kpp1/2n2n2/pB1ppb1p/P3P3/2P2N2/1P1PQPPP/RNB1K2R w - - 0 9",
            "3kr3/1p3p2/2p2p2/p6p/P4B2/2P5/1P1N1RPP/R2K4 w - - 1 24",
            "8/5k2/8/4B2p/1P6/8/1P4PP/3K4 b - - 0 33"
        };

        string[] invalidFens =
        {
            "dasgGSFDAa",
            "rnbqkbnr/pp1ppTpp/8/2p5/4P3/5N2/PPPP1PPP/RNBQKB1R b KQkq - 1 2",
            "/3kr3/1p3p2/2p2p2/p6p/P4B2/2P5/1P1N1RPP/R2K4 w - - 1 24",
            "3kr3/1p3p2/2p2p2/p6p/P4B2/2P5/1P1N1RPP/R2K4/ w - - 1 24",
            "3kr3/1p3p2/2p2p2/p6p/P4B2/2P5/1PR1N1RPP/R2K4/ w - - 1 24",
            "9/5k2/8/4B2p/1P6/8/1P4PP/3K4 b - - 0 33",
            "8/5k3/8/4B2p/1P6/8/1P4PP/3K4 b - - 0 33",
            "8/5k2/8/4B2p/1P6/8/1P4PP/3K4 b - - - 0 33",
            "3kr3/1p3p2/2p2p2/p6p/P4B2/2P5/1P1N1RPP/R2K4 c - - 1 24",
            "rnbqkbnr/pp1ppppp/8/2p5/4P3/5N2/PPPP1PPP/RNBQKB1R b kqKQ - 1 2",
            "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e32 0 1",
            "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 m 1",
            "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 3 t",
            "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b   e3 3 t"
        };

        [TestMethod]
        public void IsValidTest()
        {
            IFormatConverter fenConverter = new FENConverter();
            foreach (string fen in validFens)
            {
                Assert.IsTrue(fenConverter.IsValid(fen), $"The FEN \"{fen}\" was recognised as invalid, despite being valid");
            }
            foreach (string fen in invalidFens)
            {
                Assert.IsFalse(fenConverter.IsValid(fen), $"The FEN \"{fen}\" was recognised as valid, despite being invalid");
            }
        }
    }
}
