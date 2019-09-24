using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.AI.Evaluation
{
    public class PositionEvaluator
    {
        GameState gameState;
        EvaluationConfig config;
        public const int CHECKMATE_SCORE = 1000000;
        private const ulong SAFE_KINGSIDE_SQUARES = 0xC0;
        private const ulong SAFE_QUEENSIDE_SQUARES = 0x7;

        public PositionEvaluator(GameState gameState, EvaluationConfig config)
        {
            this.gameState = gameState;
            this.config = config;
        }

        public int Evaluate(int depth)
        {
            GameResult result = gameState.GetResult();
            if (result != GameResult.Ongoing)
            {
                if (result == GameResult.Checkmate)
                {
                    return -CHECKMATE_SCORE - depth;
                }
                else
                {
                    return 0;
                }
            }

            int score = 0;
            for (int pl = 0; pl <= 1; pl++)
            {
                int sign = pl == gameState.Turn ? 1 : -1;

                //king safety
                ulong king = gameState.Pieces[pl, (int)PieceType.King];
                bool kingCastled = false;
                ulong kingside = GetSafeKingsideSquares(pl);
                ulong queenside = GetSafeQueensideSquares(pl);
                if ((kingside & king) > 0)
                {
                    ulong rookPos = gameState.Pieces[pl, (int)PieceType.Rook] & kingside;
                    if (rookPos == 0 || rookPos > king)
                    {
                        kingCastled = true;
                        score += sign * config.GetKingCastledScore();
                    }
                }
                else if ((queenside & king) > 0)
                {
                    ulong rookPos = gameState.Pieces[pl, (int)PieceType.Rook] & queenside;
                    if (rookPos == 0 || rookPos < king)
                    {
                        kingCastled = true;
                        score += sign * config.GetKingCastledScore();
                    }
                }
                if (!kingCastled)
                {
                    IrrevState irrevState = gameState.GetIrrevState();
                    if (!irrevState.CastlingAllowed[pl, 0] && !irrevState.CastlingAllowed[pl, 1])
                    {
                        score += sign * config.GetKingMiddlePenalty();
                    }
                }
                ulong kingSurrounding = king | gameState.MoveGenerators[(int)PieceType.King].GenerateAttacks(pl);
                ulong pawnProtectors = gameState.Pieces[pl, (int)PieceType.Pawn] & kingSurrounding;
                score += sign * BitboardUtils.Popcount(pawnProtectors) * config.GetKingPawnProtectorsScore();

                ulong attacks = 0;
                for (int i = 0; i < 5; i++)
                {
                    PieceType pieceType = (PieceType)i;
                    //material
                    score += sign * BitboardUtils.Popcount(gameState.Pieces[pl, i]) * config.GetPieceValue(pieceType);
                    ulong pieceAttacks = gameState.MoveGenerators[i].GenerateAttacks(pl);
                    //mobility
                    score += sign * BitboardUtils.Popcount(pieceAttacks) * config.GetMobilityScore(pieceType);
                    if (pieceType == PieceType.Pawn)
                    {
                        //pawn central control
                        ulong pawnsCenterControl = (gameState.Pieces[pl, i] | pieceAttacks) & BitboardUtils.CENTER_MASK;
                        score += sign * BitboardUtils.SparsePopcount(pawnsCenterControl) * config.GetPawnCentralControlScore();
                        //pawn support
                        score += sign * BitboardUtils.SparsePopcount(pieceAttacks & gameState.Pieces[pl, i]) * config.GetPawnSupportScore();
                    }
                    else
                    {
                        attacks |= pieceAttacks;
                    }

                    //king attack
                    ulong kingAttacks = kingSurrounding & pieceAttacks;
                    score += sign * BitboardUtils.SparsePopcount(kingAttacks) * config.GetKingAttackScore();
                }
                //center control
                ulong piecesCenterControl = (gameState.Occupancy[pl] | attacks) & BitboardUtils.EXTENDED_CENTER_MASK;
                score += sign * BitboardUtils.Popcount(piecesCenterControl) * config.GetPieceCentralControlScore();
            }
            return score;
        }

        private ulong GetSafeKingsideSquares(int pl)
        {
            ulong mask = SAFE_KINGSIDE_SQUARES;
            if (pl == 1)
            {
                mask <<= 56;
            }
            return mask;
        }

        private ulong GetSafeQueensideSquares(int pl)
        {
            ulong mask = SAFE_QUEENSIDE_SQUARES;
            if (pl == 1)
            {
                mask <<= 56;
            }
            return mask;
        }
    }
}
