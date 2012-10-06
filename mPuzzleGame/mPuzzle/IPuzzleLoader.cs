using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mPuzzle
{
    public interface IPuzzleLoader
    {
        void loadStarted();
        void loadFinished();
        void reportProgress(int progress);
    }
}
