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
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Collections.Generic;

namespace mPuzzle
{
    public enum AnimType
    {
        COMPLEX = 0,
        TWO_PIECES = 1
    }

    public class GameViewController : IUpdateable
    {
        private static readonly int ANIMATION_DURATION = 400;

        private Rectangle[,] cell;
        private Rectangle selectionRect;
        // animation
        private DoubleAnimation[,] animationX;
        private DoubleAnimation[,] animationY;
        private Storyboard storyboard;
        private int completedLeft;
        // how many elements do we have
        private int elementsX, elementsY;
        // selection
        private int selFromX, selFromY, selToX, selToY;
        private bool selectionChanged;
        private bool selected;
        // cell size
        private int cellWidth, cellHeight;
        // picture shift
        private int shiftX, shiftY;
        private int maxShiftX, maxShiftY;
        private int shiftDirX, shiftDirY;
        private int[, ,] cellShift;
        private int selectionOriginX, selectionOriginY;
        private Dispatcher dispatcher;
        private IAnimationCallback animationCallbackObject;
        private Int32 dx, dy;
        // cell shift
        private int cellShiftX, cellShiftY;
        private AnimType animType;

        public GameViewController(int elX, int elY, Canvas canvas, Dispatcher d)
        {
            dispatcher = d;
            elementsX = elX;
            elementsY = elY;
            cell = new Rectangle[elX, elY];
            if (elY > elX)
            {
                cellWidth = GameParams.screenWidthPortrait / elementsX;
                cellHeight = (GameParams.screenHeightPortrait - GameParams.gameTimePanelHeight) / elementsY;
            }
            else
            {
                cellWidth = GameParams.screenWidthLandscape / elementsX;
                cellHeight = (GameParams.screenHeightLandscape - GameParams.gameTimePanelHeight) / elementsY;
            }

            shiftX = shiftY = 0;
            dx = new Int32();
            dy = new Int32();

            cellShift = new int[elX, elY, 2];

            selected = false;

            shiftDirX = shiftDirY = -1;
            createViews(canvas);
        }

        public void tombstone(IDictionary<string, object> state)
        {
            if (state.ContainsKey("selFromXV"))
                state.Remove("selFromXV");
            if (state.ContainsKey("selFromYV"))
                state.Remove("selFromYV");
            if (state.ContainsKey("selToXV"))
                state.Remove("selToXV");
            if (state.ContainsKey("selToYV"))
                state.Remove("selToYV");
            if (state.ContainsKey("shiftDirXV"))
                state.Remove("shiftDirXV");
            if (state.ContainsKey("shiftDirYV"))
                state.Remove("shiftDirYV");
            if (state.ContainsKey("cellShiftV"))
                state.Remove("cellShiftV");
            if (state.ContainsKey("selectedV"))
                state.Remove("selectedV");
            if (state.ContainsKey("maxShiftXV"))
                state.Remove("maxShiftXV");
            if (state.ContainsKey("maxShiftYV"))
                state.Remove("maxShiftYV");

            state["selFromXV"] = selFromX;
            state["selFromYV"] = selFromY;
            state["selToXV"] = selToX;
            state["selToYV"] = selToY;
            state["shiftDirXV"] = shiftDirX;
            state["shiftDirYV"] = shiftDirY;
            state["selectedV"] = selected;
            state["maxShiftXV"] = maxShiftX;
            state["maxShiftYV"] = maxShiftY;
            //state["shiftXV"] = shiftX;
            //state["shiftYV"] = shiftY;

            //for (int x = 0; x < elementsX; ++x)
            //    for (int y = 0; y < elementsY; ++y)
            //    {
            //        if (state.ContainsKey("transformV" + x + y + "_x"))
            //            state.Remove("transformV" + x + y + "_x");
            //        if (state.ContainsKey("transformV" + x + y + "_y"))
            //            state.Remove("transformV" + x + y + "_y");
            //        state["transformV" + x + y + "_x"] = cellShift[x, y, 0] + shiftX;
            //        state["transformV" + x + y + "_y"] = cellShift[x, y, 1];
            //    }
        }

