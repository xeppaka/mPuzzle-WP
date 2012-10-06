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

namespace mPuzzle
{
    public partial class SettingsPage : PhoneApplicationPage, IRestCallback
    {
        private PuzzleAdapter puzzleAdapter;
        private RestService restService;
        private string uname;

        public SettingsPage()
        {
            InitializeComponent();
            restService = new RestService();
            puzzleAdapter = (Application.Current as mPuzzle.App).puzzleAdapter;
            Loaded += new RoutedEventHandler(SettingsPage_Loaded);
            nicknameDialog.username.KeyDown += new KeyEventHandler(username_KeyDown);
            nicknameDialog.buttonOk.Click += onNicknameOk;
            nicknameDialog.buttonCancel.Click += onNicknameCancel;
        }

        void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (puzzleAdapter.username != null && puzzleAdapter.username.Length > 0)
                username.Text = puzzleAdapter.username;
            else
                username.Text = " ";
        }

        private void buttonChangeUsername_Click(object sender, RoutedEventArgs e)
        {
            nicknameDialog.hideStatus();
            if (puzzleAdapter.userid > 0)
                nicknameDialog.username.Text = puzzleAdapter.username;
            else
                nicknameDialog.username.Text = "";
            nicknameDialog.Visibility = Visibility.Visible;
            buttonChangeUsername.Click -= buttonChangeUsername_Click;
        }

        private delegate void changeUserCallbackUI(bool requestOk, int result);

        public void _changeUsernameCallback(bool requestOk, int result)
        {
            if (requestOk)
            {
                if (result != 0)
                {
                    nicknameDialog.showUsernameExist();
                }
                else
                {
                    puzzleAdapter.username = uname;
                    puzzleAdapter.saveSettings();
                    username.Text = puzzleAdapter.username;
                    nicknameDialog.Visibility = Visibility.Collapsed;
                    buttonChangeUsername.Click += buttonChangeUsername_Click;
                }
            }
            else
            {
                nicknameDialog.showConnectionProblem();
            }
            nicknameDialog.unlock();
        }

        public void changeUsernameCallback(bool requestOk, int result)
        {
            Dispatcher.BeginInvoke(new createUserCallbackUI(_changeUsernameCallback), requestOk, result);
        }

        private delegate void createUserCallbackUI(bool requestOk, int userid);

        public void _createUserCallback(bool requestOk, int userid)
        {
            if (requestOk)
            {
                if (userid > 0)
                {
                    puzzleAdapter.username = uname;
                    puzzleAdapter.userid = userid;
                    puzzleAdapter.saveSettings();
                    username.Text = puzzleAdapter.username;
                    nicknameDialog.Visibility = Visibility.Collapsed;
                    buttonChangeUsername.Click += buttonChangeUsername_Click;
                }
                else
                {
                    nicknameDialog.showUsernameExist();
                }
            }
            else
            {
                nicknameDialog.showConnectionProblem();
            }
            nicknameDialog.unlock();
        }

        public void createUserCallback(bool requestOk, int userid)
        {
            Dispatcher.BeginInvoke(new createUserCallbackUI(_createUserCallback), requestOk, userid);
        }

        public void syncOneCallback(bool requestOk, SyncOneResponse results)
        {
        }

        public void syncAllCallback(bool requestOk, SyncAllResponse results)
        {
        }

        private void onNicknameOk(object sender, RoutedEventArgs e)
        {
            uname = nicknameDialog.username.Text;
            if (uname.Length > 0 && puzzleAdapter.validateUsername(uname))
            {
                nicknameDialog.showChecking();
                nicknameDialog.lockForCheck();
                if (puzzleAdapter.userid <= 0)
                    // async. answer in createUserCallback
                    restService.createUser(uname, this);
                else
                    // async. answer in changeUsernameCallback
                    restService.changeUsername(puzzleAdapter.userid, puzzleAdapter.username, uname, this);
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
            buttonChangeUsername.Click += buttonChangeUsername_Click;
        }
    }
}