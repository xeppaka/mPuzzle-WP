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
using System.Windows.Media.Imaging;
using Microsoft.Phone.Shell;

namespace mPuzzle
{
    public partial class ChooseDifficultyPage : PhoneApplicationPage
    {
        PuzzleAdapter puzzleAdapter;
        Puzzle curPuzzle;
        int puzzleId;

        public ChooseDifficultyPage()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(ChooseDifficultyPage_Loaded);
        }

        void ChooseDifficultyPage_Loaded(object sender, RoutedEventArgs e)
        {
            PuzzleViewInfo pv = new PuzzleViewInfo(curPuzzle);
            yourResults.resEasyMoves.Text = pv.moveseasy.ToString();
            yourResults.resMediumMoves.Text = pv.movesmedium.ToString();
            yourResults.resHardMoves.Text = pv.moveshard.ToString();
            yourResults.resExpertMoves.Text = pv.movesexpert.ToString();
            yourResults.resEasyTime.Text = pv.timeeasy;
            yourResults.resMediumTime.Text = pv.timemedium;
            yourResults.resHardTime.Text = pv.timehard;
            yourResults.resExpertTime.Text = pv.timeexpert;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            puzzleAdapter = (Application.Current as mPuzzle.App).puzzleAdapter;
            puzzleId = puzzleAdapter.selectedPuzzle;
            curPuzzle = puzzleAdapter.puzzleDic[puzzleId];
            ImageBrush ib = new ImageBrush();
            Uri imageUri = new Uri(curPuzzle.puzzleThumbnail, UriKind.Relative);
            puzzleThumbnail.Source = new BitmapImage(imageUri);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            puzzleAdapter.selectedDifficulty = Difficulty.EASY;

            if (curPuzzle.orientation == mPuzzle.Orientation.PORTRAIT)
            {
                string uriStr = "/GamePortraitPage.xaml";
                NavigationService.Navigate(new Uri(uriStr, UriKind.RelativeOrAbsolute));
            }
            else
            {
                string uriStr = "/GameLandscapePage.xaml";
                NavigationService.Navigate(new Uri(uriStr, UriKind.RelativeOrAbsolute));
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            puzzleAdapter.selectedDifficulty = Difficulty.MEDIUM;

            if (curPuzzle.orientation == mPuzzle.Orientation.PORTRAIT)
            {
                string uriStr = "/GamePortraitPage.xaml";
                NavigationService.Navigate(new Uri(uriStr, UriKind.RelativeOrAbsolute));
            }
            else
            {
                string uriStr = "/GameLandscapePage.xaml";
                NavigationService.Navigate(new Uri(uriStr, UriKind.RelativeOrAbsolute));
            }
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            puzzleAdapter.selectedDifficulty = Difficulty.HARD;

            if (curPuzzle.orientation == mPuzzle.Orientation.PORTRAIT)
            {
                string uriStr = "/GamePortraitPage.xaml";
                NavigationService.Navigate(new Uri(uriStr, UriKind.RelativeOrAbsolute));
            }
            else
            {
                string uriStr = "/GameLandscapePage.xaml";
                NavigationService.Navigate(new Uri(uriStr, UriKind.RelativeOrAbsolute));
            }
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            puzzleAdapter.selectedDifficulty = Difficulty.EXPERT;

            if (curPuzzle.orientation == mPuzzle.Orientation.PORTRAIT)
            {
                string uriStr = "/GamePortraitPage.xaml";
                NavigationService.Navigate(new Uri(uriStr, UriKind.RelativeOrAbsolute));
            }
            else
            {
                string uriStr = "/GameLandscapePage.xaml";
                NavigationService.Navigate(new Uri(uriStr, UriKind.RelativeOrAbsolute));
            }
        }
    }
}