        public void digout(IDictionary<string, object> state)
        {
            selFromX = (int)state["selFromXV"];
            selFromY = (int)state["selFromYV"];
            selToX = (int)state["selToXV"];
            selToY = (int)state["selToYV"];
            shiftDirX = (int)state["shiftDirXV"];
            shiftDirY = (int)state["shiftDirYV"];
            selected = (bool)state["selectedV"];
            maxShiftX = (int)state["maxShiftXV"];
            maxShiftY = (int)state["maxShiftYV"];
            //shiftX = (int)state["shiftXV"];
            //shiftY = (int)state["shiftYV"];

            //for (int x = 0; x < elementsX; ++x)
            //    for (int y = 0; y < elementsY; ++y)
            //    {
            //        cellShift[x, y, 0] = (int)state["cellShiftV" + x + y + "_0"];
            //        cellShift[x, y, 1] = (int)state["cellShiftV" + x + y + "_1"];
            //    }


            if (selected)
                selectionChanged = true;
        }

        // at this moment bitmap is not loaded yet and bitmap.PixeWidth, bitmap.PixelHeight are equal to 0
        private void createViews(Canvas canvas)
        {
            for (int x = 0;x < elementsX;++x)
                for (int y = 0; y < elementsY; ++y)
                {
                    Rectangle r = new Rectangle();
                    cell[x, y] = r;
                    r.Width = cellWidth;
                    r.Height = cellHeight;
                    r.IsHitTestVisible = false;
                    Canvas.SetZIndex(r, 1);
                    Canvas.SetLeft(r, x * cellWidth);
                    Canvas.SetTop(r, y * cellHeight);

                    ImageBrush ib = new ImageBrush();
                    ib.Stretch = Stretch.None;
                    ib.AlignmentX = AlignmentX.Left;
                    ib.AlignmentY = AlignmentY.Top;
                    TranslateTransform tt = new TranslateTransform();
                    ib.Transform = tt;
                    tt.X = tt.Y = 0;
                    r.Fill = ib;
                    r.RenderTransform = new TranslateTransform();

                    canvas.Children.Add(r);
                }
            selectionRect = new Rectangle();
            selectionRect.Visibility = Visibility.Collapsed;
            selectionRect.Stroke = new SolidColorBrush(Colors.Red);
            selectionRect.StrokeThickness = 3.0;
            selectionRect.RenderTransform = new TranslateTransform();
            Canvas.SetZIndex(selectionRect, 3);
            canvas.Children.Add(selectionRect);

            createAnimations();
        }

        public void setBitmap(BitmapImage bitmap)
        {
            for (int x = 0;x < elementsX;++x)
                for (int y = 0; y < elementsY; ++y)
                {
                    ((ImageBrush)cell[x, y].Fill).ImageSource = bitmap;
                }
        }

        private void createAnimations()
        {
            DoubleAnimation daX, daY;
            TimeSpan duration = TimeSpan.FromMilliseconds(ANIMATION_DURATION);
            storyboard = new Storyboard();
            storyboard.Duration = duration;
            animationX = new DoubleAnimation[elementsX, elementsY];
            animationY = new DoubleAnimation[elementsX, elementsY];
            for (int x = 0; x < elementsX; ++x)
                for (int y = 0; y < elementsY; ++y)
                {
                    daX = new DoubleAnimation();
                    daY = new DoubleAnimation();
                    animationX[x, y] = daX;
                    animationY[x, y] = daY;
                    daX.EasingFunction = daY.EasingFunction = new ExponentialEase();
                    daX.Duration = duration;
                    daY.Duration = duration;
                    Storyboard.SetTarget(daX, cell[x, y]);
                    Storyboard.SetTargetProperty(daX, new PropertyPath("(UIElement.RenderTransform).X"));
                    Storyboard.SetTarget(daY, cell[x, y]);
                    Storyboard.SetTargetProperty(daY, new PropertyPath("(UIElement.RenderTransform).Y"));
                    daX.Completed += onAnimationCompleted;
                    daY.Completed += onAnimationCompleted;
                }
        }

        public void update()
        {
            if (shiftX + shiftDirX < -maxShiftX || shiftX + shiftDirX > 0)
                shiftDirX = -shiftDirX;
            if (shiftY + shiftDirY < -maxShiftY || shiftY + shiftDirY > 0)
                shiftDirY = -shiftDirY;
            shiftX += shiftDirX; shiftY += shiftDirY;

            dispatcher.BeginInvoke(_update);
        }

