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
using System.Collections.Generic;

namespace mPuzzle
{
    enum InputState
    {
        RELEASED_NO_SELECTION,
        RELEASED_SELECTED,
        SELECTING_ELEMENTS,
        MOVING_ELEMENTS,
        ANIMATION
    }

    public class GameController : IUpdateable, IAnimationCallback
    {
        private static readonly int UPDATE_UI_TICKS = 1;

        private int[,] randomMask;
        private int elementsX, elementsY;
        private Puzzle puzzle;
        private GameViewController gameViewController;
        private IPuzzleGamePage gamePage;
        private GameThread thread;
        private long tick;
        private int selFromX, selToX, selFromY, selToY;
        private bool lastSelOneItem;
        private int lastSelX, lastSelY;

        // game
        private long startTime;
        private long mseconds;
        private int moves;

        // input
        private InputState inputState;

        // animation
        private bool gameInputLock;

        private bool assembled;

        public GameController(int elx, int ely, Puzzle p, GameViewController gvc, IPuzzleGamePage pgp)
        {
            elementsX = elx;
            elementsY = ely;
            puzzle = p;
            gameViewController = gvc;
            gamePage = pgp;
            inputState = InputState.RELEASED_NO_SELECTION;

            randomize(elx, ely);
            gameViewController.setRandomization(randomMask);

            assembled = false;

            startTime = 0;
            thread = new GameThread(this);
        }

        public void tombstone(IDictionary<string, object> state)
        {
            if (state.ContainsKey("selFromX"))
                state.Remove("selFromX");
            if (state.ContainsKey("selToX"))
                state.Remove("selToX");
            if (state.ContainsKey("selFromY"))
                state.Remove("selFromY");
            if (state.ContainsKey("selToY"))
                state.Remove("selToY");
            if (state.ContainsKey("lastSelOneItem"))
                state.Remove("lastSelOneItem");
            if (state.ContainsKey("lastSelX"))
                state.Remove("lastSelX");
            if (state.ContainsKey("lastSelY"))
                state.Remove("lastSelY");
            if (state.ContainsKey("mseconds"))
                state.Remove("mseconds");
            if (state.ContainsKey("moves"))
                state.Remove("moves");
            if (state.ContainsKey("inputState"))
                state.Remove("inputState");
            if (state.ContainsKey("assembled"))
                state.Remove("assembled");

            for (int i = 0; i < elementsX * elementsY; ++i)
            {
                if (state.ContainsKey("randMask" + i))
                    state.Remove("randMask" + i);
                state["randMask" + i] = randomMask[i, 0];
            }

            state["selFromX"] = selFromX;
            state["selToX"] = selToX;
            state["selFromY"] = selFromY;
            state["selToY"] = selToY;
            state["lastSelOneItem"] = lastSelOneItem;
            state["lastSelX"] = lastSelX;
            state["lastSelY"] = lastSelY;
            state["mseconds"] = mseconds;
            state["moves"] = moves;
            state["assembled"] = assembled;
            switch (inputState)
            {
                case InputState.ANIMATION:
                    inputState = InputState.RELEASED_NO_SELECTION;
                    break;
                case InputState.MOVING_ELEMENTS:
                case InputState.SELECTING_ELEMENTS:
                    inputState = InputState.RELEASED_SELECTED;
                    break;
            }
            state["inputState"] = inputState;
        }

        public void digout(IDictionary<string, object> state)
        {
            for (int i = 0; i < elementsX * elementsY; ++i)
            {
                randomMask[i, 0] = (int)state["randMask" + i];
            }
            gameViewController.setRandomization(randomMask);

            selFromX = (int)state["selFromX"];
            selToX = (int)state["selToX"];
            selFromY = (int)state["selFromY"];
            selToY = (int)state["selToY"];
            lastSelOneItem = (bool)state["lastSelOneItem"]; 
            lastSelX = (int)state["lastSelX"];
            lastSelY = (int)state["lastSelY"];
            mseconds = (long)state["mseconds"];
            startTime = mseconds;
            moves = (int)state["moves"];
            inputState = (InputState)state["inputState"];
            assembled = (bool)state["assembled"];

            if (!assembled)
                gameViewController.shadowViews();
        }

