using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.AI.Evaluation
{
    public class Modifier
    {
        int[] values = new int[3];

        public Modifier(int value)
        {
            values[0] = values[1] = values[2] = value;
        }

        public Modifier(int opening, int middlegame, int endgame)
        {
            values[0] = opening;
            values[1] = middlegame;
            values[2] = endgame;
        }

        public int Get(GameStage stage)
        {
            return values[(int)stage];
        }
    }
}
