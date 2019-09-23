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
        GameState gameState;
        Canvas boardCanvas;
        List<UIPiece> uiPieces;
        UIPiece selectedPiece;
        public bool GameEnded;
        public List<GameAction> LegalActions;
        public InputSource[] PlayerType { get; } = { InputSource.Player, InputSource.Engine };
        ArtemisEngine engine;
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
            engine = new ArtemisEngine(gameState);
            boardCanvas.Children.Clear();
            MovesList = "";
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
