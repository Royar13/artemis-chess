using Artemis.Core.AI.Evaluation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.AI
{
    public class EngineConfig : IEngineConfig
    {
        public virtual bool ConstantDepth { get; set; } = false;
        public virtual int Depth { get; set; } = 7;
        public virtual int TimeLimit { get; set; } = 4500;
        public virtual int NullMoveDepthReduction { get; set; } = 2;
        public virtual bool Multithreading { get; set; } = true;
        public virtual bool UseOpeningBook { get; set; } = true;

        public void Update(IEngineConfig config)
        {
            ConstantDepth = config.ConstantDepth;
            Depth = config.Depth;
            TimeLimit = config.TimeLimit;
            NullMoveDepthReduction = config.NullMoveDepthReduction;
            Multithreading = config.Multithreading;
            UseOpeningBook = config.UseOpeningBook;
        }
    }
}
