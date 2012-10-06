using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Phone.Controls;
using System.Windows.Media.Imaging;
using System.Text;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace mPuzzle
{
    public partial class GamePortraitPage : PhoneApplicationPage, IPuzzleGamePage, IRestCallback
    {
        private GameController gameController;
        private GameViewController gameViewController;
        private string moves;
        private string time;
        private long itime;
        private int imoves;
        private Puzzle puzzle;
        private PuzzleAdapter puzzleAdapter;
        private Difficulty diff;
        private DispatcherTimer dtimer;
        private int uploadingDots;
        private bool nextPuzzleOpened;
        private bool uploaded;
        private NicknameDialog nicknameDialog;
        private RestService restService;
        private string username;
        private bool reload;

        // tombstome
        private int internetBestMoves;
        private long internetBestTime;
        private int internetPlace;
        private int prevMoves;
        private long prevTime;

        // assembled
        private bool assembled;

        public GamePortraitPage()
        {
            InitializeComponent();
            puzzleAdapter = (Application.Current as mPuzzle.App).puzzleAdapter;
            dtimer = new DispatcherTimer();
            dtimer.Tick += new EventHandler(uploadingOnTimer);
            dtimer.Interval = new TimeSpan(0, 0, 0, 0, 600);
            restService = new RestService();
            nicknameDialog = new NicknameDialog();
            nicknameDialog.username.KeyDown += new KeyEventHandler(username_KeyDown);
            nicknameDialog.buttonOk.Click += onNicknameOk;
            nicknameDialog.buttonCancel.Click += onNicknameCancel;
            nicknameDialog.Visibility = Visibility.Collapsed;
        }

        private void bitmap_Loaded(object sender, RoutedEventArgs e)
        {
            gameViewController.setBitmapHeight(((BitmapImage)sender).PixelHeight);
            gameViewController.setBitmapWidth(((BitmapImage)sender).PixelWidth);

            if (reload && !assembled)
            {
                gameController.startGame(true);
                buttonPause.IsChecked = true;
            }
            else
                gameController.startGame(false);
            //if (reload)
            //{
            //    if (!assembled)
            //    {
            //        gameController.togglePause();
            //        buttonPause.IsChecked = true;
            //    }
            //}
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            reload = State.ContainsKey("assembled");

            // reload puzzles and page state
            if (reload)
            {
                itime = (long)State["itime"];
                imoves = (int)State["imoves"];
                nextPuzzleOpened = (bool)State["nextPuzzleOpened"];
                time = Utils.formatTime(itime);
                moves = imoves.ToString();
                assembled = (bool)State["assembled"];
                if (assembled)
                {
                    prevTime = (long)State["prevTime"];
                    prevMoves = (int)State["prevMoves"];
                    uploaded = (bool)State["uploaded"];
                    if (uploaded)
                    {
                        internetBestMoves = (int)State["internetBestMoves"];
                        internetBestTime = (long)State["internetBestTime"];
                        internetPlace = (int)State["internetPlace"];
                    }
                }
            }

            puzzleAdapter = (Application.Current as mPuzzle.App).puzzleAdapter;

            puzzle = puzzleAdapter.puzzleDic[puzzleAdapter.selectedPuzzle];
            diff = puzzleAdapter.selectedDifficulty;

            gameViewController = new GameViewController(3 + (int)diff, 5 + (int)diff, gameCanvas, Dispatcher);
            gameController = new GameController(3 + (int)diff, 5 + (int)diff, puzzle, gameViewController, this);

            if (reload)
            {
                gameViewController.digout(this.State);
                gameController.digout(this.State);

                if (assembled)
                {
                    if (uploaded)
                    {
                        scorePanelInternet.Visibility = Visibility.Visible;
                        resPlace.Text = internetPlace.ToString();
                        res4.Text = " Time: " + Utils.formatTime(internetBestTime) + "; Moves: " + internetBestMoves;
                        scorePanelInternet.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        buttonUpload.Visibility = Visibility.Visible;
                    }

                    bool betterScore = (imoves < prevMoves) || (imoves == prevMoves && itime < prevTime) || (prevMoves == 0 && prevTime == 0);
                    if (betterScore)
                        beterScoreText.Text = " Better score achieved!";
                    else
                        beterScoreText.Text = " ";

                    res1.Text = " Time: " + time + "; Moves: " + moves;
                    res2.Text = " Time: " + Utils.formatTime(prevTime) + "; Moves: " + prevMoves;

                    scorePanel.Visibility = Visibility.Visible;

                    if (nextPuzzleOpened)
                        buttonNextPuzzle.Visibility = Visibility.Visible;

                    buttonPause.Visibility = Visibility.Collapsed;
                }

                movesText.Text = moves;
                timeText.Text = time;
            }

            Uri bitmapUri = new Uri(puzzle.puzzleImage, UriKind.Relative);
            BitmapImage bitmap = new BitmapImage(bitmapUri);
            bitmap.ImageOpened += new EventHandler<RoutedEventArgs>(bitmap_Loaded);
            gameViewController.setBitmap(bitmap);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (State.ContainsKey("itime"))
                State.Remove("itime");
            State["itime"] = itime;
            if (State.ContainsKey("imoves"))
                State.Remove("imoves");
            State["imoves"] = imoves;
            if (State.ContainsKey("nextPuzzleOpened"))
                State.Remove("nextPuzzleOpened");
            State["nextPuzzleOpened"] = nextPuzzleOpened;
            if (State.ContainsKey("assembled"))
                State.Remove("assembled");
            State["assembled"] = assembled;
            if (assembled)
            {
                if (State.ContainsKey("prevMoves"))
                    State.Remove("prevMoves");
                State["prevMoves"] = prevMoves;
                if (State.ContainsKey("prevTime"))
                    State.Remove("prevTime");
                State["prevTime"] = prevTime;
                if (State.ContainsKey("uploaded"))
                    State.Remove("uploaded");
                State["uploaded"] = uploaded;
                if (uploaded)
                {
                    if (State.ContainsKey("internetBestMoves"))
                        State.Remove("internetBestMoves");
                    State["internetBestMoves"] = internetBestMoves;
                    if (State.ContainsKey("internetBestTime"))
                        State.Remove("internetBestTime");
                    State["internetBestTime"] = internetBestTime;
                    if (State.ContainsKey("internetPlace"))
                        State.Remove("internetPlace");
                    State["internetPlace"] = internetPlace;
                }
            }

            gameController.tombstone(State);
            gameViewController.tombstone(State);
        }

        private void gameCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!assembled)
                gameController.mouseDown((int)e.GetPosition((UIElement)sender).X, (int)e.GetPosition((UIElement)sender).Y);
            else
            {
                if (scorePanel.Visibility == Visibility.Collapsed)
                {
                    scorePanel.Visibility = Visibility.Visible;
                    if (!uploaded)
                        buttonUpload.Visibility = Visibility.Visible;
                    else
                        scorePanelInternet.Visibility = Visibility.Visible;
                    if (nextPuzzleOpened)
                        buttonNextPuzzle.Visibility = Visibility.Visible;
                }
                else
                {
                    scorePanel.Visibility = Visibility.Collapsed;
                    if (uploaded)
                        scorePanelInternet.Visibility = Visibility.Collapsed;
                    else
                        buttonUpload.Visibility = Visibility.Collapsed;
                    buttonNextPuzzle.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void gameCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!assembled)
                gameController.mouseUp((int)e.GetPosition((UIElement)sender).X, (int)e.GetPosition((UIElement)sender).Y);
        }

        private void gameCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!assembled)
                gameController.mouseMove((int)e.GetPosition((UIElement)sender).X, (int)e.GetPosition((UIElement)sender).Y);
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            if (!assembled)
                gameController.togglePause();
        }

        public void updateTime(long mseconds)
        {
            itime = mseconds;
            time = Utils.formatTime(mseconds);
            Dispatcher.BeginInvoke(_updateTime);
        }

        public void _updateTime()
        {
            timeText.Text = time.ToString();
        }

        public void updateMoves(int moves)
        {
            imoves = moves;
            this.moves = moves.ToString();
            Dispatcher.BeginInvoke(_updateMoves);
        }

        private void _updateMoves()
        {
            movesText.Text = moves;
        }

        public void puzzleAssembled()
        {
            assembled = true;
            switch (diff)
            { 
                case Difficulty.EASY:
                    prevTime = puzzle.timeeasy;
                    prevMoves = puzzle.moveseasy;
                    break;
                case Difficulty.MEDIUM:
                    prevTime = puzzle.timemedium;
                    prevMoves = puzzle.movesmedium;
                    break;
                case Difficulty.HARD:
                    prevTime = puzzle.timehard;
                    prevMoves = puzzle.moveshard;
                    break;
                case Difficulty.EXPERT:
                    prevTime = puzzle.timeexpert;
                    prevMoves = puzzle.movesexpert;
                    break;
            }

            bool betterScore = (imoves < prevMoves) || (imoves == prevMoves && itime < prevTime) || (prevMoves == 0 && prevTime == 0);
            if (betterScore)
            {
                beterScoreText.Text = " Better score achieved!";
            }
            else
            {
                beterScoreText.Text = " ";
            }

            res1.Text = " Time: " + time + "; Moves: " + moves;
            res2.Text = " Time: " + Utils.formatTime(prevTime) + "; Moves: " + prevMoves;

            if (betterScore)
            {
                switch (diff)
                {
                    case Difficulty.EASY:
                        puzzle.timeeasy = itime;
                        puzzle.moveseasy = imoves;
                        break;
                    case Difficulty.MEDIUM:
                        puzzle.timemedium = itime;
                        puzzle.movesmedium = imoves;
                        break;
                    case Difficulty.HARD:
                        puzzle.timehard = itime;
                        puzzle.moveshard = imoves;
                        break;
                    case Difficulty.EXPERT:
                        puzzle.timeexpert = itime;
                        puzzle.movesexpert = imoves;
                        break;
                }

                puzzleAdapter.saveScores();
            }

            nextPuzzleOpened = (puzzleAdapter.selectedPuzzle == puzzleAdapter.puzzle[puzzleAdapter.openedPuzzles - 1].id) && puzzleAdapter.openNewPuzzle();
            if (nextPuzzleOpened)
            {
                puzzleAdapter.saveSettings();
                nextPuzzleOpened = true;
                buttonNextPuzzle.Visibility = Visibility.Visible;
            }
            scorePanel.Visibility = Visibility.Visible;
            buttonUpload.Visibility = Visibility.Visible;
            buttonPause.Visibility = Visibility.Collapsed;

            uploaded = false;
        }

        private void buttonUpload_Click(object sender, RoutedEventArgs e)
        {
            if (puzzleAdapter.userid <= 0)
            {
                nicknameDialog.hideStatus();
                gameCanvas.Children.Add(nicknameDialog);
                Canvas.SetLeft(nicknameDialog, (GameParams.screenWidthPortrait - GameParams.nicknameDialogWidth) / 2);
                Canvas.SetTop(nicknameDialog, (GameParams.screenHeightPortrait - GameParams.nicknameDialogHeight) / 2);
                Canvas.SetZIndex(nicknameDialog, 3);
                scorePanel.Visibility = Visibility.Collapsed;
                buttonUpload.Visibility = Visibility.Collapsed;
                buttonNextPuzzle.Visibility = Visibility.Collapsed;

                gameCanvas.MouseLeftButtonDown -= gameCanvas_MouseLeftButtonDown;
                nicknameDialog.Visibility = Visibility.Visible;
            }
            else
            {
                uploadingDots = 0;
                buttonUpload.Content = "Please wait.";
                dtimer.Start();
                buttonUpload.Click -= buttonUpload_Click;

                restService.sync(itime, imoves, diff, puzzleAdapter.userid, puzzle.id, this);
            }
        }

        private void uploadingOnTimer(object o, EventArgs sender)
        {
            uploadingDots = (uploadingDots + 1) % 3;
            buttonUpload.Content = "Please wait.";
            for (int i = 0; i < uploadingDots; ++i)
                buttonUpload.Content += ".";
        }

        private void buttonNextPuzzle_Click(object sender, RoutedEventArgs e)
        {
            puzzleAdapter.selectedPuzzle = puzzleAdapter.puzzle[puzzleAdapter.openedPuzzles - 1].id;
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

        private void onNicknameOk(object sender, RoutedEventArgs e)
        {
            username = nicknameDialog.username.Text;
            if (username.Length > 0 && puzzleAdapter.validateUsername(username))
            {
                nicknameDialog.showChecking();
                nicknameDialog.lockForCheck();
                // async. answer in usernameExistCallback
                restService.createUser(username, this);
            }
            else
            {
                nicknameDialog.showUsernameNotValid();
            }
        }

        void username_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                onNicknameOk(null, null);
            }
        }

        private void onNicknameCancel(object sender, RoutedEventArgs e)
        {
            restService.cancelRequest();
            nicknameDialog.unlock();
            nicknameDialog.Visibility = Visibility.Collapsed;
            scorePanel.Visibility = Visibility.Visible;
            buttonUpload.Visibility = Visibility.Visible;
            if (nextPuzzleOpened)
                buttonNextPuzzle.Visibility = Visibility.Visible;
            gameCanvas.Children.Remove(nicknameDialog);

            gameCanvas.MouseLeftButtonDown += gameCanvas_MouseLeftButtonDown;
        }

        private delegate void createUserCallbackUI(bool requestOk, int userid);

        private void _createUserCallback(bool requestOk, int userid)
        {
            nicknameDialog.unlock();

            if (!requestOk)
            {
                nicknameDialog.showConnectionProblem();
                return;
            }

            if (userid > 0)
            {
                puzzleAdapter.username = username;
                puzzleAdapter.userid = userid;
                puzzleAdapter.saveSettings();
                nicknameDialog.Visibility = Visibility.Collapsed;
                gameCanvas.Children.Remove(nicknameDialog);
                scorePanel.Visibility = Visibility.Visible;
                buttonUpload.Visibility = Visibility.Visible;
                if (nextPuzzleOpened)
                    buttonNextPuzzle.Visibility = Visibility.Visible;
                gameCanvas.MouseLeftButtonDown += gameCanvas_MouseLeftButtonDown;
                buttonUpload_Click(null, null);
            }
            else
            {
                nicknameDialog.showUsernameExist();
            }
        }

        public void createUserCallback(bool requestOk, int userid)
        {
            Dispatcher.BeginInvoke(new createUserCallbackUI(_createUserCallback), requestOk, userid);
        }

        private delegate void syncOneCallbackUI(bool requestOk, SyncOneResponse res);

        private void _syncOneCallbackUI(bool requestOk, SyncOneResponse res)
        {
            dtimer.Stop();

            if (!requestOk)
            {
                buttonUpload.Content = "Check global place";
                buttonUpload.Click += buttonUpload_Click;
                Canvas.SetZIndex(syncError, 3);
                syncError.Visibility = Visibility.Visible;
                syncErrorAnimation.RepeatBehavior = new RepeatBehavior(5);
                syncErrorAnimation.Begin();
                return;
            }
            
            uploaded = true;
            buttonUpload.Visibility = Visibility.Collapsed;
            resPlace.Text = res.place.ToString();
            res4.Text = " Time: " + Utils.formatTime(res.besttime) + "; Moves: " + res.bestmoves;
            scorePanelInternet.Visibility = Visibility.Visible;

            // save best results in case if tombstoming needed
            internetBestMoves = res.bestmoves;
            internetBestTime = res.besttime;
            internetPlace = res.place;

            switch (diff)
            {
                case Difficulty.EASY:
                    puzzle.timeeasyb = res.besttime;
                    puzzle.moveseasyb = res.bestmoves;
                    if (res.place < puzzle.easyp)
                        puzzle.easyp = res.place;
                    puzzle.userlisteasy = res.userlist;
                    break;
                case Difficulty.MEDIUM:
                    puzzle.timemediumb = res.besttime;
                    puzzle.movesmediumb = res.bestmoves;
                    if (res.place < puzzle.mediump)
                        puzzle.mediump = res.place;
                    puzzle.userlistmedium = res.userlist;
                    break;
                case Difficulty.HARD:
                    puzzle.timehardb = res.besttime;
                    puzzle.moveshardb = res.bestmoves;
                    if (res.place < puzzle.hardp)
                        puzzle.hardp = res.place;
                    puzzle.userlisthard = res.userlist;
                    break;
                case Difficulty.EXPERT:
                    puzzle.timeexpertb = res.besttime;
                    puzzle.movesexpertb = res.bestmoves;
                    if (res.place < puzzle.expertp)
                        puzzle.expertp = res.place;
                    puzzle.userlistexpert = res.userlist;
                    break;
            }

            puzzleAdapter.saveScores();
        }

        public void syncOneCallback(bool requestOk, SyncOneResponse result)
        {
            Dispatcher.BeginInvoke(new syncOneCallbackUI(_syncOneCallbackUI), requestOk, result);
        }

        public void syncAllCallback(bool requestOk, SyncAllResponse result)
        {
            //Dispatcher.BeginInvoke(new syncOneCallbackUI(_syncOneCallbackUI), requestOk, result);
        }

        public void changeUsernameCallback(bool requestOk, int result)
        {
        }

        private void syncErrorAnimation_Completed(object sender, EventArgs e)
        {
            syncError.Visibility = Visibility.Collapsed;
        }
    }
}