        private void _update()
        {
            for (int x = 0; x < elementsX; ++x)
                for (int y = 0; y < elementsY; ++y)
                {
                    ((TranslateTransform)cell[x, y].Fill.Transform).X = cellShift[x, y, 0] + shiftX;
                    ((TranslateTransform)cell[x, y].Fill.Transform).Y = cellShift[x, y, 1] + shiftY;
                }

            if (selectionChanged)
            {
                selectionChanged = false;
                if (selected)
                {
                    int sfx, sfy, stx, sty;
                    if (selFromX > selToX)
                    {
                        sfx = selToX;
                        stx = selFromX;
                    }
                    else
                    {
                        sfx = selFromX;
                        stx = selToX;
                    }

                    if (selFromY > selToY)
                    {
                        sfy = selToY;
                        sty = selFromY;
                    }
                    else
                    {
                        sfy = selFromY;
                        sty = selToY;
                    }
                    Canvas.SetLeft(selectionRect, sfx * cellWidth);
                    Canvas.SetTop(selectionRect, sfy * cellHeight);
                    selectionRect.Height = (sty - sfy + 1) * cellHeight;
                    selectionRect.Width = (stx - sfx + 1) * cellWidth;
                    selectionRect.Visibility = Visibility.Visible;
                }
                else
                {
                    selectionRect.Visibility = Visibility.Collapsed;
                }
            }
        }

        // should be called in the UI thread context
        public void setBitmapWidth(int width)
        {
            maxShiftX = width - cellWidth * elementsX;
        }

        // should be called in the UI thread context
        public void setBitmapHeight(int height)
        {
            maxShiftY = height - cellHeight * elementsY;
        }

        // should be called in the UI thread context
        public void setRandomization(int[,] randomization)
        {
            int length = elementsX * elementsY;
            int x, y;
            for (int i = 0; i < length; ++i)
            {
                x = randomization[i, 0] % elementsX;
                y = randomization[i, 0] / elementsX;
                ((TranslateTransform)cell[i % elementsX, i / elementsX].Fill.Transform).X = cellShift[i % elementsX, i / elementsX, 0] = -x * cellWidth + shiftX;
                ((TranslateTransform)cell[i % elementsX, i / elementsX].Fill.Transform).Y = cellShift[i % elementsX, i / elementsX, 1] = -y * cellHeight + shiftY;
            }
        }

        public void updateRandomization(int[,] randomization)
        {
            int length = elementsX * elementsY;
            int x, y;
            for (int i = 0; i < length; ++i)
            {
                x = randomization[i, 0] % elementsX;
                y = randomization[i, 0] / elementsX;
                cellShift[i % elementsX, i / elementsX, 0] = -x * cellWidth;
                cellShift[i % elementsX, i / elementsX, 1] = -y * cellHeight;
                ((TranslateTransform)cell[i % elementsX, i / elementsX].Fill.Transform).X = cellShift[i % elementsX, i / elementsX, 0] + shiftX;
                ((TranslateTransform)cell[i % elementsX, i / elementsX].Fill.Transform).Y = cellShift[i % elementsX, i / elementsX, 1] + shiftY;
            }
        }

        public int getCellWidth()
        {
            return cellWidth;
        }

        public int getCellHeight()
        {
            return cellHeight;
        }

        private void determineNewPosShift(ref Int32 rdx, ref Int32 rdy)
        {
            int left = selFromX * cellWidth + cellShiftX;
            int top = selFromY * cellHeight + cellShiftY;
            int right = left + (selToX - selFromX + 1) * cellWidth - cellWidth / 2;
            int bottom = top + (selToY - selFromY + 1) * cellHeight - cellHeight / 2;
            left += cellWidth / 2;
            top += cellHeight / 2;
            if (left < 0 || top < 0 || right > elementsX * cellWidth || bottom > elementsY * cellHeight)
            {
                rdx = 0;
                rdy = 0;
            }
            else
            {
                rdx = left / cellWidth - selFromX;
                rdy = top / cellHeight - selFromY;
            }
        }

        // input
        public void startSelection(int elementX, int elementY)
        {
            selFromX = selToX = elementX;
            selFromY = selToY = elementY;
            selected = true;
            selectionChanged = true;
        }

        public void continueSelection(int elementX, int elementY)
        {
            selToX = elementX;
            selToY = elementY;
            selectionChanged = true;
        }

        public void stopSelection()
        {
            selected = false;
            selectionChanged = true;
        }

