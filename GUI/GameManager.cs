using ChessMoveGeneration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GUI
{
    class GameManager
    {
        GameState gameState;
        Canvas boardCanvas;
        List<UIPiece> uiPieces;
        string movesList;
        public string MovesList
        {
            get
            {
                return movesList;
            }
            set
            {
                movesList = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("MovesList"));
                }
            }
        }
        public double SquareSize
        {
            get
            {
                return boardCanvas.Width / GameState.BOARD_SIZE;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public GameManager(Canvas boardCanvas)
        {
            this.boardCanvas = boardCanvas;
            boardCanvas.Background = new ImageBrush
            {
                ImageSource = GetImage("board.png")
            };
        }

        public void NewGame()
        {
            gameState = new GameState();
            boardCanvas.Children.Clear();
            MovesList = "";
            var pieces = gameState.GetPiecesList();
            uiPieces = pieces.Select(p => new UIPiece(p.Item3, p.Item2, p.Item1, gameState, this, boardCanvas)).ToList();
            foreach (UIPiece piece in uiPieces)
            {
                piece.Create();
            }
        }

        public static BitmapImage GetImage(string relativePath)
        {
            Uri uri = new Uri(@"pack://application:,,,/img/" + relativePath);
            return new BitmapImage(uri);
        }

        public Point GetLocationOfPos(int pos)
        {
            Point p = new Point();
            int file = BitboardUtils.GetFile(pos);
            int rank = BitboardUtils.GetRank(pos);
            p.X = SquareSize * file;
            p.Y = SquareSize * (GameState.BOARD_SIZE - rank - 1);
            return p;
        }
    }
}
