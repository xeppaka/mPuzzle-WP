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

namespace mPuzzle
{
    public partial class NicknameDialog : UserControl
    {
        private static readonly string USERNAME_INVALID = "Invalid username";
        private static readonly string USERNAME_EXIST = "Such username already exist";
        private static readonly string CHECK_AVAILABILITY = "Checking username availability";
        private static readonly string CONNECTION_PROBLEM = "Internet connection problem";

        private Brush red = new SolidColorBrush(Colors.Red);
        private Brush white = new SolidColorBrush(Colors.White);

        public NicknameDialog()
        {
            InitializeComponent();
        }

        public void hideStatus()
        {
            statusText.Visibility = Visibility.Collapsed;
        }

        public void showUsernameNotValid()
        {
            statusText.Text = USERNAME_INVALID;
            statusText.Foreground = red;
            statusText.Visibility = Visibility.Visible;
            statusTextAnimation.RepeatBehavior = new RepeatBehavior(2);
            statusTextAnimation.Stop();
            statusTextAnimation.Begin();
        }

        public void showChecking()
        {
            statusText.Text = CHECK_AVAILABILITY;
            statusText.Foreground = white;
            statusText.Visibility = Visibility.Visible;
            statusTextAnimation.RepeatBehavior = new RepeatBehavior(100000000);
            statusTextAnimation.Stop();
            statusTextAnimation.Begin();
        }

        public void showConnectionProblem()
        {
            statusText.Text = CONNECTION_PROBLEM;
            statusText.Foreground = red;
            statusText.Visibility = Visibility.Visible;
            statusTextAnimation.RepeatBehavior = new RepeatBehavior(3);
            statusTextAnimation.Stop();
            statusTextAnimation.Begin();
        }

        public void showUsernameExist()
        {
            statusText.Text = USERNAME_EXIST;
            statusText.Foreground = red;
            statusText.Visibility = Visibility.Visible;
            statusTextAnimation.RepeatBehavior = new RepeatBehavior(3);
            statusTextAnimation.Stop();
            statusTextAnimation.Begin();
        }

        public void lockForCheck()
        {
            buttonOk.IsEnabled = false;
            username.IsEnabled = false;
        }

        public void unlock()
        {
            buttonOk.IsEnabled = true;
            username.IsEnabled = true;
        }
    }
}
