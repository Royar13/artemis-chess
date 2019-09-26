using Artemis.Core.Moves;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artemis.GUI
{
    class MovesHistory : INotifyPropertyChanged
    {
        public List<GameAction> Actions { get; private set; } = new List<GameAction>();
        private string movesList;
        public string MovesList
        {
            get
            {
                return movesList;
            }
            set
            {
                movesList = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MovesList"));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private string GetMovesList()
        {
            List<string> moves = new List<string>();
            int moveNum = 1;
            for (int i = 0; i < Actions.Count; i += 2)
            {
                GameAction wAction = Actions[i];
                string move = $"{moveNum}. {wAction.ToString()}";
                if (i + 1 < Actions.Count)
                {
                    GameAction bAction = Actions[i + 1];
                    move += " " + bAction.ToString();
                }
                moves.Add(move);
                moveNum++;
            }
            return string.Join("\n", moves);
        }

        public void AddAction(GameAction action)
        {
            Actions.Add(action);
            MovesList = GetMovesList();
        }

        public void RemoveAction()
        {
            Actions.RemoveAt(Actions.Count - 1);
            MovesList = GetMovesList();
        }

        public void Reset()
        {
            Actions = new List<GameAction>();
            MovesList = "";
        }
    }
}