        private void randomize(int elx, int ely)
        {
            int len = elx * ely;
            randomMask = new int[len, 2];
            int placesLeft;
            int[] places = new int[len];

            //for (int i = 0; i < len; ++i)
            //{
            //    randomMask[i, 0] = i;
            //    randomMask[i, 1] = -1;
            //    places[i] = 0;
            //}

            //randomMask[0, 0] = 1;
            //randomMask[1, 0] = 0;

            while (true)
            {
            l1:
                Random rand = new Random();
                placesLeft = len;
                for (int i = 0; i < len; ++i)
                {
                    randomMask[i, 1] = randomMask[i, 0] = -1;
                    places[i] = 0;
                }

                for (int i = 0; i < len; ++i)
                {
                    int retry = 0;
                    while (true)
                    {
                        if (++retry > 10)
                            goto l1;
                        int place = rand.Next(placesLeft);
                        int pos = -1;
                        while (place >= 0)
                        {
                            ++pos;
                            if (places[pos] == 0)
                                --place;
                        }
                        place = pos;
                        // check neighboors
                        if (place - 1 >= 0 && i - 1 >= 0 && randomMask[place - 1, 0] == i - 1)
                            continue;
                        if (place - elx >= 0 && i - elx >= 0 && randomMask[place - elx, 0] == i - elx)
                            continue;
                        randomMask[place, 0] = i;
                        places[place] = 1;
                        --placesLeft;
                        break;
                    }
                }
                break;
            }
        }

        public void update()
        {
            if (++tick % UPDATE_UI_TICKS == 0)
                lock (thread)
                {
                    gameViewController.update();
                }

            long part = thread.getRunTime() / 1000000 + startTime;
            if (part > mseconds && !assembled)
            {
                mseconds = part;
                gamePage.updateTime(mseconds);
            }
        }

        public void movePiece(int x1, int y1, int x2, int y2)
        {
            int c1 = elementsX * y1 + x1;
            int c2 = elementsX * y2 + x2;

            randomMask[c2, 1] = randomMask[c2, 0];
            randomMask[c2, 0] = randomMask[c1, 1] >= 0 ? randomMask[c1, 1] : randomMask[c1, 0];
        }

        public void switchPieces(int x1, int y1, int x2, int y2)
        {
            movePiece(x1, y1, x2, y2);
            movePiece(x2, y2, x1, y1);
        }

        public void clearShadowRandomizeBuffer()
        {
            for (int i = 0; i < elementsX * elementsY; ++i)
            {
                randomMask[i, 1] = -1;
            }
        }

        public void fixSelection()
        {
            int temp;
            if (selFromX > selToX)
            {
                temp = selToX;
                selToX = selFromX;
                selFromX = temp;
            }
            if (selFromY > selToY)
            {
                temp = selToY;
                selToY = selFromY;
                selFromY = temp;
            }
        }

        public void startGame(bool paused)
        {
            thread.start(paused);
        }

        public void togglePause()
        {
            if (thread.getState() == GameState.RUNNING)
            {
                thread.pause();
                gameViewController.shadowViews();
            }
            else if (thread.getState() == GameState.PAUSED)
            {
                thread.resume();
                gameViewController.unshadowViews();
            }
        }

