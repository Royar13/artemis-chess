using Artemis.Core.Moves.MagicBitboards;
using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.Moves.Generator
{
    class MoveGeneratorBuilder
    {
        private GameState gameState;
        private MagicBitboardsData magic;

        public MoveGeneratorBuilder(GameState gameState, MagicBitboardsData magic)
        {
            this.gameState = gameState;
            this.magic = magic;
        }

        public IMoveGenerator Build(PieceType type)
        {
            IMoveGenerator generator = null;
            switch (type)
            {
                case PieceType.Bishop:
                    generator = new BishopMoveGenerator(gameState, magic);
                    break;
                case PieceType.Rook:
                    generator = new RookMoveGenerator(gameState, magic);
                    break;
                case PieceType.Knight:
                    generator = new KnightMoveGenerator(gameState, magic);
                    break;
                case PieceType.Pawn:
                    generator = new PawnMoveGenerator(gameState, magic);
                    break;
                case PieceType.Queen:
                    generator = new QueenMoveGenerator(gameState, magic);
                    break;
                case PieceType.King:
                    generator = new KingMoveGenerator(gameState, magic);
                    break;
            }
            return generator;
        }
    }
}
