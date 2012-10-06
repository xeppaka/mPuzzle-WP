using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Threading;

namespace mPuzzle
{
    public enum GameState
    {
        CREATED,
        RUNNING,
        PAUSED,
        STOPPED
    }

    public class GameThread
    {
        private Thread gameThread;
        private GameState state;
        private IUpdateable updateObject;
        private long startTicks;
        private long startPause;

        // all puzzle data

        public GameThread(IUpdateable obj)
        {
            updateObject = obj;
        }

        public void initialize()
        {
            state = GameState.CREATED;
        }

        public void start(bool paused)
        {
            if (state != GameState.CREATED)
                return;
            gameThread = new Thread(new ThreadStart(gameLoop));
            startTicks = DateTime.Now.Ticks;
            if (paused)
            {
                state = GameState.PAUSED;
                startPause = DateTime.Now.Ticks;
            }
            else
                state = GameState.RUNNING;
            gameThread.Start();
        }

        public void pause()
        {
            if (state == GameState.RUNNING)
            {
                lock (this)
                {
                    state = GameState.PAUSED;
                    startPause = DateTime.Now.Ticks;
                    Monitor.Wait(this);
                }
            }
        }

        public void resume()
        {
            if (state != GameState.PAUSED)
                return;
            lock (this)
            {
                Monitor.Pulse(this);
                state = GameState.RUNNING;
                startTicks += DateTime.Now.Ticks - startPause;
            }
        }

        public void stop()
        {
        }

        private void gameLoop()
        {
            while (state != GameState.STOPPED)
            {
                if (state == GameState.PAUSED)
                {
                    lock (this)
                    {
                        Monitor.Pulse(this);
                        Monitor.Wait(this);
                    }
                }

                lock (this)
                {
                    updateObject.update();
                }

                Thread.Sleep(70);
            }
        }

        public GameState getState()
        {
            return state;
        }

        public long getRunTime()
        {
            return DateTime.Now.Ticks - startTicks;
        }
    }
}
