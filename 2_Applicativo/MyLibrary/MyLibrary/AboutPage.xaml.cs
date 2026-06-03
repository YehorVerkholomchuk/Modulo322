using System;
using Microsoft.Maui.Controls;

namespace MyLibrary
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        private async void OnNavigateToDashboard(object sender, EventArgs e) =>
            await Navigation.PushAsync(new MainPage());

        private async void OnNavigateToAddMedia(object sender, EventArgs e) =>
            await Navigation.PushAsync(new AddMediaPage());

        private async void OnNavigateToStats(object sender, EventArgs e) =>
            await Navigation.PushAsync(new StatsPage());

        private async void OnNavigateToProfile(object sender, EventArgs e) =>
            await Navigation.PushAsync(new ProfilePage());

        private async void OnNavigateToSettings(object sender, EventArgs e) =>
            await Navigation.PushAsync(new SettingsPage());
    }
}
