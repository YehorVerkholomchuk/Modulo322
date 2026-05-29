using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using MyLibrary.Services; // Ensure this is here to access AuthService

namespace MyLibrary
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new AuthPage());
        }

        public static async Task ApplyGlobalSettingsAsync(string username)
        {
            try
            {
                string settingsPath = Path.Combine(FileSystem.AppDataDirectory, $"settings_data_{username}.txt");

                if (File.Exists(settingsPath))
                {
                    string content = await File.ReadAllTextAsync(settingsPath);
                    string[] segments = content.Split('|');

                    if (segments.Length >= 3)
                    {
                        string theme = segments[0];
                        string font = segments[1];
                        double fontSize = double.Parse(segments[2]);

                        if (theme == "Dark") Current.UserAppTheme = AppTheme.Dark;
                        else if (theme == "Light") Current.UserAppTheme = AppTheme.Light;
                        else Current.UserAppTheme = AppTheme.Unspecified;

                        Current.Resources["GlobalFontFamily"] = font;
                        Current.Resources["GlobalFontSize"] = fontSize;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load global settings: {ex.Message}");
            }
        }
    }
}