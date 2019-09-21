using Artemis.Core;
using Artemis.Core.Moves;
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

namespace Artemis.GUI
{
    class UIPiece
    {
        public PieceType PieceType { get; private set; }
        public int Pl { get; private set; }
        public int Position { get; private set; }

        GameState gameState;
        GameManager gm;
        Canvas boardCanvas;
        Rectangle pieceRect;
        Dictionary<Rectangle, GameAction> suggestedPromotionActions;
        Border promotionPanel;
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
            if (action.ChangeType == null)
            {
                gm.PerformAction(action);
            }
            else
            {
                ShowPromotionMenu(action);
            }
        }

        private void ShowPromotionMenu(GameAction pAction)
        {
            List<GameAction> promotionActions = legalActions.Where(m => m.To.Equals(pAction.To)).ToList();
            double btnSize = gm.SquareSize / 2;
            double panelSize = btnSize * 4;
            Point loc = gm.GetLocationOfPos(pAction.To);
            loc.X += gm.SquareSize / 2 - panelSize / 2;
            loc.X = Math.Max(loc.X, 0);
            loc.X = Math.Min(loc.X, boardCanvas.Width - panelSize);
            promotionPanel = new Border();
            promotionPanel.BorderThickness = new Thickness(1);
            promotionPanel.BorderBrush = new SolidColorBrush(Colors.Black);
            Canvas.SetLeft(promotionPanel, loc.X);
            Canvas.SetTop(promotionPanel, loc.Y);
            Canvas.SetZIndex(promotionPanel, 7);

            WrapPanel promotionPanelWrap = new WrapPanel();
            promotionPanelWrap.Width = panelSize;
            SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            brush.Opacity = 0.7;
            promotionPanelWrap.Background = brush;
            promotionPanel.Child = promotionPanelWrap;
            suggestedPromotionActions = new Dictionary<Rectangle, GameAction>();
            foreach (GameAction action in promotionActions)
            {
                Rectangle rect = new Rectangle();
                rect.Width = btnSize;
                rect.Height = btnSize;
                rect.Fill = new ImageBrush
                {
                    ImageSource = GetImageOfPiece(action.ChangeType.Value)
                };
                rect.Cursor = Cursors.Hand;
                rect.MouseUp += PromotionAction_MouseUp;
                rect.Margin = new Thickness(0, 3, 0, 3);
                promotionPanelWrap.Children.Add(rect);
                suggestedPromotionActions[rect] = action;
            }
            boardCanvas.Children.Add(promotionPanel);
            HideSuggestedActions();
        }

        private void PromotionAction_MouseUp(object sender, MouseButtonEventArgs e)
        {
            GameAction action = suggestedPromotionActions[(Rectangle)sender];
            PerformPromotionAction(action);
        }

        private void PerformPromotionAction(GameAction action)
        {
            boardCanvas.Children.Remove(promotionPanel);
            promotionPanel = null;
            gm.PerformAction(action);
        }

        public void Deselect()
        {
            legalActions = null;
            HideSuggestedActions();
            if (promotionPanel != null)
            {
                boardCanvas.Children.Remove(promotionPanel);
                promotionPanel = null;
            }
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

        public void ChangeType(PieceType type)
        {
            PieceType = type;
            pieceRect.Fill = new ImageBrush
            {
                ImageSource = GetImage()
            };
        }
    }
}
