using System;
using System.Collections.Generic;
using System.Text;

namespace ChessMoveGeneration
{
    public static class ExtensionMethods
    {
        public static string PosToString(this int pos)
        {
            int file = BitboardUtils.GetFile(pos);
            int rank = BitboardUtils.GetRank(pos);
            char c = (char)(97 + file);
            return c.ToString() + (rank + 1);
        }

        public static string PosToString(this ulong bb)
        {
            int sqInd = BitboardUtils.BitScanForward(bb);
            return sqInd.PosToString();
        }
    }
}
