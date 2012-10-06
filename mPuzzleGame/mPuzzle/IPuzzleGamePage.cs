using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mPuzzle
{
    public interface IPuzzleGamePage
    {
        void updateTime(long seconds);
        void updateMoves(int moves);
        void puzzleAssembled();
    }
}
