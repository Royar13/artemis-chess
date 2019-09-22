using System;
using System.Collections.Generic;
using System.IO;
using Artemis.Core;
using Artemis.Core.Moves;
using b = DotNetEngine.Engine.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotNetEngine.Engine.Helpers;
using DotNetEngine.Engine.Enums;
using System.Linq;

namespace Test.Moves
{
    [TestClass]
    public class MoveGenerationTest
    {
        [TestMethod]
        public void Perft()
        {
            GameState gameState = new GameState();
            string txt = File.ReadAllText(@"data/TestPositions.txt");
            string[] positions = txt.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string pos in positions)
            {
                string[] parts = pos.Split('|');
                gameState.LoadFEN(parts[0]);
                int amount = GenerateMoves(gameState, int.Parse(parts[1]));
                int expectedAmount = int.Parse(parts[2]);
                Assert.AreEqual(expectedAmount, amount, $"Failed on position {parts[0]}, depth {parts[1]}");
            }
        }

        public int GenerateMoves(GameState gameState, int depth)
        {
            if (depth == 0)
            {
                return 1;
            }
            int amount = 0;
            List<Move> moves = gameState.GetMoves();
            foreach (Move move in moves)
            {
                gameState.MakeMove(move);
                if (move.IsLegal())
                {
                    amount += GenerateMoves(gameState, depth - 1);
                }
                gameState.UnmakeMove(move);
            }
            return amount;
        }

        [TestMethod]
        public void SpecificPosTest()
        {
            GameState gameState = new GameState();
            string fen = "rnbqkbnr/p1pppppp/8/Pp6/8/8/1PPPPPPP/RNBQKBNR b KQkq - 0 2";
            gameState.LoadFEN(fen);
            List<Move> legalMoves = gameState.GetLegalMoves();
        }

        [TestMethod]
        public void CompareToEngine()
        {
            GameState gameState = new GameState();
            string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            int depth = 4;
            gameState.LoadFEN(fen);
            b.ZobristHash hash = new b.ZobristHash();
            b.GameState bGameState = new b.GameState(fen, hash);
            List<Move> problemLine = new List<Move>();
            CompareToEngineRec(gameState, bGameState, new b.MoveData(), hash, problemLine, depth);
            string line = string.Join(",", problemLine.Select(m => m.ToString()));
            Console.WriteLine(line);
        }

        public bool CompareToEngineRec(GameState gameState, b.GameState bGameState, b.MoveData moveData, b.ZobristHash zobristHash, List<Move> problemLine, int depth, int ply = 0)
        {
            if (depth == 0)
            {
                return false;
            }

            bGameState.GenerateMoves(MoveGenerationMode.All, ply, moveData);
            List<Move> moves = gameState.GetMoves();

            foreach (var move in bGameState.Moves[ply])
            {
                bGameState.MakeMove(move, zobristHash);

                if (!bGameState.IsOppositeSideKingAttacked(moveData))
                {
                    string searchMove = move.ToMoveString();
                    Move foundMove = moves.Find(m => MoveToString(m) == searchMove);
                    if (foundMove != null)
                    {
                        gameState.MakeMove(foundMove);
                        if (foundMove.IsLegal())
                        {
                            bool badMove = CompareToEngineRec(gameState, bGameState, moveData, zobristHash, problemLine, depth - 1, ply + 1);

                            moves.Remove(foundMove);

                            if (badMove)
                            {
                                problemLine.Insert(0, foundMove);
                                return true;
                            }
                        }
                        gameState.UnmakeMove(foundMove);
                    }
                    else
                    {
                        Console.WriteLine($"Missing move: {searchMove}");
                        return true;
                    }
                }

                bGameState.UnMakeMove(move);
            }

            Move illegalMove = moves.Find(m =>
            {
                gameState.MakeMove(m);
                bool legal = m.IsLegal();
                gameState.UnmakeMove(m);
                return legal;
            });
            if (illegalMove != null)
            {
                problemLine.Insert(0, illegalMove);
                Console.WriteLine($"Illegal move: {illegalMove}");
                return true;
            }

            return false;
        }

        private string MoveToString(Move move)
        {
            string str = $"{move.From.PosToString()}{move.To.PosToString()}";
            GameAction action = move.GetAction();
            if (action.ChangeType != null)
            {
                str += char.ToLower(action.ChangeType.Value.ToNotation());
            }
            return str;
        }
    }
}
