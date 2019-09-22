using System;
using Artemis.Core;
using Artemis.Core.AI.TranspositionTable;
using Artemis.Core.Moves;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Moves
{
    [TestClass]
    public class ZobristIncrementalUpdateTest
    {
        GameState gameState;
        PrivateObject zobristHashUtils;

        [TestInitialize]
        public void TestInit()
        {
            gameState = new GameState();
            zobristHashUtils = new PrivateObject(gameState.ZobristHashUtils);
        }

        private ulong[,] GetPiecePosBitstring()
        {
            return (ulong[,])zobristHashUtils.GetField("piecePosBitstring");
        }

        private ulong GetBlackTurnBitstring()
        {
            return (ulong)zobristHashUtils.GetField("blackTurnBitstring");
        }

        private ulong[,] GetCastlingBitstring()
        {
            return (ulong[,])zobristHashUtils.GetField("castlingBitstring");
        }

        private ulong[] GetEnPassantBitstring()
        {
            return (ulong[])zobristHashUtils.GetField("enPassantBitstring");
        }

        private ulong GetZobristHash()
        {
            return gameState.GetIrrevState().ZobristHash;
        }

        private ulong GetPieceBitstring(int pl, PieceType pieceType, int sqInd)
        {
            int pInd = (int)zobristHashUtils.Invoke("GetPieceIndex", pl, pieceType);
            return GetPiecePosBitstring()[pInd, sqInd];
        }

        private void TestMove(string fen, Move move, ulong expectedHash)
        {
            gameState.LoadFEN(fen);
            expectedHash ^= GetZobristHash();
            gameState.MakeMove(move);
            Assert.AreEqual(expectedHash, GetZobristHash());
        }

        [TestMethod]
        public void ZobristUpdateTest_RegularMove()
        {
            Move move = new Move(gameState, BitboardUtils.GetBitboard(12), BitboardUtils.GetBitboard(20), PieceType.Pawn);
            ulong expectedHash = GetPieceBitstring(0, PieceType.Pawn, 12) ^ GetPieceBitstring(0, PieceType.Pawn, 20) ^ GetBlackTurnBitstring();
            TestMove("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", move, expectedHash);

            move = new Move(gameState, BitboardUtils.GetBitboard(36), BitboardUtils.GetBitboard(30), PieceType.Knight);
            expectedHash = GetPieceBitstring(1, PieceType.Knight, 36) ^ GetPieceBitstring(1, PieceType.Knight, 30) ^ GetBlackTurnBitstring();
            TestMove("r2qkb1r/pbp1pppp/1p3n2/3pn3/2BP4/5N2/PPP2PPP/RNBQR1K1 b kq - 1 7", move, expectedHash);
        }

        [TestMethod]
        public void ZobristUpdateTest_Capture()
        {
            Move move = new Move(gameState, BitboardUtils.GetBitboard(4), BitboardUtils.GetBitboard(28), PieceType.Rook);
            ulong expectedHash = GetPieceBitstring(0, PieceType.Rook, 4) ^ GetPieceBitstring(0, PieceType.Rook, 28)
                ^ GetPieceBitstring(1, PieceType.Knight, 28) ^ GetBlackTurnBitstring();
            TestMove("r2q1b1r/pbp1pkpp/1p6/3p4/2BPn3/8/PPP2PPP/RNB1R1K1 w - - 0 10", move, expectedHash);

            move = new Move(gameState, BitboardUtils.GetBitboard(27), BitboardUtils.GetBitboard(34), PieceType.Pawn);
            expectedHash = GetPieceBitstring(0, PieceType.Pawn, 27) ^ GetPieceBitstring(0, PieceType.Pawn, 34)
                ^ GetPieceBitstring(1, PieceType.Pawn, 34) ^ GetEnPassantBitstring()[2] ^ GetBlackTurnBitstring();
            TestMove("r2q1b1r/pb2pkpp/1p6/2pp4/P1BPn3/8/1PP2PPP/RNB1R1K1 w - c6 0 11", move, expectedHash);
        }

        [TestMethod]
        public void ZobristUpdateTest_Castling()
        {
            Move move = new CastlingMove(gameState, BitboardUtils.GetBitboard(4), BitboardUtils.GetBitboard(6));
            ulong expectedHash = GetPieceBitstring(0, PieceType.Rook, 7) ^ GetPieceBitstring(0, PieceType.Rook, 5)
                ^ GetPieceBitstring(0, PieceType.King, 4) ^ GetPieceBitstring(0, PieceType.King, 6)
                ^ GetCastlingBitstring()[0, 0] ^ GetCastlingBitstring()[0, 1] ^ GetBlackTurnBitstring();
            TestMove("r1bqkbnr/ppp2ppp/2n5/3pp3/4P3/3B1N2/PPPP1PPP/RNBQK2R w KQkq - 0 4", move, expectedHash);

            move = new CastlingMove(gameState, BitboardUtils.GetBitboard(60), BitboardUtils.GetBitboard(58));
            expectedHash = GetPieceBitstring(1, PieceType.Rook, 56) ^ GetPieceBitstring(1, PieceType.Rook, 59)
                ^ GetPieceBitstring(1, PieceType.King, 60) ^ GetPieceBitstring(1, PieceType.King, 58)
                ^ GetCastlingBitstring()[1, 0] ^ GetCastlingBitstring()[1, 1] ^ GetBlackTurnBitstring();
            TestMove("r3kbnr/ppp2ppp/2nq4/3pp3/4P1b1/3B1N2/PPPPQPPP/RNBR2K1 b kq - 5 6", move, expectedHash);
        }
    }
}
