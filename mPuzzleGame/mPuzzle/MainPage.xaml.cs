using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using System.Windows.Media.Imaging;
using System.IO;

namespace mPuzzle
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private void button_play_click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ChoosePuzzlePage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void button_scores_click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ScoresPage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void button_help_click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/HelpPage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void button_settings_click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.RelativeOrAbsolute));
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }
    }
}