        public void startMoveSelection(int x, int y)
        {
            cellShiftX = 0;
            cellShiftY = 0;
            selectionOriginX = x;
            selectionOriginY = y;
            int temp;
            if (selFromX > selToX)
            {
                temp = selFromX;
                selFromX = selToX;
                selToX = temp;
            }

            if (selFromY > selToY)
            {
                temp = selFromY;
                selFromY = selToY;
                selToY = temp;
            }

            for (int i = selFromX; i <= selToX; ++i)
                for (int j = selFromY; j <= selToY; ++j)
                {
                    Canvas.SetZIndex(cell[i, j], 2);
                }
        }

        public void continueMoveSelection(int x, int y)
        {
            cellShiftX = x - selectionOriginX;
            cellShiftY = y - selectionOriginY;

            for (int i = selFromX; i <= selToX; ++i)
                for (int j = selFromY; j <= selToY; ++j)
                {
                    ((TranslateTransform)cell[i, j].RenderTransform).X = cellShiftX;
                    ((TranslateTransform)cell[i, j].RenderTransform).Y = cellShiftY;
                }

            ((TranslateTransform)selectionRect.RenderTransform).X = cellShiftX;
            ((TranslateTransform)selectionRect.RenderTransform).Y = cellShiftY;
        }

        public void stopMoveSelection()
        {
            for (int i = selFromX; i <= selToX; ++i)
                for (int j = selFromY; j <= selToY; ++j)
                {
                    ((TranslateTransform)cell[i, j].RenderTransform).X = 0;
                    ((TranslateTransform)cell[i, j].RenderTransform).Y = 0;
                    Canvas.SetZIndex(cell[i, j], 1);
                }

            ((TranslateTransform)selectionRect.RenderTransform).X = 0;
            ((TranslateTransform)selectionRect.RenderTransform).Y = 0;
        }

        public void resetPositions()
        {
            for (int i = 0; i < elementsX; ++i)
                for (int j = 0; j < elementsY; ++j)
                {
                    ((TranslateTransform)cell[i, j].RenderTransform).X = 0;
                    ((TranslateTransform)cell[i, j].RenderTransform).Y = 0;
                    Canvas.SetZIndex(cell[i, j], 1);
                }

            ((TranslateTransform)selectionRect.RenderTransform).X = 0;
            ((TranslateTransform)selectionRect.RenderTransform).Y = 0;
        }

        public void switchPieces(int x1, int y1, int x2, int y2)
        {
            double temp1, temp2;
            temp1 = ((TranslateTransform)cell[x1, y1].Fill.Transform).X;
            temp2 = ((TranslateTransform)cell[x1, y1].Fill.Transform).Y;
            ((TranslateTransform)cell[x1, y1].Fill.Transform).X = ((TranslateTransform)cell[x2, y2].Fill.Transform).X;
            ((TranslateTransform)cell[x1, y1].Fill.Transform).Y = ((TranslateTransform)cell[x2, y2].Fill.Transform).Y;
            ((TranslateTransform)cell[x2, y2].Fill.Transform).X = temp1;
            ((TranslateTransform)cell[x2, y2].Fill.Transform).Y = temp2;
        }

