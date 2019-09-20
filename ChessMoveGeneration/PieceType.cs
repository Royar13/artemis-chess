using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ChessMoveGeneration
{
    public enum PieceType
    {
        Rook,
        Knight,
        Bishop,
        Pawn,
        Queen,
        King
    }

    public static class PieceTypeConverter
    {
        static Dictionary<PieceType, char> pieceNotation = new Dictionary<PieceType, char>
        {
            { PieceType.Rook, 'R' },
            { PieceType.Knight, 'N' },
            { PieceType.Bishop, 'B' },
            { PieceType.Pawn, 'P' },
            { PieceType.Queen, 'Q' },
            { PieceType.King, 'K' }
        };

        public static char ToNotation(this PieceType pieceType)
        {
            return pieceNotation[pieceType];
        }

        public static PieceType ToPieceType(this char notation)
        {
            notation = char.ToUpper(notation);
            return pieceNotation.First(n => n.Value == notation).Key;
        }
    }
}
