using System;
using System.Collections.Generic;
using System.IO;
using Artemis.Core;
using Artemis.Core.Moves;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Diagnostics;

namespace Artemis.Test.Moves
{
    [TestClass]
    public class MoveGenerationTest
    {
        [TestMethod]
        public void Perft()
        {
            GameState gameState = GameStateBuilder.Build();
            string txt = File.ReadAllText(@"data/TestPositions.txt");
            string[] positions = txt.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            long totalAmount = 0;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            foreach (string pos in positions)
            {
                string[] parts = pos.Split('|');
                gameState.LoadFEN(parts[0]);
                int amount = GenerateMoves(gameState, int.Parse(parts[1]));
                totalAmount += amount;
                int expectedAmount = int.Parse(parts[2]);
                Assert.AreEqual(expectedAmount, amount, $"Failed on position {parts[0]}, depth {parts[1]}");
            }
            watch.Stop();
            //nodes per second. worse than the actual value, because it only counts leaf nodes,
            //and it doesn't count variations ending in mate/stalemate before the requested depth
            double nps = totalAmount / ((double)watch.ElapsedMilliseconds / 1000);
            Console.WriteLine($"{nps} NPS");
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
            GameState gameState = GameStateBuilder.Build();
            string fen = "rnbqkbnr/p1pppppp/8/Pp6/8/8/1PPPPPPP/RNBQKBNR b KQkq - 0 2";
            gameState.LoadFEN(fen);
            List<Move> legalMoves = gameState.GetLegalMoves();
        }
    }
}
