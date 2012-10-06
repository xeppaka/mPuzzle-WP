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
    public partial class ScoreUserListPage : PhoneApplicationPage
    {
        private PuzzleAdapter puzzleAdapter;

        public ScoreUserListPage()
        {
            InitializeComponent();
            puzzleAdapter = (Application.Current as mPuzzle.App).puzzleAdapter;
            Loaded += new RoutedEventHandler(ScoreUserListPage_Loaded);
        }

        void ScoreUserListPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (puzzleAdapter.puzzleDic[puzzleAdapter.selectedPuzzle].userlisteasy != null)
                puzzlesListBox.ItemsSource = puzzleAdapter.puzzleDic[puzzleAdapter.selectedPuzzle].userlisteasy;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (puzzleAdapter.puzzleDic[puzzleAdapter.selectedPuzzle].userlisteasy != null)
                puzzlesListBox.ItemsSource = puzzleAdapter.puzzleDic[puzzleAdapter.selectedPuzzle].userlisteasy;
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if (puzzleAdapter.puzzleDic[puzzleAdapter.selectedPuzzle].userlistmedium != null)
                puzzlesListBox.ItemsSource = puzzleAdapter.puzzleDic[puzzleAdapter.selectedPuzzle].userlistmedium;
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            if (puzzleAdapter.puzzleDic[puzzleAdapter.selectedPuzzle].userlisthard != null)
                puzzlesListBox.ItemsSource = puzzleAdapter.puzzleDic[puzzleAdapter.selectedPuzzle].userlisthard;
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            if (puzzleAdapter.puzzleDic[puzzleAdapter.selectedPuzzle].userlistexpert != null)
                puzzlesListBox.ItemsSource = puzzleAdapter.puzzleDic[puzzleAdapter.selectedPuzzle].userlistexpert;
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
    }
}