        public void startMoveAnimation(IAnimationCallback callback)
        {
            selectionRect.Visibility = Visibility.Collapsed;
            animationCallbackObject = callback;
            completedLeft = 0;
            storyboard.Children.Clear();

            determineNewPosShift(ref dx, ref dy);

            if (dx == 0 && dy == 0)
            {
                TimeSpan duration;
                if (dx < 0)
                    duration = TimeSpan.FromMilliseconds(ANIMATION_DURATION);
                else
                    duration = TimeSpan.FromMilliseconds(ANIMATION_DURATION / 4);

                for (int i = selFromX; i <= selToX; ++i)
                    for (int j = selFromY; j <= selToY; ++j)
                    {
                        animationX[i, j].From = ((TranslateTransform)cell[i, j].RenderTransform).X;
                        animationX[i, j].To = 0;
                        animationX[i, j].By = animationX[i, j].From > 0 ? -1 : 1;
                        animationX[i, j].Duration = duration;
                        animationY[i, j].From = ((TranslateTransform)cell[i, j].RenderTransform).Y;
                        animationY[i, j].To = 0;
                        animationY[i, j].By = animationY[i, j].From > 0 ? -1 : 1;
                        animationY[i, j].Duration = duration;
                        storyboard.Children.Add(animationX[i, j]);
                        storyboard.Children.Add(animationY[i, j]);
                        completedLeft += 2;
                    }

                animType = AnimType.COMPLEX;

                storyboard.Begin();
                return;
            }

            int targetX, targetY;
            if (Math.Abs(dx) > selToX - selFromX || Math.Abs(dy) > selToY - selFromY)
            {
                TimeSpan duration = TimeSpan.FromMilliseconds(ANIMATION_DURATION);
                for (int y = selFromY; y <= selToY; ++y)
                {
                    targetY = y + dy;
                    for (int x = selFromX; x <= selToX; ++x)
                    {
                        targetX = x + dx;
                        animationX[x, y].From = ((TranslateTransform)cell[x, y].RenderTransform).X;
                        animationX[x, y].To = dx * cellWidth;
                        animationX[x, y].By = animationX[x, y].From > animationX[x, y].To ? -1 : 1;
                        animationX[x, y].Duration = duration;
                        animationY[x, y].From = ((TranslateTransform)cell[x, y].RenderTransform).Y;
                        animationY[x, y].To = dy * cellHeight;
                        animationY[x, y].By = animationY[x, y].From > animationY[x, y].To ? -1 : 1;
                        animationY[x, y].Duration = duration;

                        animationX[targetX, targetY].From = ((TranslateTransform)cell[targetX, targetY].RenderTransform).X;
                        animationX[targetX, targetY].To = -dx * cellWidth;
                        animationX[targetX, targetY].By = animationX[targetX, targetY].From > animationX[targetX, targetY].To ? -1 : 1;
                        animationX[targetX, targetY].Duration = duration;
                        animationY[targetX, targetY].From = ((TranslateTransform)cell[targetX, targetY].RenderTransform).Y;
                        animationY[targetX, targetY].To = -dy * cellHeight;
                        animationY[targetX, targetY].By = animationY[targetX, targetY].From > animationY[targetX, targetY].To ? -1 : 1;
                        animationY[targetX, targetY].Duration = duration;

                        Canvas.SetZIndex(cell[targetX, targetY], 0);

                        storyboard.Children.Add(animationX[x, y]);
                        storyboard.Children.Add(animationY[x, y]);
                        storyboard.Children.Add(animationX[targetX, targetY]);
                        storyboard.Children.Add(animationY[targetX, targetY]);

                        completedLeft += 4;
                    }
                }
            }
            else
            {
                TimeSpan duration = TimeSpan.FromMilliseconds(ANIMATION_DURATION);
                for (int y = selFromY; y <= selToY; ++y)
                {
                    targetY = y + dy;
                    for (int x = selFromX; x <= selToX; ++x)
                    {
                        targetX = x + dx;

                        animationX[x, y].From = ((TranslateTransform)cell[x, y].RenderTransform).X;
                        animationX[x, y].To = dx * cellWidth;
                        animationX[x, y].By = animationX[x, y].From > animationX[x, y].To ? -1 : 1;
                        animationX[x, y].Duration = duration;
                        animationY[x, y].From = ((TranslateTransform)cell[x, y].RenderTransform).Y;
                        animationY[x, y].To = dy * cellHeight;
                        animationY[x, y].By = animationY[x, y].From > animationY[x, y].To ? -1 : 1;
                        animationY[x, y].Duration = duration;

                        storyboard.Children.Add(animationX[x, y]);
                        storyboard.Children.Add(animationY[x, y]);

                        completedLeft += 2;

                        int dsttmpX = -1;
                        int dsttmpY = -1;

                        if (targetY < selFromY || targetY > selToY)
                        {
                            dsttmpX = targetX - dx;
                            dsttmpY = targetY;
                            if (targetY < selFromY)
                            {
                                dsttmpY += selToY - selFromY + 1;
                            }
                            if (targetY > selToY)
                            {
                                dsttmpY -= selToY - selFromY + 1;
                            }
                        }
                        else
                            if (targetX > selToX || targetX < selFromX)
                            {
                                dsttmpX = targetX;
                                dsttmpY = targetY;
                                if (targetX > selToX)
                                {
                                    dsttmpX = targetX - selToX + selFromX - 1;
                                }
                                if (targetX < selFromX)
                                {
                                    dsttmpX = targetX + selToX - selFromX + 1;
                                }
                            }

                        if (dsttmpX != -1 && dsttmpY != -1)
                        {
                            Canvas.SetZIndex(cell[targetX, targetY], 0);

                            animationX[targetX, targetY].From = ((TranslateTransform)cell[targetX, targetY].RenderTransform).X;
                            animationX[targetX, targetY].To = (dsttmpX - targetX) * cellWidth;
                            animationX[targetX, targetY].By = animationX[targetX, targetY].From > animationX[targetX, targetY].To ? -1 : 1;
                            animationX[targetX, targetY].Duration = duration;
                            animationY[targetX, targetY].From = ((TranslateTransform)cell[targetX, targetY].RenderTransform).Y;
                            animationY[targetX, targetY].To = (dsttmpY - targetY) * cellHeight;
                            animationY[targetX, targetY].By = animationY[targetX, targetY].From > animationY[targetX, targetY].To ? -1 : 1;
                            animationY[targetX, targetY].Duration = duration;
                            storyboard.Children.Add(animationX[targetX, targetY]);
                            storyboard.Children.Add(animationY[targetX, targetY]);

                            completedLeft += 2;
                        }
                    }
                }
            }

            animType = AnimType.COMPLEX;

            storyboard.Begin();
        }

