using System;
using System.Collections.Generic;
using System.Text;

namespace ChessMoveGeneration.Moves.Generator
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
            }
            return generator;
        }
    }
}
