using Artemis.Core;
using Artemis.Core.AI;
using Artemis.Core.AI.Opening;
using Artemis.Core.AI.Transposition;
using Artemis.Core.FormatConverters;
using Artemis.Core.Moves;
using Artemis.Core.Moves.PregeneratedAttacks;
using Artemis.GUI.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Artemis.GUI
{
    class GameManager : INotifyPropertyChanged
    {
        private ZobristHashUtils zobristHashUtils = new ZobristHashUtils();
        private PregeneratedAttacksData pregeneratedAttacks = new PregeneratedAttacksData();
        OpeningBook openingBook;
        private GameState gameState;
        private ArtemisEngine engine;
        private Canvas boardCanvas;
        private List<UIPiece> uiPieces;
        private UIPiece selectedPiece;
        private LastMoveHighlight lastMoveHighlight;
        private MovesHistory movesHistory;
        private IFormatConverter fenConverter = new FENConverter();
        private CancellationTokenSource cts;
        private Task<Move> engineTask;
        private IEngineConfig engineConfig;
        private ISettings settings;
        private bool ignoreDraw = false;
        public bool GameEnded { get; private set; }
        public List<GameAction> LegalActions;
        private string fen;
        public string FEN
        {
            get { return fen; }
            set
            {
                fen = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FEN"));
            }
        }
        private Brush turnColor;
        public Brush TurnColor
        {
            get { return turnColor; }
            set
            {
                turnColor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TurnColor"));
            }
        }
        private bool displayEngineTurn = false;
        public bool DisplayEngineTurn
        {
            get { return displayEngineTurn; }
            set
            {
                displayEngineTurn = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DisplayEngineTurn"));
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

        public GameManager(Canvas boardCanvas, MovesHistory movesHistory, ISettings settings)
        {
            this.boardCanvas = boardCanvas;
            this.movesHistory = movesHistory;
            this.settings = settings;
            engineConfig = settings;
            boardCanvas.Background = new ImageBrush
            {
                ImageSource = GetImage("board.png")
            };
            lastMoveHighlight = new LastMoveHighlight(this, boardCanvas);
            gameState = new GameState(pregeneratedAttacks, zobristHashUtils);
            openingBook = new OpeningBook(pregeneratedAttacks, zobristHashUtils);
            engine = new ArtemisEngine(gameState, engineConfig, openingBook);
        }

        public async Task NewGame(string fen = null)
        {
            await CancelEngineCalc();
            LazyInitialize();
            if (fen == null)
            {
                gameState.LoadPosition();
            }
            else
            {
                gameState.LoadPosition(fen);
            }
            UpdateFEN();
            ClearBoard();
            movesHistory.Reset();
            lastMoveHighlight.Initialize();
            var pieces = gameState.GetPiecesList();
            uiPieces = new List<UIPiece>();
            foreach (var piece in pieces)
            {
                CreatePiece(piece.Item3, piece.Item2, piece.Item1);
            }
            GameEnded = false;
            ignoreDraw = false;
            await StartTurn();
        }

        private void LazyInitialize()
        {
            if (!pregeneratedAttacks.IsInitialized)
            {
                pregeneratedAttacks.Initialize();
            }
            if (!openingBook.IsLoaded)
            {
                openingBook.Load();
            }
        }

        private UIPiece CreatePiece(PieceType pieceType, int pl, int position)
        {
            UIPiece piece = new UIPiece(pieceType, pl, position, gameState, this, boardCanvas);
            piece.PieceSelected += UiPiece_PieceSelected;
            piece.Create();
            uiPieces.Add(piece);
            return piece;
        }

        private void ClearBoard()
        {
            boardCanvas.Children.Clear();
            lastMoveHighlight.Hide();
        }

        public async Task StartTurn()
        {
            Color color = gameState.Turn == 0 ? Colors.White : Colors.Black;
            TurnColor = new SolidColorBrush(color);
            DisplayEngineTurn = settings.PlayerType[gameState.Turn] == InputSource.Engine;

            if (settings.PlayerType[gameState.Turn] == InputSource.Player)
            {
                LegalActions = gameState.GetLegalMoves().Select(m => m.GetAction()).ToList();
            }
            else
            {
                //engine
                using (cts = new CancellationTokenSource())
                {
                    engineTask = engine.Calculate(cts.Token);
                    Move move = await engineTask;
                    engineTask = null;
                    if (!cts.IsCancellationRequested)
                    {
                        await PerformAction(move.GetAction());
                    }
                }
            }
        }

        public async Task EndTurn()
        {
            lastMoveHighlight.Show(movesHistory.Actions.Last());
            DisplayEngineTurn = false;
            UpdateFEN();
            GameResultData resultData = gameState.GetResult();
            if (resultData.Result == GameResult.Ongoing)
            {
                await StartTurn();
            }
            else if (resultData.Result == GameResult.Win)
            {
                string color = resultData.Winner == 0 ? "White" : "Black";
                MessageBox.Show($"{color} won by checkmate!");
                GameEnded = true;
            }
            else
            {
                if (resultData.Reason == GameResultReason.Stalemate)
                {
                    MessageBox.Show($"Draw by stalemate");
                    GameEnded = true;
                }
                else if (!ignoreDraw)
                {
                    string drawMessage;
                    if (resultData.Reason == GameResultReason.ThreefoldRepetition)
                    {
                        drawMessage = "Draw by threefold repetition";
                    }
                    else
                    {
                        drawMessage = "Draw by 50-move rule";
                    }
                    MessageBoxResult result = MessageBox.Show($"{drawMessage}.\nDo you wish to continue?", "Draw", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.No)
                    {
                        GameEnded = true;
                    }
                    else
                    {
                        ignoreDraw = true;
                        await StartTurn();
                    }
                }
                else
                {
                    await StartTurn();
                }
            }
        }

        private void UpdateFEN()
        {
            FEN = fenConverter.Convert(gameState);
        }

        public async Task<bool> LoadFen(string fenTxt)
        {
            if (!fenConverter.IsValid(fenTxt))
            {
                MessageBox.Show("Invalid FEN");
                return false;
            }
            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure you want to load a new position?", "Load FEN", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                await NewGame(fenTxt);
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
            if (settings.BottomColor == 0)
            {
                rank = GameState.BOARD_SIZE - rank - 1;
            }
            else
            {
                file = GameState.BOARD_SIZE - file - 1;
            }
            p.X = SquareSize * file;
            p.Y = SquareSize * rank;
            return p;
        }

        public async Task UpdateSettings(ISettings updatedSettings)
        {
            await CancelEngineCalc();
            if (selectedPiece != null)
            {
                selectedPiece.Deselect();
            }
            ISettings oldSettings = settings;
            engineConfig.Update(updatedSettings);
            settings = updatedSettings;
            if (updatedSettings.BottomColor != oldSettings.BottomColor)
            {
                UpdateBottomColor();
            }
            await StartTurn();
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

        public async Task PerformAction(GameAction action)
        {
            action.Perform();
            movesHistory.AddAction(action);
            if (selectedPiece != null)
            {
                selectedPiece.Deselect();
            }
            UpdatePiecesAfterAction(action);
            await EndTurn();
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

        private async Task CancelEngineCalc()
        {
            if (engineTask != null)
            {
                cts.Cancel();
                await engineTask;
            }
        }

        public async Task Undo()
        {
            if (movesHistory.Actions.Count == 0)
                return;

            await CancelEngineCalc();

            if (settings.PlayerType[1 - gameState.Turn] == InputSource.Engine && settings.PlayerType[gameState.Turn] == InputSource.Player && movesHistory.Actions.Count > 1)
            {
                UndoAction();
            }
            UndoAction();
            UpdateFEN();
            GameEnded = false;
            await StartTurn();
        }

        protected void UndoAction()
        {
            GameAction action = movesHistory.Actions.Last();
            action.Undo();
            movesHistory.RemoveAction();
            if (selectedPiece != null)
            {
                selectedPiece.Deselect();
            }
            UpdatePiecesAfterActionUndo(action);
            if (movesHistory.Actions.Count > 0)
            {
                lastMoveHighlight.Show(movesHistory.Actions.Last());
            }
            else
            {
                lastMoveHighlight.Hide();
            }
        }

        public void UpdatePiecesAfterActionUndo(GameAction action)
        {
            UIPiece piece = GetPieceByPos(action.To);
            piece.UpdatePosition(action.From);

            if (action.Capture != null)
            {
                ulong capturedBB = BitboardUtils.GetBitboard(action.Capture.Value);
                PieceType capturedPieceType = gameState.GetPieceBySquare(1 - gameState.Turn, capturedBB).Value;
                CreatePiece(capturedPieceType, 1 - gameState.Turn, action.Capture.Value);
            }

            if (action.ChangeType != null)
            {
                ulong fromBB = BitboardUtils.GetBitboard(action.From);
                PieceType originalPieceType = gameState.GetPieceBySquare(gameState.Turn, fromBB).Value;
                piece.ChangeType(originalPieceType);
            }

            if (action.ExtraAction != null)
            {
                UpdatePiecesAfterActionUndo(action.ExtraAction);
            }
        }

        public void UpdateBottomColor()
        {
            foreach (UIPiece piece in uiPieces)
            {
                piece.UpdatePosition();
            }
            lastMoveHighlight.UpdatePosition();
        }
    }
}
