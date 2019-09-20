using System;
using System.Collections.Generic;
using System.Text;

namespace ChessMoveGeneration.Moves
{
    /// <summary>
    /// A summary of a Move, for use by the GUI.
    /// </summary>
    public class GameAction
    {
        public int From { get; }
        public int To { get; }
        public int? Capture { get; }
        public PieceType? CaptureType { get; }
        public PieceType? ChangeType { get; }
        public GameAction ExtraAction { get; }

        public GameAction(int from, int to, int? capture = null, PieceType? captureType = null, PieceType? changeType = null, GameAction extraAction = null)
        {
            From = from;
            To = to;
            Capture = capture;
            CaptureType = captureType;
            ChangeType = changeType;
            ExtraAction = extraAction;
        }
    }
}
