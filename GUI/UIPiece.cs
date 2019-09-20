using ChessMoveGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GUI
{
    class UIPiece
    {
        GameState gameState;
        GameManager gm;
        Canvas boardCanvas;
        Rectangle pieceRect;
        public PieceType PieceType { get; private set; }
        public int Pl { get; private set; }
        public int Position { get; private set; }

        public UIPiece(PieceType pieceType, int pl, int position, GameState gameState, GameManager gm, Canvas boardCanvas)
        {
            PieceType = pieceType;
            Pl = pl;
            Position = position;
            this.gameState = gameState;
            this.gm = gm;
            this.boardCanvas = boardCanvas;
        }

        public void Create()
        {
            pieceRect = new Rectangle();
            pieceRect.Width = gm.SquareSize;
            pieceRect.Height = gm.SquareSize;
            pieceRect.Fill = new ImageBrush
            {
                ImageSource = GetImage()
            };
            pieceRect.Cursor = Cursors.Hand;
            Point loc = GetLocation();
            Canvas.SetLeft(pieceRect, loc.X);
            Canvas.SetTop(pieceRect, loc.Y);
            Canvas.SetZIndex(pieceRect, 5);
            //pieceRect.MouseUp += PieceRect_MouseUp;
            boardCanvas.Children.Add(pieceRect);
        }

        private Point GetLocation()
        {
            return gm.GetLocationOfPos(Position);
        }

        private BitmapImage GetImage()
        {
            return GetImageOfPiece(PieceType);
        }

        private BitmapImage GetImageOfPiece(PieceType pieceType)
        {
            char color = Pl == 0 ? 'w' : 'b';
            char notation = pieceType.ToNotation();
            string path = $@"pieces/{color}{notation}.png";
            return GameManager.GetImage(path);
        }
    }
}
