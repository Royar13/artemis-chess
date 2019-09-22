using Artemis.Core.Moves;
using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.AI.Search
{
    interface IMoveSearch
    {
        Move CalculateMove();
    }
}
