using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using MyLibrary.Models;
using MyLibrary.Services;

namespace MyLibrary
{
    public partial class MainPage : ContentPage
    {
        private readonly TxtStorageService _storageService;
        private List<MediaItem> _allItemsList;
        public ObservableCollection<MediaItem> DisplayedItems { get; set; }

        public MainPage()
        {
            InitializeComponent();
            _storageService = new TxtStorageService();
            DisplayedItems = new ObservableCollection<MediaItem>();
            MediaCollectionView.ItemsSource = DisplayedItems;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await RefreshDataDataGridAsync();
        }

        private async Task RefreshDataDataGridAsync()
        {
            try
            {
                _allItemsList = await _storageService.LoadMediaItemsAsync();
                FilterAndRenderGrid(MediaSearchBar.Text);
            }
            catch (Exception ex)
            {
                await DisplayAlert("System Error", $"Failed to update interface feeds: {ex.Message}", "OK");
            }
        }

        private void FilterAndRenderGrid(string query)
        {
            if (_allItemsList == null) return;

            var filtered = string.IsNullOrWhiteSpace(query)
                ? _allItemsList
                : _allItemsList.Where(i => i.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                           i.Genre.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                DisplayedItems.Clear();
                foreach (var item in filtered)
                {
                    DisplayedItems.Add(item);
                }

                lblTotalCount.Text = _allItemsList.Count.ToString();
                lblCompletedCount.Text = _allItemsList.Count(i => i.IsCompleted).ToString();
                double hoursTotal = _allItemsList.Sum(i => i.TimeSpentMinutes) / 60.0;
                lblTotalHours.Text = $"{hoursTotal:F1} hrs";
            });
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            FilterAndRenderGrid(e.NewTextValue);
        }

        private async void OnMediaSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is MediaItem selectedItem)
            {
                MediaCollectionView.SelectedItem = null;

                await DisplayAlert(selectedItem.Title, $"Review Notes:\n{selectedItem.Review}", "Dismiss");
            }
        }

        private async void OnNavigateToAddMedia(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddMediaPage());
        }

        private async void OnNavigateToStats(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new StatsPage());
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