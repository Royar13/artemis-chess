﻿using Artemis.Core.Moves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Artemis.GUI
{
    class LastMoveHighlight
    {
        private GameManager gm;
        private Canvas boardCanvas;
        private Rectangle lastMoveFrom;
        private Rectangle lastMoveTo;

        public LastMoveHighlight(GameManager gm, Canvas boardCanvas)
        {
            this.gm = gm;
            this.boardCanvas = boardCanvas;
        }

        private void Initialize()
        {
            lastMoveFrom = new Rectangle();
            lastMoveTo = new Rectangle();
            lastMoveFrom.Width = lastMoveFrom.Height = lastMoveTo.Width = lastMoveTo.Height = gm.SquareSize;
            SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(235, 184, 101));
            brush.Opacity = 0.6;
            lastMoveFrom.Fill = lastMoveTo.Fill = brush;
            Canvas.SetZIndex(lastMoveFrom, 1);
            Canvas.SetZIndex(lastMoveTo, 1);
            boardCanvas.Children.Add(lastMoveFrom);
            boardCanvas.Children.Add(lastMoveTo);
        }

        public void Show(GameAction action)
        {
            if (lastMoveFrom == null)
            {
                Initialize();
            }
            Point fromLoc = gm.GetLocationOfPos(action.From);
            Canvas.SetLeft(lastMoveFrom, fromLoc.X);
            Canvas.SetTop(lastMoveFrom, fromLoc.Y);
            Point toLoc = gm.GetLocationOfPos(action.To);
            Canvas.SetLeft(lastMoveTo, toLoc.X);
            Canvas.SetTop(lastMoveTo, toLoc.Y);
        }
    }
}