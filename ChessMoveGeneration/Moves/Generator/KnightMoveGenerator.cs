using System;
using System.Collections.Generic;
using System.Text;

namespace ChessMoveGeneration.Moves.Generator
{
    class KnightMoveGenerator : MoveGenerator
    {
        public KnightMoveGenerator(GameState gameState, MagicBitboardsData magic) : base(gameState, magic, PieceType.Knight)
        {
        }

        public override ulong GenerateAttacks(int pl)
        {
            ulong knights = gameState.Pieces[pl, (int)pieceType];
            ulong west, east, union, attacks;
            east = BitboardUtils.OneEast(knights);
            west = BitboardUtils.OneWest(knights);
            union = east | west;
            attacks = union << 16;
            attacks |= union >> 16;
            east = BitboardUtils.OneEast(east);
            west = BitboardUtils.OneWest(west);
            union = east | west;
            attacks |= union << 8;
            attacks |= union >> 8;
            return attacks;
        }

        protected override IEnumerable<ulong> GetSequentialAttacksFromSquare(int pl, ulong sq)
        {
            ulong reversedOccupancy = ~gameState.Occupancy[pl];
            ulong attack = sq << 17 & BitboardUtils.NOT_A_FILE & reversedOccupancy;
            yield return attack;

            attack = sq << 10 & BitboardUtils.NOT_AB_FILES & reversedOccupancy;
            yield return attack;

            attack = sq >> 6 & BitboardUtils.NOT_AB_FILES & reversedOccupancy;
            yield return attack;

            attack = sq >> 15 & BitboardUtils.NOT_A_FILE & reversedOccupancy;
            yield return attack;

            attack = sq << 15 & BitboardUtils.NOT_H_FILE & reversedOccupancy;
            yield return attack;

            attack = sq << 6 & BitboardUtils.NOT_GH_FILES & reversedOccupancy;
            yield return attack;

            attack = sq >> 10 & BitboardUtils.NOT_GH_FILES & reversedOccupancy;
            yield return attack;

            attack = sq >> 17 & BitboardUtils.NOT_H_FILE & reversedOccupancy;
            yield return attack;
        }

    }
}