        public void mouseDown(int x, int y)
        {
            if (gameInputLock || thread.getState() == GameState.PAUSED)
                return;

            switch (inputState)
            {
                case InputState.RELEASED_NO_SELECTION:
                    {
                        selFromX = selToX = x / gameViewController.getCellWidth();
                        selFromY = selToY = y / gameViewController.getCellHeight();
                        if (selFromX < 0)
                            selFromX = selToX = 0;
                        if (selFromX >= elementsX)
                            selToX = --selFromX;
                        if (selFromY < 0)
                            selFromY = selToY = 0;
                        if (selFromY >= elementsY)
                            selToY = --selFromY;
                        gameViewController.startSelection(selFromX, selFromY);
                        inputState = InputState.SELECTING_ELEMENTS;
                        break;
                    }
                case InputState.SELECTING_ELEMENTS:
                    {
                        break;
                    }
                case InputState.RELEASED_SELECTED:
                    {
                        if (isPointInSelection(x, y))
                        {
                            gameViewController.startMoveSelection(x, y);
                            inputState = InputState.MOVING_ELEMENTS;
                        }
                        else
                        {
                            selFromX = selToX = x / gameViewController.getCellWidth();
                            selFromY = selToY = y / gameViewController.getCellHeight();
                            if (selFromX < 0)
                                selFromX = selToX = 0;
                            if (selFromX >= elementsX)
                                selToX = --selFromX;
                            if (selFromY < 0)
                                selFromY = selToY = 0;
                            if (selFromY >= elementsY)
                                selToY = --selFromY;
                            gameViewController.startSelection(selFromX, selFromY);
                            inputState = InputState.SELECTING_ELEMENTS;
                        }
                        break;
                    }
                case InputState.MOVING_ELEMENTS:
                    {
                        break;
                    }
                case InputState.ANIMATION:
                    {
                        break;
                    }
            }
        }

        public void mouseUp(int x, int y)
        {
            if (gameInputLock || thread.getState() == GameState.PAUSED)
                return;

            switch (inputState)
            {
                case InputState.RELEASED_NO_SELECTION:
                    {
                        break;
                    }
                case InputState.SELECTING_ELEMENTS:
                    {
                        selToX = x / gameViewController.getCellWidth();
                        selToY = y / gameViewController.getCellHeight();

                        if (selToX < 0)
                            selToX = 0;
                        if (selToX >= elementsX)
                            --selToX;
                        if (selToY < 0)
                            selToY = 0;
                        if (selToY >= elementsY)
                            --selToY;

                        gameViewController.continueSelection(selToX, selToY);
                        fixSelection();

                        if (selFromX - selToX == 0 && selFromY - selToY == 0)
                        {
                            if (lastSelOneItem)
                            {
                                lastSelOneItem = false;
                                if (lastSelX != selFromX || lastSelY != selFromY)
                                {
                                    inputState = InputState.ANIMATION;
                                    gameInputLock = true;
                                    gameViewController.stopSelection();
                                    gameViewController.startMoveTwoPiecesAnimation(lastSelX, lastSelY, this);
                                    break;
                                }
                            }
                            else
                            {
                                lastSelOneItem = true;
                                lastSelX = selFromX;
                                lastSelY = selFromY;
                            }
                        }

                        inputState = InputState.RELEASED_SELECTED;
                        break;
                    }
                case InputState.RELEASED_SELECTED:
                    {
                        break;
                    }
                case InputState.MOVING_ELEMENTS:
                    {
                        inputState = InputState.ANIMATION;
                        gameInputLock = true;
                        gameViewController.stopSelection();
                        gameViewController.startMoveAnimation(this);
                        break;
                    }
                case InputState.ANIMATION:
                    {
                        break;
                    }
            }
        }

        public void mouseMove(int x, int y)
        {
            if (gameInputLock || thread.getState() == GameState.PAUSED)
                return;

            switch (inputState)
            {
                case InputState.RELEASED_NO_SELECTION:
                    {
                        break;
                    }
                case InputState.SELECTING_ELEMENTS:
                    {
                        int elx = x / gameViewController.getCellWidth();
                        int ely = y / gameViewController.getCellHeight();
                        if (elx != selToX || ely != selToY)
                        {
                            selToX = elx;
                            selToY = ely;
                            gameViewController.continueSelection(selToX, selToY);
                        }
                        break;
                    }
                case InputState.RELEASED_SELECTED:
                    {
                        break;
                    }
                case InputState.MOVING_ELEMENTS:
                    {
                        gameViewController.continueMoveSelection(x, y);
                        break;
                    }
                case InputState.ANIMATION:
                    {
                        break;
                    }
            }
        }

