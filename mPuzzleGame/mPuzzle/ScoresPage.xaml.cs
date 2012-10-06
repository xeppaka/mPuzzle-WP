using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Threading;

namespace mPuzzle
{
    public partial class ScoresPage : PhoneApplicationPage, IRestCallback
    {
        private PuzzleAdapter puzzleAdapter;
        private RestService restService;
        private string username;

        public ScoresPage()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(ScoresPage_Loaded);

            puzzleAdapter = (Application.Current as mPuzzle.App).puzzleAdapter;
            restService = new RestService();
        }

        private void ScoresPage_Loaded(object sender, RoutedEventArgs e)
        {
            List<PuzzleViewInfo> puzzles = new List<PuzzleViewInfo>();
            PuzzleViewInfo pv;
            for (int i = 0; i < puzzleAdapter.puzzle.Length; ++i)
            {
                pv = new PuzzleViewInfo(puzzleAdapter.puzzle[i]);
                puzzles.Add(pv);
            }
            puzzlesListBox.ItemsSource = puzzles;
            nicknameDialog.username.KeyDown += new KeyEventHandler(username_KeyDown);
            nicknameDialog.buttonOk.Click += onNicknameOk;
            nicknameDialog.buttonCancel.Click += onNicknameCancel;
            syncErrorDialog.buttonOk.Click += onSyncErrorOk;
        }

        private void puzzlesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PuzzleViewInfo pv = (PuzzleViewInfo)puzzlesListBox.SelectedItem;
            if (pv != null && pv.opened)
            {
                string uriStr = "/ScoreUserListPage.xaml";
                if (puzzleAdapter != null)
                    puzzleAdapter.selectedPuzzle = pv.id;
                NavigationService.Navigate(new Uri(uriStr, UriKind.RelativeOrAbsolute));
            }
        }

        private void puzzlesListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            int c = puzzlesListBox.Items.Count;
            for (int i = 0; i < c; ++i)
            {
                ListBoxItem lbi = (ListBoxItem)puzzlesListBox.ItemContainerGenerator.ContainerFromIndex(i);
                if (lbi != null)
                    VisualStateManager.GoToState(lbi, "Normal", false);
            }
        }

        private void buttonSyncAll_Click(object sender, RoutedEventArgs e)
        {
            buttonSyncAll.Click -= buttonSyncAll_Click;
            if (puzzleAdapter.userid <= 0)
            {
                nicknameDialog.hideStatus();
                Canvas.SetZIndex(nicknameDialog, 3);
                nicknameDialog.Visibility = Visibility.Visible;
                puzzlesListBox.IsEnabled = false;
            }
            else
            {
                puzzlesListBox.IsEnabled = false;
                // async result in syncAllCallback
                restService.sync(puzzleAdapter.puzzle, puzzleAdapter.userid, this);
                buttonSyncAll.Style = (Style)Resources["ScoresSyncButtonStyleWait"];
            }
        }

        private delegate void syncAllCallbackUI(bool requestOk, SyncAllResponse res);

        private void _syncAllCallbackUI(bool requestOk, SyncAllResponse res)
        {
            puzzlesListBox.IsEnabled = true;
            buttonSyncAll.Click += buttonSyncAll_Click;
            buttonSyncAll.Style = (Style)Resources["ScoresSyncButtonStyle"];

            if (!requestOk)
            {
                Canvas.SetZIndex(syncErrorDialog, 3);
                syncErrorDialog.Visibility = Visibility.Visible;
                return;
            }

            int count = res.bestResults.Length;
            for (int i = 0; i < count; ++i)
            {
                PuzzleViewInfo curPVI = (PuzzleViewInfo)puzzlesListBox.Items[i];
                Puzzle curPuzzle = puzzleAdapter.puzzleDic[res.bestResults[i].puzzleid];
                curPuzzle.easyp = res.bestResults[i].placeeasy;
                curPuzzle.mediump = res.bestResults[i].placemedium;
                curPuzzle.hardp = res.bestResults[i].placehard;
                curPuzzle.expertp = res.bestResults[i].placeexpert;
                curPuzzle.moveseasyb = res.bestResults[i].moveseasy;
                curPuzzle.movesmediumb = res.bestResults[i].movesmedium;
                curPuzzle.moveshardb = res.bestResults[i].moveshard;
                curPuzzle.movesexpertb = res.bestResults[i].movesexpert;
                curPuzzle.timeeasyb = res.bestResults[i].timeeasy;
                curPuzzle.timemediumb = res.bestResults[i].timemedium;
                curPuzzle.timehardb = res.bestResults[i].timehard;
                curPuzzle.timeexpertb = res.bestResults[i].timeexpert;
                curPuzzle.userlisteasy = res.bestResults[i].userlisteasy;
                curPuzzle.userlistmedium = res.bestResults[i].userlistmedium;
                curPuzzle.userlisthard = res.bestResults[i].userlisthard;
                curPuzzle.userlistexpert = res.bestResults[i].userlistexpert;

                curPVI.update(curPuzzle);
            }
            puzzlesListBox.Visibility = Visibility.Visible;
            puzzleAdapter.saveScores();
        }

        public void syncAllCallback(bool requestOk, SyncAllResponse result)
        {
            Dispatcher.BeginInvoke(new syncAllCallbackUI(_syncAllCallbackUI), requestOk, result);
        }

        public void syncOneCallback(bool requestOk, SyncOneResponse result)
        {
            return;
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
                buttonSyncAll_Click(null, null);
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
            buttonSyncAll.Click += buttonSyncAll_Click;
            puzzlesListBox.IsEnabled = true;
        }

        private void onSyncErrorOk(object sender, RoutedEventArgs e)
        {
            syncErrorDialog.Visibility = Visibility.Collapsed;
            buttonSyncAll.Click += buttonSyncAll_Click;
            puzzlesListBox.IsEnabled = true;
        }

        public void changeUsernameCallback(bool requestOk, int result)
        {
        }
    }
}