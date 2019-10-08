using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.AI
{
    public interface IEngineConfig
    {
        bool ConstantDepth { get; }
        int Depth { get; }
        /// <summary>
        /// Time limit per move in milliseconds.
        /// </summary>
        int TimeLimit { get; }
        /// <summary>
        /// Minimal search depth to start using multiple threads.
        /// </summary>
        int MinimalDepthMultithreading { get; }
    }
}
