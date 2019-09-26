using Artemis.Core;
using Artemis.Core.AI;
using Artemis.Core.Moves;
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

namespace Artemis.GUI
{
    class GameManager
    {
        private GameState gameState;
        private ArtemisEngine engine;
        private Canvas boardCanvas;
        private List<UIPiece> uiPieces;
        private UIPiece selectedPiece;
        private LastMoveHighlight lastMoveHighlight;
        private MovesHistory movesHistory;
        public InputSource[] PlayerType { get; } = { InputSource.Player, InputSource.Player };
        public bool GameEnded { get; private set; }
        public List<GameAction> LegalActions;

        public double SquareSize
        {
            get
            {
                return boardCanvas.Width / GameState.BOARD_SIZE;
            }
        }

        public GameManager(Canvas boardCanvas, MovesHistory movesHistory)
        {
            this.boardCanvas = boardCanvas;
            this.movesHistory = movesHistory;
            boardCanvas.Background = new ImageBrush
            {
                ImageSource = GetImage("board.png")
            };
            lastMoveHighlight = new LastMoveHighlight(this, boardCanvas);
        }

        public void NewGame()
        {
            gameState = new GameState();
            engine = new ArtemisEngine(gameState);
            boardCanvas.Children.Clear();
            movesHistory.Reset();
            var pieces = gameState.GetPiecesList();
            uiPieces = pieces.Select(p => new UIPiece(p.Item3, p.Item2, p.Item1, gameState, this, boardCanvas)).ToList();
            foreach (UIPiece piece in uiPieces)
            {
                piece.PieceSelected += UiPiece_PieceSelected;
                piece.Create();
            }
            StartTurn();
        }

        public async Task StartTurn()
        {
            if (PlayerType[gameState.Turn] == InputSource.Player)
            {
                LegalActions = gameState.GetLegalMoves().Select(m => m.GetAction()).ToList();
            }
            else
            {
                //engine
                Move move = await engine.Calculate();
                PerformAction(move.GetAction());
            }
        }

        public void EndTurn()
        {
            lastMoveHighlight.Show(movesHistory.Actions.Last());
            GameResult result = gameState.GetResult();
            if (result == GameResult.Ongoing)
            {
                StartTurn();
            }
            else
            {
                if (result == GameResult.Checkmate)
                {
                    string color = (1 - gameState.Turn) == 0 ? "White" : "Black";
                    MessageBox.Show($"{color} won by checkmate!");
                }
                else
                {
                    MessageBox.Show($"Draw by stalemate");
                }
                GameEnded = true;
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

        private void UiPiece_PieceSelected(object sender, EventArgs e)
        {
            if (selectedPiece != null)
            {
                selectedPiece.Deselect();
            }
            selectedPiece = (UIPiece)sender;
        }

        private UIPiece GetPieceByPos(int pos)
        {
            return uiPieces.FirstOrDefault(p => p.Position == pos);
        }

        private void CapturePiece(int pos)
        {
            UIPiece piece = GetPieceByPos(pos);
            uiPieces.Remove(piece);
            piece.Remove();
        }

        public void PerformAction(GameAction action)
        {
            action.Perform();
            movesHistory.AddAction(action);
            selectedPiece.Deselect();
            UpdatePiecesAfterAction(action);
            EndTurn();
        }

        public void UpdatePiecesAfterAction(GameAction action)
        {
            if (action.Capture != null)
            {
                CapturePiece(action.Capture.Value);
            }

            UIPiece piece = GetPieceByPos(action.From);
            piece.UpdatePosition(action.To);
            if (action.ChangeType != null)
            {
                piece.ChangeType(action.ChangeType.Value);
            }

            if (action.ExtraAction != null)
            {
                UpdatePiecesAfterAction(action.ExtraAction);
            }
        }
    }
}
