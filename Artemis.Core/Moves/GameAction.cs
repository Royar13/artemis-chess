using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Artemis.Core.Moves
{
    /// <summary>
    /// A description of a Move, for use by the GUI.
    /// </summary>
    public class GameAction
    {
        private GameState gameState;
        private Move move;
        private string moveStr;
        public int From { get; }
        public int To { get; }
        public int? Capture { get; }
        public PieceType? ChangeType { get; }
        public GameAction ExtraAction { get; }

        public GameAction(GameState gameState, Move move, int from, int to, int? capture = null, PieceType? changeType = null, GameAction extraAction = null)
        {
            this.gameState = gameState;
            this.move = move;
            From = from;
            To = to;
            Capture = capture;
            ChangeType = changeType;
            ExtraAction = extraAction;
        }

        public void Perform()
        {
            moveStr = move.GetPgnNotation();
            gameState.MakeMove(move);
            if (gameState.IsCheck())
            {
                if (!gameState.HasLegalMove())
                {
                    moveStr += '#';
                }
                else
                {
                    moveStr += '+';
                }
            }
        }

        public void Undo()
        {
            gameState.UnmakeMove(move);
        }

        /// <summary>
        /// Should be called after the action is made
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return moveStr;
        }
    }
}
