using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.AI.Evaluation
{
    public class MaterialBalance : IEvaluator
    {
        GameState gameState;
        private readonly Dictionary<PieceType, int> piecesValue = new Dictionary<PieceType, int> {
            { PieceType.Bishop, 300 },
            { PieceType.Knight, 290 },
            { PieceType.Rook, 500 },
            { PieceType.Queen, 900 },
            { PieceType.Pawn, 100 }
        };

        public MaterialBalance(GameState gameState)
        {
            this.gameState = gameState;
        }

        public int GetScore()
        {
            int score = 0;
            for (int pl = 0; pl <= 1; pl++)
            {
                int sign = pl == gameState.Turn ? 1 : -1;
                for (int i = 0; i < 5; i++)
                {
                    score += sign * BitboardUtils.Popcount(gameState.Pieces[pl, i]) * piecesValue[(PieceType)i];
                }
            }
            return score;
        }
    }
}
