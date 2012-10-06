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
using System.Collections.ObjectModel;

namespace mPuzzle
{
    public partial class ChoosePuzzle : PhoneApplicationPage
    {
        private PuzzleAdapter puzzleAdapter;

        public ChoosePuzzle()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(ChoosePuzzle_Loaded);
        }

        void ChoosePuzzle_Loaded(object sender, RoutedEventArgs e)
        {
            List<PuzzleViewInfo> puzzles = new List<PuzzleViewInfo>();
            PuzzleViewInfo pv;
            for (int i = 0; i < puzzleAdapter.puzzle.Length; ++i)
            {
                pv = new PuzzleViewInfo(puzzleAdapter.puzzle[i]);
                puzzles.Add(pv);
            }
            puzzlesListBox.ItemsSource = puzzles;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            puzzleAdapter = (Application.Current as mPuzzle.App).puzzleAdapter;
        }

        //private void loaded()

        private void puzzlesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PuzzleViewInfo pv = (PuzzleViewInfo)puzzlesListBox.SelectedItem;
            if (pv != null && pv.opened)
            {
                string uriStr = "/ChooseDifficultyPage.xaml";
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

    }
}