using System;
using System.Collections.Generic;
using System.Text;

namespace ChessMoveGeneration.Moves
{
    /// <summary>
    /// A description of a Move, for use by the GUI.
    /// </summary>
    public class GameAction
    {
        private GameState gameState;
        private Move move;
        public int From { get; }
        public int To { get; }
        public int? Capture { get; }
        public PieceType? ChangeType { get; }
        public GameAction ExtraAction { get; }

        public GameAction(GameState gameState, Move move, int? capture = null, PieceType? changeType = null, GameAction extraAction = null)
        {
            this.gameState = gameState;
            this.move = move;
            From = BitboardUtils.BitScanForward(move.From);
            To = BitboardUtils.BitScanForward(move.To);
            Capture = capture;
            ChangeType = changeType;
            ExtraAction = extraAction;
        }

        public void Perform()
        {
            gameState.MakeMove(move);
        }

        public void Undo()
        {
            gameState.UnmakeMove(move);
        }
    }
}
