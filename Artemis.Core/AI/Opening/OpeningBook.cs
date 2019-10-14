using Artemis.Core.AI.Transposition;
using Artemis.Core.Moves;
using Artemis.Core.Moves.PregeneratedAttacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Artemis.Core.AI.Opening
{
    public class OpeningBook
    {
        private PregeneratedAttacksData pregeneratedAttacks;
        private ZobristHashUtils zobristHashUtils;
        private GameState gameState;
        private Random rng = new Random();
        private Dictionary<ulong, List<Move>> book = new Dictionary<ulong, List<Move>>();
        private List<MoveNode> bookList = new List<MoveNode>
        {
            new MoveNode("e2e4",
                new List<MoveNode> { new MoveNode("c7c5"), new MoveNode("e7e5"), new MoveNode("e7e6"),
                    new MoveNode("c7c6"), new MoveNode("d7d6"), new MoveNode("g7g6"),
                    new MoveNode("d7d5"), new MoveNode("g8f6") }),
            new MoveNode("d2d4",
                new List<MoveNode> { new MoveNode("g8f6"), new MoveNode("d7d5"), new MoveNode("e7e6"),
                    new MoveNode("d7d6"), new MoveNode("f7f5"), new MoveNode("g7g6"),
                    new MoveNode("c7c5")
                }),
            new MoveNode("g1f3",
                new List<MoveNode> { new MoveNode("g8f6"), new MoveNode("d7d5"), new MoveNode("c7c5"),
                    new MoveNode("g7g6"), new MoveNode("f7f5"), new MoveNode("d7d6"),
                    new MoveNode("e7e6")
                }),
            new MoveNode("c2c4",
                new List<MoveNode> { new MoveNode("g8f6"), new MoveNode("e7e5"), new MoveNode("e7e6"),
                    new MoveNode("c7c5"), new MoveNode("g7g6"), new MoveNode("c7c6")
                }),
            new MoveNode("g2g3",
                new List<MoveNode> { new MoveNode("d7d5"), new MoveNode("g8f6"), new MoveNode("g7g6"),
                     new MoveNode("e7e5"), new MoveNode("c7c5")
                }),
            new MoveNode("b2b3",
                new List<MoveNode> { new MoveNode("e7e5"), new MoveNode("d7d5"), new MoveNode("g8f6"),
                    new MoveNode("b7b6")
                })
        };
        public bool IsLoaded { get; private set; } = false;

        public OpeningBook(PregeneratedAttacksData pregeneratedAttacks, ZobristHashUtils zobristHashUtils)
        {
            this.zobristHashUtils = zobristHashUtils;
            this.pregeneratedAttacks = pregeneratedAttacks;
        }

        public void Load()
        {
            gameState = new GameState(pregeneratedAttacks, zobristHashUtils);
            LoadRecursive(bookList);
            IsLoaded = true;
        }

        private void LoadRecursive(List<MoveNode> movesList)
        {
            List<Move> legalMoves = gameState.GetLegalMoves();
            List<Move> openingMoves = new List<Move>();
            foreach (MoveNode moveNode in movesList)
            {
                Move foundMove = legalMoves.First(m => m.ToString() == moveNode.Text);
                if (foundMove != null)
                {
                    openingMoves.Add(foundMove);
                    if (moveNode.Responses.Count > 0)
                    {
                        gameState.MakeMove(foundMove);
                        LoadRecursive(moveNode.Responses);
                        gameState.UnmakeMove(foundMove);
                    }
                }
            }
            if (openingMoves.Count > 0)
            {
                ulong hash = gameState.GetIrrevState().ZobristHash;
                book[hash] = openingMoves;
            }
        }

        public bool TryGetMove(GameState gameState, out Move move)
        {
            move = null;
            ulong hash = gameState.GetIrrevState().ZobristHash;
            List<Move> responses;
            bool found = book.TryGetValue(hash, out responses);
            if (found)
            {
                move = PickRandomMove(responses);
                move.SetGameState(gameState);
            }
            return found;
        }

        private Move PickRandomMove(List<Move> moves)
        {
            int rndIndex = rng.Next(0, moves.Count);
            return moves[rndIndex];
        }
    }
}
