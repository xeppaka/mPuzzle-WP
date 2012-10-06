using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mPuzzle
{
    public interface IAnimationCallback
    {
        void animationFinished(int dx, int dy);
        void animationFinishedSimple();
    }
}
