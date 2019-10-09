using Artemis.Core.Moves.PregeneratedAttacks;
using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.Moves.Generator
{
    class MoveGeneratorBuilder
    {
        private GameState gameState;
        private PregeneratedAttacksData pregeneratedAttacks;

        public MoveGeneratorBuilder(GameState gameState, PregeneratedAttacksData pregeneratedAttacks)
        {
            this.gameState = gameState;
            this.pregeneratedAttacks = pregeneratedAttacks;
        }

        public IMoveGenerator Build(PieceType type)
        {
            IMoveGenerator generator = null;
            switch (type)
            {
                case PieceType.Bishop:
                    generator = new BishopMoveGenerator(gameState, pregeneratedAttacks);
                    break;
                case PieceType.Rook:
                    generator = new RookMoveGenerator(gameState, pregeneratedAttacks);
                    break;
                case PieceType.Knight:
                    generator = new KnightMoveGenerator(gameState, pregeneratedAttacks);
                    break;
                case PieceType.Pawn:
                    generator = new PawnMoveGenerator(gameState, pregeneratedAttacks);
                    break;
                case PieceType.Queen:
                    generator = new QueenMoveGenerator(gameState, pregeneratedAttacks);
                    break;
                case PieceType.King:
                    generator = new KingMoveGenerator(gameState, pregeneratedAttacks);
                    break;
            }
            return generator;
        }
    }
}
