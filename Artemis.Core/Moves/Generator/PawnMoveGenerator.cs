using Artemis.Core.Moves.PregeneratedAttacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Artemis.Core.Moves.Generator
{
    public class PawnMoveGenerator : MoveGenerator
    {
        readonly PieceType[] promotionTypes = { PieceType.Queen, PieceType.Rook, PieceType.Knight, PieceType.Bishop };
        public PawnMoveGenerator(GameState gameState, PregeneratedAttacksData pregeneratedAttacks) : base(gameState, pregeneratedAttacks, PieceType.Pawn)
        {
        }

        public override ulong GenerateAttacks(int pl)
        {
            ulong pawns = gameState.Pieces[pl, (int)pieceType];
            return GenerateAttacksByPawns(pawns, pl);
        }

        public override ulong GenerateAttacksFromSquare(int sqInd)
        {
            ulong pawn = BitboardUtils.GetBitboard(sqInd);
            int pl = (gameState.Pieces[0, (int)pieceType] & pawn) > 0 ? 0 : 1;
            return GenerateAttacksByPawns(pawn, pl);
        }

        public override ulong GenerateAttacksFromSquare(int sqInd, int pl)
        {
            ulong bb = BitboardUtils.GetBitboard(sqInd);
            return GenerateAttacksByPawns(bb, pl);
        }

        private ulong GenerateAttacksByPawns(ulong pawns, int pl)
        {
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

        protected override IEnumerable<Move> GenerateMovesFromSquare(int sqInd, GenerationMode generationMode = GenerationMode.Normal)
        {
            ulong sq = BitboardUtils.GetBitboard(sqInd);
            if ((sq & BitboardUtils.SECOND_RANK[1 - gameState.Turn]) == 0)
            {
                return GetRegularMovesFromSquare(sq, generationMode);
            }
            else
            {
                return GetPromotionMovesFromSquare(sq, generationMode);
            }
        }

        private IEnumerable<Move> GetRegularMovesFromSquare(ulong sq, GenerationMode generationMode)
        {
            ulong reversedFullOccupancy = ~gameState.FullOccupancy;
            ulong to;
            ulong attacks;
            if (gameState.Turn == 0)
            {
                if (generationMode == GenerationMode.Normal)
                {
                    to = sq << 8 & reversedFullOccupancy;
                    if (to != 0)
                    {
                        yield return new Move(gameState, sq, to, pieceType);
                        if ((sq & BitboardUtils.SECOND_RANK[gameState.Turn]) > 0)
                        {
                            to = to << 8 & reversedFullOccupancy;
                            if (to != 0)
                                yield return new Move(gameState, sq, to, pieceType);
                        }
                    }
                }
                to = sq << 7 & BitboardUtils.NOT_H_FILE;
                attacks = to;
                if ((to & gameState.Occupancy[1 - gameState.Turn]) != 0)
                    yield return new Move(gameState, sq, to, pieceType);
                to = sq << 9 & BitboardUtils.NOT_A_FILE;
                attacks |= to;
                if ((to & gameState.Occupancy[1 - gameState.Turn]) != 0)
                    yield return new Move(gameState, sq, to, pieceType);
            }
            else
            {
                if (generationMode == GenerationMode.Normal)
                {
                    to = sq >> 8 & reversedFullOccupancy;
                    if (to != 0)
                    {
                        yield return new Move(gameState, sq, to, pieceType);
                        if ((sq & BitboardUtils.SECOND_RANK[gameState.Turn]) > 0)
                        {
                            to = to >> 8 & reversedFullOccupancy;
                            if (to != 0)
                                yield return new Move(gameState, sq, to, pieceType);
                        }
                    }
                }
                to = sq >> 9 & BitboardUtils.NOT_H_FILE;
                attacks = to;
                if ((to & gameState.Occupancy[1 - gameState.Turn]) != 0)
                    yield return new Move(gameState, sq, to, pieceType);
                to = sq >> 7 & BitboardUtils.NOT_A_FILE;
                attacks |= to;
                if ((to & gameState.Occupancy[1 - gameState.Turn]) != 0)
                    yield return new Move(gameState, sq, to, pieceType);
            }

            //En Passant
            IrrevState irrevState = gameState.GetIrrevState();
            if ((irrevState.EnPassantCapture & attacks) > 0)
            {
                yield return new EnPassantMove(gameState, sq, irrevState.EnPassantCapture);
            }
        }

        private IEnumerable<Move> GetPromotionMovesFromSquare(ulong sq, GenerationMode generationMode)
        {
            return GetRegularMovesFromSquare(sq, generationMode).SelectMany(m =>
            {
                return promotionTypes.Select(p => new PromotionMove(gameState, m.From, m.To, p));
            });
        }
    }
}
