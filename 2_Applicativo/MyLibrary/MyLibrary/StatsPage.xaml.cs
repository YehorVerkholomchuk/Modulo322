using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using MyLibrary.Models;
using MyLibrary.Services;

namespace MyLibrary
{
    public partial class StatsPage : ContentPage
    {
        private readonly TxtStorageService _storageService;

        public StatsPage()
        {
            InitializeComponent();
            _storageService = new TxtStorageService();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadAndCalculateStatisticsAsync();
        }

        private async Task LoadAndCalculateStatisticsAsync()
        {
            try
            {
                // 1. Fetch raw line data from your .txt database
                List<MediaItem> allItems = await _storageService.LoadMediaItemsAsync();

                if (allItems == null || allItems.Count == 0)
                {
                    ResetUiMetrics();
                    return;
                }

                // 2. Compute aggregate metrics using LINQ queries
                int totalCount = allItems.Count;
                int bookCount = allItems.Count(i => i.Type.Equals("Book", StringComparison.OrdinalIgnoreCase));
                int movieCount = allItems.Count(i => i.Type.Equals("Movie", StringComparison.OrdinalIgnoreCase));

                double avgRating = allItems.Average(i => i.Rating);
                double completionRate = ((double)allItems.Count(i => i.IsCompleted) / totalCount) * 100;

                // 3. Extract and sort genre counts
                var genreSummary = allItems
                    .GroupBy(i => i.Genre)
                    .Select(g => new KeyValuePair<string, int>(string.IsNullOrWhiteSpace(g.Key) ? "General" : g.Key, g.Count()))
                    .OrderByDescending(kvp => kvp.Value)
                    .ToList();

                // 4. Extract top 5 items ordered by time tracked
                var topTimeInvestments = allItems
                    .OrderByDescending(i => i.TimeSpentMinutes)
                    .Take(5)
                    .ToList();

                // 5. Apply calculated values securely to UI text fields
                lblTotalBooks.Text = bookCount.ToString();
                lblTotalMovies.Text = movieCount.ToString();
                lblAvgRating.Text = avgRating.ToString();
                lblCompletionRate.Text = $"{completionRate:F0}%";

                GenreListView.ItemsSource = genreSummary;
                TopTimeCollectionView.ItemsSource = topTimeInvestments;
            }
            catch (Exception ex)
            {
                // Core exception containment blocks
                System.Diagnostics.Debug.WriteLine($"Statistics processing error: {ex.Message}");
                await DisplayAlert("Metrics Failure", "An error occurred while compiling analytical ledger rows.", "Dismiss");
            }
        }

        private void ResetUiMetrics()
        {
            lblTotalBooks.Text = "0";
            lblTotalMovies.Text = "0";
            lblAvgRating.Text = "0";
            lblCompletionRate.Text = "0%";
            GenreListView.ItemsSource = null;
            TopTimeCollectionView.ItemsSource = null;
        }

        private async void OnNavigateToDashboard(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MainPage());
        }

        private async void OnNavigateToAddMedia(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddMediaPage());
        }

        private async void OnNavigateToProfile(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ProfilePage());
        }

        private async void OnNavigateToSettings(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }

        private async void OnNavigateToAbout(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AboutPage());
        }
    }
}