        public void startMoveTwoPiecesAnimation(int prevElementX, int prevElementY, IAnimationCallback callback)
        {
            selectionRect.Visibility = Visibility.Collapsed;
            animationCallbackObject = callback;
            completedLeft = 2;
            storyboard.Children.Clear();

            Canvas.SetZIndex(cell[prevElementX, prevElementY], 2);
            Canvas.SetZIndex(cell[selFromX, selFromY], 0);

            TimeSpan duration = TimeSpan.FromMilliseconds(ANIMATION_DURATION);

            animationX[prevElementX, prevElementY].From = ((TranslateTransform)cell[prevElementX, prevElementY].RenderTransform).X;
            animationX[prevElementX, prevElementY].To = (selFromX - prevElementX)  * cellWidth;
            animationX[prevElementX, prevElementY].By = animationX[prevElementX, prevElementY].From > animationX[prevElementX, prevElementY].To ? -1 : 1;
            animationX[prevElementX, prevElementY].Duration = duration;
            animationY[prevElementX, prevElementY].From = ((TranslateTransform)cell[prevElementX, prevElementY].RenderTransform).Y;
            animationY[prevElementX, prevElementY].To = (selFromY - prevElementY) * cellHeight;
            animationY[prevElementX, prevElementY].By = animationY[prevElementX, prevElementY].From > animationY[prevElementX, prevElementY].To ? -1 : 1;
            animationY[prevElementX, prevElementY].Duration = duration;

            animationX[selFromX, selFromY].From = ((TranslateTransform)cell[selFromX, selFromY].RenderTransform).X;
            animationX[selFromX, selFromY].To = (prevElementX - selFromX) * cellWidth;
            animationX[selFromX, selFromY].By = animationX[selFromX, selFromY].From > animationX[selFromX, selFromY].To ? -1 : 1;
            animationX[selFromX, selFromY].Duration = duration;
            animationY[selFromX, selFromY].From = ((TranslateTransform)cell[selFromX, selFromY].RenderTransform).Y;
            animationY[selFromX, selFromY].To = (prevElementY - selFromY) * cellHeight;
            animationY[selFromX, selFromY].By = animationY[selFromX, selFromY].From > animationY[selFromX, selFromY].To ? -1 : 1;
            animationY[selFromX, selFromY].Duration = duration;

            storyboard.Children.Add(animationX[selFromX, selFromY]);
            storyboard.Children.Add(animationY[selFromX, selFromY]);
            storyboard.Children.Add(animationX[prevElementX, prevElementY]);
            storyboard.Children.Add(animationY[prevElementX, prevElementY]);

            animType = AnimType.TWO_PIECES;

            storyboard.Begin();
        }

        public void onAnimationCompleted(object sender, EventArgs e)
        {
            if (--completedLeft == 0 && animationCallbackObject != null)
            {
                storyboard.Stop();
                if (animType == AnimType.COMPLEX)
                    animationCallbackObject.animationFinished(dx, dy);
                else
                    animationCallbackObject.animationFinishedSimple();

                animationCallbackObject = null;
            }
        }

        public void shadowViews()
        {
            for (int i = 0; i < elementsY; ++i)
                for (int j = 0; j < elementsX; ++j)
                    cell[j, i].Opacity = 0.09;
        }

        public void unshadowViews()
        {
            for (int i = 0; i < elementsY; ++i)
                for (int j = 0; j < elementsX; ++j)
                    cell[j, i].Opacity = 1.0;
        }
    }
}
