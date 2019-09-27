using Artemis.Core;
using Artemis.Core.AI;
using Artemis.Core.FormatConverters;
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
    class GameManager : INotifyPropertyChanged
    {
        private GameState gameState;
        private ArtemisEngine engine;
        private Canvas boardCanvas;
        private List<UIPiece> uiPieces;
        private UIPiece selectedPiece;
        private LastMoveHighlight lastMoveHighlight;
        private MovesHistory movesHistory;
        private IFormatConverter fenConverter = new FENConverter();
        public InputSource[] PlayerType { get; } = { InputSource.Player, InputSource.Engine };
        public bool GameEnded { get; private set; }
        public List<GameAction> LegalActions;
        private string fen;
        public string FEN
        {
            get
            {
                return fen;
            }
            set
            {
                fen = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FEN"));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

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

        public void NewGame(string fen = null)
        {
            if (fen == null)
            {
                gameState = new GameState();
            }
            else
            {
                gameState = new GameState(fen);
            }
            engine = new ArtemisEngine(gameState);
            UpdateFEN();
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
            UpdateFEN();
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

        private void UpdateFEN()
        {
            FEN = fenConverter.Convert(gameState);
        }

        public bool LoadFen(string fenTxt)
        {
            if (!fenConverter.IsValid(fenTxt))
            {
                MessageBox.Show("Invalid FEN");
                return false;
            }
            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure you want to load a new position?", "Load FEN", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                NewGame(fenTxt);
                return true;
            }
            else
            {
                return false;
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
            if (selectedPiece != null)
            {
                selectedPiece.Deselect();
            }
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
