using Artemis.Core;
using Artemis.Core.AI.Transposition;
using Artemis.Core.Moves.PregeneratedAttacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artemis.Test
{
    static class GameStateBuilder
    {
        public static GameState Build(string fen = null)
        {
            PregeneratedAttacksData pregeneratedAttacks = new PregeneratedAttacksData();
            ZobristHashUtils zobristHashUtils = new ZobristHashUtils();
            pregeneratedAttacks.Initialize();
            GameState gameState = fen == null ? new GameState(pregeneratedAttacks, zobristHashUtils) : new GameState(fen, pregeneratedAttacks, zobristHashUtils);
            return gameState;
        }
    }
}
