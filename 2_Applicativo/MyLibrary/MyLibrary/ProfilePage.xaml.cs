using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using MyLibrary.Models;
using MyLibrary.Services;

namespace MyLibrary
{
    public partial class ProfilePage : ContentPage
    {
        private TxtStorageService _storageService;
        private string _profileFilePath;
        private const char Delimiter = '|';

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadProfileAndStatsAsync();
        }

        public ProfilePage()
        {
            InitializeComponent();
            _profileFilePath = Path.Combine(FileSystem.AppDataDirectory, $"user_profile_data_{AuthService.CurrentUsername}.txt");
            _storageService = new TxtStorageService();
        }

        private async Task LoadProfileAndStatsAsync()
        {
            try
            {
                string username = AuthService.CurrentUsername;
                string location = string.Empty;

                string cachedDateStr = Preferences.Default.Get($"{AuthService.CurrentUsername}_CreationDate", DateTime.Now.ToString("yyyy-MM-dd"));
                DateTime creationDate = DateTime.Parse(cachedDateStr);

                if (File.Exists(_profileFilePath))
                {
                    string content = await File.ReadAllTextAsync(_profileFilePath);
                    string[] segments = content.Split(Delimiter);

                    if (segments.Length >= 2)
                    {
                        location = segments[0];
                    }
                }

                List<MediaItem> itemsList = await _storageService.LoadMediaItemsAsync();

                int movies = itemsList.Count(i => i.Type.Equals("Movie", StringComparison.OrdinalIgnoreCase));
                int books = itemsList.Count(i => i.Type.Equals("Book", StringComparison.OrdinalIgnoreCase));
                int tv = itemsList.Count(i => i.Type.Equals("TV Series", StringComparison.OrdinalIgnoreCase) || i.Type.Equals("TV", StringComparison.OrdinalIgnoreCase));
                int anime = itemsList.Count(i => i.Type.Equals("Anime", StringComparison.OrdinalIgnoreCase));
                int comics = itemsList.Count(i => i.Type.Equals("Comic", StringComparison.OrdinalIgnoreCase) || i.Type.Equals("Comics", StringComparison.OrdinalIgnoreCase));

                var favoriteTitles = itemsList.Where(i => i.IsFavorite).Select(i => $"• {i.Title} ({i.Type})");
                string favoritesDisplay = string.Join(Environment.NewLine, favoriteTitles);

                // Update read-only interface locks safely
                EntryUsername.Text = username;
                EntryLocation.Text = location;
                PickerCreationDate.Date = creationDate;

                EntryMovies.Text = movies.ToString();
                EntryBooks.Text = books.ToString();
                EntryTV.Text = tv.ToString();
                EntryAnime.Text = anime.ToString();
                EntryComics.Text = comics.ToString();

                EditorFavorites.Text = string.IsNullOrWhiteSpace(favoritesDisplay) ? "No favorites added yet." : favoritesDisplay;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading user dashboard profile elements: {ex.Message}");
            }
        }

        private async void OnSaveProfileClicked(object sender, EventArgs e)
        {
            try
            {
                string newUsername = EntryUsername.Text?.Trim();

                if (string.IsNullOrWhiteSpace(newUsername))
                {
                    await DisplayAlert("Validation Error", "Username cannot be empty.", "OK");
                    return;
                }

                if (!newUsername.Equals(AuthService.CurrentUsername, StringComparison.OrdinalIgnoreCase))
                {
                    var authService = new AuthService();

                    await authService.ChangeUsernameAsync(AuthService.CurrentUsername, newUsername);

                    _profileFilePath = Path.Combine(FileSystem.AppDataDirectory, $"user_profile_data_{AuthService.CurrentUsername}.txt");
                    _storageService = new TxtStorageService();
                }

                string safeLoc = (EntryLocation.Text ?? string.Empty).Replace('|', ' ');

                string serializedData = $"{safeLoc}";

                await File.WriteAllTextAsync(_profileFilePath, serializedData);
                await DisplayAlert("Profile Saved", "Your account has been updated.", "OK");
            }
            catch (InvalidOperationException ex)
            {
                await DisplayAlert("Username unavailable", ex.Message, "OK");
                EntryUsername.Text = AuthService.CurrentUsername;
            }
            catch (Exception ex)
            {
                await DisplayAlert("System Error", $"Could not process changes: {ex.Message}", "OK");
            }
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            AuthService.CurrentUsername = string.Empty;
            Application.Current.MainPage = new NavigationPage(new AuthPage());
        }
        private async void OnDeleteProfileClicked(object sender, EventArgs e)
        {
            string usernameToDelete = AuthService.CurrentUsername;

            if (string.IsNullOrWhiteSpace(usernameToDelete)) return;

            // Double-check user intent with explicit dialog box confirmation
            bool confirm = await DisplayAlert(
                "Confirm Operation",
                $"Are you sure you want to delete profile '{usernameToDelete}'?",
                "Delete",
                "Cancel");

            if (!confirm) return;

            try
            {
                string authFilePath = Path.Combine(FileSystem.AppDataDirectory, "users.txt");
                if (File.Exists(authFilePath))
                {
                    List<string> lines = (await File.ReadAllLinesAsync(authFilePath)).ToList();

                    List<string> optimizedLines = lines
                        .Where(line => !line.Split('|')[0].Equals(usernameToDelete, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    await File.WriteAllLinesAsync(authFilePath, optimizedLines);
                }

                string userLibraryDataFile = Path.Combine(FileSystem.AppDataDirectory, $"library_data_{usernameToDelete}.txt");
                if (File.Exists(userLibraryDataFile)) File.Delete(userLibraryDataFile);

                string userProfileDataFile = Path.Combine(FileSystem.AppDataDirectory, $"user_profile_data_{usernameToDelete}.txt");
                if (File.Exists(userProfileDataFile)) File.Delete(userProfileDataFile);

                string targetPreferenceCacheKey = $"{usernameToDelete}_CreationDate";
                if (Preferences.Default.ContainsKey(targetPreferenceCacheKey))
                {
                    Preferences.Default.Remove(targetPreferenceCacheKey);
                }

                AuthService.CurrentUsername = string.Empty;

                await DisplayAlert("Profile Deleted", "Your account and all your data was deleted.", "OK");
                Application.Current.MainPage = new NavigationPage(new AuthPage());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Wipe Failed", $"An unexpected storage fault occurred during cleanup: {ex.Message}", "OK");
            }
        }

        private async void OnNavigateToDashboard(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MainPage());
        }
        private async void OnNavigateToAddMedia(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddMediaPage());
        }
        private async void OnNavigateToStats(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new StatsPage());
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