using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using MyLibrary.Models;
using MyLibrary.Services;

namespace MyLibrary
{
    public partial class SettingsPage : ContentPage
    {
        private readonly string _settingsFilePath;
        private const char Delimiter = '|';

        public SettingsPage()
        {
            InitializeComponent();

            _settingsFilePath = Path.Combine(FileSystem.AppDataDirectory, $"settings_data_{AuthService.CurrentUsername}.txt");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadSettingsAsync();
        }

        private async Task LoadSettingsAsync()
        {
            try
            {
                PickerTheme.SelectedIndex = 0; // Light
                PickerFont.SelectedIndex = 0; // Default
                PickerLanguage.SelectedIndex = 0; // English
                SliderFontSize.Value = 14;
                SwitchHighContrast.IsToggled = false;
                SwitchScreenReader.IsToggled = true;
                SwitchNotifications.IsToggled = true;

                if (!File.Exists(_settingsFilePath)) return;

                string content = await File.ReadAllTextAsync(_settingsFilePath);
                string[] segments = content.Split(Delimiter);

                if (segments.Length >= 7)
                {
                    PickerTheme.SelectedItem = segments[0];
                    PickerFont.SelectedItem = segments[1];
                    SliderFontSize.Value = double.TryParse(segments[2], out double fs) ? fs : 14;
                    SwitchHighContrast.IsToggled = bool.TryParse(segments[3], out bool hc) && hc;
                    SwitchScreenReader.IsToggled = bool.TryParse(segments[4], out bool sr) && sr;
                    PickerLanguage.SelectedItem = segments[5];
                    SwitchNotifications.IsToggled = bool.TryParse(segments[6], out bool n) && n;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Settings Load Error: {ex.Message}");
            }
        }

        private void OnFontSizeChanged(object sender, ValueChangedEventArgs e)
        {
            LblFontSizeDisplay.Text = $"Global Font Size: {Math.Round(e.NewValue)}";
        }

        private async void OnSaveSettingsClicked(object sender, EventArgs e)
        {
            try
            {
                string selectedTheme = PickerTheme.SelectedItem?.ToString() ?? "System Default";
                if (selectedTheme == "Dark") Application.Current.UserAppTheme = AppTheme.Dark;
                else if (selectedTheme == "Light") Application.Current.UserAppTheme = AppTheme.Light;
                else Application.Current.UserAppTheme = AppTheme.Unspecified;

                Application.Current.Resources["GlobalFontFamily"] = PickerFont.SelectedItem?.ToString() ?? "OpenSansRegular";
                Application.Current.Resources["GlobalFontSize"] = Math.Round(SliderFontSize.Value);

                await DisplayAlert("Success", "Settings applied globally!", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("System Error", $"Could not save settings: {ex.Message}", "OK");
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
        private async void OnNavigateToProfile(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ProfilePage());
        }
        private async void OnNavigateToAbout(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AboutPage());
        }
    }
}