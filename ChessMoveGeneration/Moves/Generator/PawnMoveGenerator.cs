﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ChessMoveGeneration.Moves.Generator
{
    public class PawnMoveGenerator : MoveGenerator
    {
        public PawnMoveGenerator(GameState gameState, MagicBitboardsData magic) : base(gameState, magic, PieceType.Pawn)
        {
        }

        public override ulong GenerateAttacks(int pl)
        {
            ulong pawns = gameState.Pieces[pl, (int)pieceType];
            ulong attacks = 0;
            if (pl == 0)
            {
                attacks |= pawns << 7 & BitboardUtils.NOT_H_FILE;
                attacks |= pawns << 9 & BitboardUtils.NOT_A_FILE;
            }
            else
            {
                attacks |= pawns >> 9 & BitboardUtils.NOT_H_FILE;
                attacks |= pawns >> 7 & BitboardUtils.NOT_A_FILE;
            }
            return attacks;
        }

        protected override IEnumerable<Move> GetMovesFromSquare(ulong sq)
        {
            ulong reversedFullOccupancy = ~gameState.FullOccupancy;
            ulong pawns = gameState.Pieces[gameState.Turn, (int)pieceType];
            ulong to;
            if (gameState.Turn == 0)
            {
                to = pawns << 8 & reversedFullOccupancy;
                if (to != 0)
                {
                    yield return new Move(gameState, sq, to, pieceType);
                    if ((sq & BitboardUtils.SECOND_RANK[gameState.Turn]) > 0)
                    {
                        to = to << 8 & reversedFullOccupancy;
                        yield return new Move(gameState, sq, to, pieceType);
                    }
                }
                to = pawns << 7 & BitboardUtils.NOT_H_FILE & gameState.Occupancy[1 - gameState.Turn];
                if (to != 0)
                    yield return new Move(gameState, sq, to, pieceType);
                to = pawns << 9 & BitboardUtils.NOT_A_FILE & gameState.Occupancy[1 - gameState.Turn];
                if (to != 0)
                    yield return new Move(gameState, sq, to, pieceType);
            }
            else
            {
                to = pawns >> 8 & reversedFullOccupancy;
                if (to != 0)
                {
                    yield return new Move(gameState, sq, to, pieceType);
                    if ((sq & BitboardUtils.SECOND_RANK[gameState.Turn]) > 0)
                    {
                        to = to >> 8 & reversedFullOccupancy;
                        yield return new Move(gameState, sq, to, pieceType);
                    }
                }
                to = pawns >> 9 & BitboardUtils.NOT_H_FILE & gameState.Occupancy[1 - gameState.Turn];
                if (to != 0)
                    yield return new Move(gameState, sq, to, pieceType);
                to = pawns >> 7 & BitboardUtils.NOT_A_FILE & gameState.Occupancy[1 - gameState.Turn];
                if (to != 0)
                    yield return new Move(gameState, sq, to, pieceType);
            }
        }
    }
}
