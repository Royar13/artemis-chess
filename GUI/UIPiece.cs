using ChessMoveGeneration;
using ChessMoveGeneration.Moves;
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
        Dictionary<Rectangle, GameAction> suggestedActions;
        List<GameAction> legalActions;

        public event EventHandler PieceSelected;

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
            pieceRect.MouseUp += PieceRect_MouseUp;
            boardCanvas.Children.Add(pieceRect);
        }

        private void PieceRect_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!gm.GameEnded && gameState.Turn == Pl)
            {
                Select();
            }
        }

        public void Select()
        {
            PieceSelected?.Invoke(this, EventArgs.Empty);
            if (legalActions == null)
            {
                legalActions = gm.LegalActions.Where(a => a.From == Position).ToList();

                List<GameAction> filteredActions = legalActions.GroupBy(m => m.To).Select(g => g.First()).ToList();
                suggestedActions = new Dictionary<Rectangle, GameAction>();
                foreach (GameAction action in filteredActions)
                {
                    Rectangle rect = new Rectangle();
                    suggestedActions[rect] = action;
                    rect.Width = gm.SquareSize;
                    rect.Height = gm.SquareSize;
                    SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(255, 241, 145));
                    brush.Opacity = 0.5;
                    rect.Fill = brush;
                    Point loc = gm.GetLocationOfPos(action.To);
                    Canvas.SetLeft(rect, loc.X);
                    Canvas.SetTop(rect, loc.Y);
                    Canvas.SetZIndex(rect, 6);
                    rect.Cursor = Cursors.Hand;
                    rect.MouseUp += SuggestedAction_MouseUp;
                    boardCanvas.Children.Add(rect);
                }
            }
        }

        private void SuggestedAction_MouseUp(object sender, MouseButtonEventArgs e)
        {
            GameAction action = suggestedActions[(Rectangle)sender];
            gm.PerformAction(action);
        }

        public void Deselect()
        {
            legalActions = null;
            HideSuggestedActions();
        }

        private void HideSuggestedActions()
        {
            if (suggestedActions != null)
            {
                foreach (Rectangle rect in suggestedActions.Keys)
                {
                    boardCanvas.Children.Remove(rect);
                }
            }
            suggestedActions = null;
        }

        private Point GetLocation()
        {
            return gm.GetLocationOfPos(Position);
        }

        public void UpdatePosition(int pos)
        {
            Position = pos;
            Point loc = GetLocation();
            Canvas.SetLeft(pieceRect, loc.X);
            Canvas.SetTop(pieceRect, loc.Y);
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

        public void Remove()
        {
            boardCanvas.Children.Remove(pieceRect);
        }
    }
}
