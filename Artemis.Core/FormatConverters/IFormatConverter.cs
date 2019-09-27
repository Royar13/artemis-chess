using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.FormatConverters
{
    public interface IFormatConverter
    {
        bool IsValid(string text);
        void Load(string text, GameState gameState);
        string Convert(GameState gameState);
    }
}
