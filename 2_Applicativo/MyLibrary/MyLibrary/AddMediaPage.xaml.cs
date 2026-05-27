using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using MyLibrary.Models;
using MyLibrary.Services;

namespace MyLibrary
{
    public partial class AddMediaPage : ContentPage
    {
        private readonly TxtStorageService _storageService;

        public AddMediaPage()
        {
            InitializeComponent();
            _storageService = new TxtStorageService();

            PickerType.SelectedIndex = 0;
            PickerRating.SelectedIndex = 4;
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                // 1. Validate mandatory fields
                if (string.IsNullOrWhiteSpace(EntryTitle.Text))
                {
                    await DisplayAlert("Validation Error", "The Title field cannot be empty.", "OK");
                    return;
                }

                // 2. Parse numeric inputs safely
                double timeSpent = 0;
                if (!string.IsNullOrWhiteSpace(EntryTime.Text))
                {
                    if (!double.TryParse(EntryTime.Text, out timeSpent))
                    {
                        throw new FormatException("Time Spent must be a valid number.");
                    }
                }

                // 3. Construct the MediaItem (Leveraging the encapsulation built into the model)
                var newItem = new MediaItem
                {
                    Title = EntryTitle.Text.Trim(),
                    Type = PickerType.SelectedItem?.ToString() ?? "Book",
                    Genre = string.IsNullOrWhiteSpace(EntryGenre.Text) ? "General" : EntryGenre.Text.Trim(),
                    Rating = (int)(PickerRating.SelectedItem ?? 10),
                    TimeSpentMinutes = timeSpent,
                    Review = EditorReview.Text?.Trim(),
                    IsCompleted = CheckCompleted.IsChecked,
                    IsFavorite = CheckFavorite.IsChecked,
                    DateAdded = DateTime.Now
                };

                // 4. Save via JSON Service
                List<MediaItem> existingItems = await _storageService.LoadMediaItemsAsync();
                existingItems.Add(newItem);
                await _storageService.SaveMediaItemsAsync(existingItems);

                // 5. Success Feedback and Navigation
                await DisplayAlert("Success", $"{newItem.Title} has been added to your library.", "OK");
                await Navigation.PopToRootAsync(); // Navigate back to the previous page
            }
            catch (FormatException ex)
            {
                // Specifically catches the numeric parsing failure
                await DisplayAlert("Input Error", ex.Message, "OK");
            }
            catch (Exception ex)
            {
                // Catch-all for any other unexpected errors (file access, memory, etc.)
                await DisplayAlert("System Error", $"Could not save the item: {ex.Message}", "OK");
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MainPage());
        }

        private async void OnNavigateToDashboard(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MainPage());
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