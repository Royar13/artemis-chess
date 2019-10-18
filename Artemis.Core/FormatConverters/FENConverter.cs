using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Artemis.Core.FormatConverters
{
    public class FENConverter : IFormatConverter
    {
        public bool IsValid(string fen)
        {
            string pattern = @"^([rRnNbBqQkKpP1-8]{1,8}\/){7}[rRnNbBqQkKpP1-8]{1,8} [wb] (K|Q|k|q|KQ|Kk|Kq|Qk|Qq|kq|KQk|KQq|Kkq|Qkq|KQkq|-) (([a-h][1-8])|-) (0|([1-9]+[0-9]*)) [1-9]+[0-9]*$";
            if (!Regex.IsMatch(fen, pattern))
            {
                return false;
            }
            string[] parts = fen.Trim().Split(' ');
            string[] ranks = parts[0].Split('/');
            foreach (string rank in ranks)
            {
                int sum = 0;
                foreach (char c in rank)
                {
                    if (char.IsDigit(c))
                    {
                        sum += (int)char.GetNumericValue(c);
                    }
                    else
                    {
                        sum++;
                    }
                }
                if (sum != 8)
                {
                    return false;
                }
            }
            return true;
        }

        public void Load(string fen, GameState gameState, bool verifyValidity = false)
        {
            if (verifyValidity && !IsValid(fen))
            {
                throw new ArgumentException("Invalid FEN");
            }
            gameState.Reset();
            IrrevState irrevState = gameState.GetIrrevState();
            ulong currPos = 1;
            string[] parts = fen.Split(' ');
            string[] ranks = parts[0].Split('/');
            for (int i = ranks.Length - 1; i >= 0; i--)
            {
                string rank = ranks[i];
                for (int j = 0; j < rank.Length; j++)
                {
                    char c = rank[j];
                    if (char.IsDigit(c))
                    {
                        int shift = (int)char.GetNumericValue(c);
                        currPos = currPos << shift;
                    }
                    else
                    {
                        int pl = char.IsLower(c) ? 1 : 0;
                        int pieceType = (int)c.ToPieceType();
                        gameState.Pieces[pl, pieceType] |= currPos;
                        gameState.Occupancy[pl] |= currPos;
                        currPos = currPos << 1;
                    }
                }
            }
            int turn = parts[1] == "w" ? 0 : 1;
            if (gameState.Turn != turn)
            {
                gameState.ChangeTurn();
            }
            if (parts[2].IndexOf('Q') < 0)
            {
                irrevState.CastlingAllowed[0, 0] = false;
            }
            if (parts[2].IndexOf('K') < 0)
            {
                irrevState.CastlingAllowed[0, 1] = false;
            }
            if (parts[2].IndexOf('q') < 0)
            {
                irrevState.CastlingAllowed[1, 0] = false;
            }
            if (parts[2].IndexOf('k') < 0)
            {
                irrevState.CastlingAllowed[1, 1] = false;
            }

            if (parts[3] != "-")
            {
                irrevState.EnPassantCapture = BitboardUtils.GetBitboard(parts[3].StringToPos());
            }
            irrevState.HalfmoveClock = int.Parse(parts[4]);
            irrevState.ZobristHash = gameState.ZobristHashUtils.GenerateHash(gameState);
        }

        public string Convert(GameState gameState)
        {
            StringBuilder builder = new StringBuilder();
            for (int r = 7; r >= 0; r--)
            {
                int emptySq = 0;
                for (int f = 0; f < 8; f++)
                {
                    int sqInd = r * 8 + f;
                    string piece = GetPieceBySquare(gameState, sqInd);
                    if (piece == null)
                    {
                        emptySq++;
                    }
                    else
                    {
                        if (emptySq > 0)
                        {
                            builder.Append(emptySq);
                            emptySq = 0;
                        }
                        builder.Append(piece);
                    }
                }
                if (emptySq > 0)
                {
                    builder.Append(emptySq);
                }
                if (r > 0)
                {
                    builder.Append('/');
                }
            }
            char turn = gameState.Turn == 0 ? 'w' : 'b';
            builder.Append(" " + turn + " ");
            string castling = "";
            IrrevState irrevState = gameState.GetIrrevState();
            if (irrevState.CastlingAllowed[0, 1])
            {
                castling += 'K';
            }
            if (irrevState.CastlingAllowed[0, 0])
            {
                castling += 'Q';
            }
            if (irrevState.CastlingAllowed[1, 1])
            {
                castling += 'k';
            }
            if (irrevState.CastlingAllowed[1, 0])
            {
                castling += 'q';
            }
            if (castling == "")
            {
                castling = "-";
            }
            builder.Append(castling + " ");
            string enPassant = "-";
            if (irrevState.EnPassantCapture != 0)
            {
                enPassant = irrevState.EnPassantCapture.PosToString();
            }
            builder.Append(enPassant + " ");
            builder.Append(irrevState.HalfmoveClock + " ");
            builder.Append(gameState.GetTurnNum());
            return builder.ToString();
        }

        private string GetPieceBySquare(GameState gameState, int sqInd)
        {
            string notation = null;
            int pl = 0;
            PieceType? pieceType = gameState.GetPieceBySquare(pl, BitboardUtils.GetBitboard(sqInd));
            if (pieceType == null)
            {
                pl = 1;
                pieceType = gameState.GetPieceBySquare(pl, BitboardUtils.GetBitboard(sqInd));
            }
            if (pieceType != null)
            {
                char c = pieceType.Value.ToNotation();
                c = pl == 0 ? c : char.ToLower(c);
                notation = char.ToString(c);
            }
            return notation;
        }
    }
}