        private bool isPointInSelection(int x, int y)
        {
            int sfx, sfy, stx, sty;
            int cellWidth = gameViewController.getCellWidth();
            int cellHeight = gameViewController.getCellHeight();
            if (selFromX > selToX)
            {
                sfx = selToX * cellWidth;
                stx = (selFromX + 1) * cellWidth;
            }
            else
            {
                sfx = selFromX * cellWidth;
                stx = (selToX + 1) * cellWidth;
            }

            if (selFromY > selToY)
            {
                sfy = selToY * cellHeight;
                sty = (selFromY + 1) * cellHeight;
            }
            else
            {
                sfy = selFromY * cellHeight;
                sty = (selToY + 1) * cellHeight;
            }

            if (x >= sfx && x <= stx && y >= sfy && y <= sty)
                return true;

            return false;
        }

        public void animationFinished(int dx, int dy)
        {
            gameViewController.resetPositions();

            int targetX, targetY;
            if (dx != 0 || dy != 0)
            {
                if (Math.Abs(dx) > selToX - selFromX || Math.Abs(dy) > selToY - selFromY)
                {
                    for (int y = selFromY; y <= selToY; ++y)
                    {
                        targetY = y + dy;
                        for (int x = selFromX; x <= selToX; ++x)
                        {
                            targetX = x + dx;
                            switchPieces(x, y, targetX, targetY);
                        }
                    }
                }
                else
                {
                    for (int y = selFromY; y <= selToY; ++y)
                    {
                        targetY = y + dy;
                        for (int x = selFromX; x <= selToX; ++x)
                        {
                            targetX = x + dx;

                            movePiece(x, y, targetX, targetY);

                            int dsttmpX = -1;
                            int dsttmpY = -1;

                            if (targetY < selFromY || targetY > selToY)
                            {
                                dsttmpX = targetX - dx;
                                dsttmpY = targetY;
                                if (targetY < selFromY)
                                    dsttmpY += selToY - selFromY + 1;
                                if (targetY > selToY)
                                    dsttmpY -= selToY - selFromY + 1;
                            }
                            else if (targetX > selToX || targetX < selFromX)
                            {
                                dsttmpX = targetX;
                                dsttmpY = targetY;
                                if (targetX > selToX)
                                    dsttmpX = targetX - selToX + selFromX - 1;
                                if (targetX < selFromX)
                                    dsttmpX = targetX + selToX - selFromX + 1;
                            }

                            if (dsttmpX != -1 && dsttmpY != -1)
                                movePiece(targetX, targetY, dsttmpX, dsttmpY);
                        }
                    }
                }

                clearShadowRandomizeBuffer();
                gameViewController.updateRandomization(randomMask);
                gamePage.updateMoves(++moves);

                if (checkPuzzleAssembled())
                    puzzleAssembled();
            }

            inputState = InputState.RELEASED_NO_SELECTION;
            gameInputLock = false;
            lastSelOneItem = false;
        }

        public void animationFinishedSimple()
        {
            gameViewController.resetPositions();
            switchPieces(lastSelX, lastSelY, selFromX, selFromY);
            clearShadowRandomizeBuffer();
            gameViewController.updateRandomization(randomMask);
            inputState = InputState.RELEASED_NO_SELECTION;
            gameInputLock = false;
            lastSelOneItem = false;
            gamePage.updateMoves(++moves);

            if (checkPuzzleAssembled())
                puzzleAssembled();
        }

        private bool checkPuzzleAssembled()
        {
            for (int i = 0; i < elementsX * elementsY; ++i)
            {
                if (randomMask[i, 0] != i)
                    return false;
            }

            return true;
        }

        private void puzzleAssembled()
        {
            gamePage.puzzleAssembled();
            assembled = true;
        }
    }
}
