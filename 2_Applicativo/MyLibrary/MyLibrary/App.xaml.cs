using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using MyLibrary.Services;

namespace MyLibrary
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new AuthPage());
        }

        // Applica le impostazioni salvate dell'utente all'avvio o dopo il login.
        public static async Task ApplyGlobalSettingsAsync(string username)
        {
            try
            {
                string path = Path.Combine(FileSystem.AppDataDirectory, $"settings_data_{username}.txt");
                if (!File.Exists(path)) return;

                string contenuto = await File.ReadAllTextAsync(path);
                string[] segmenti = contenuto.Split('|');
                if (segmenti.Length < 3) return;

                string tema = segmenti[0];
                string font = segmenti[1];
                double fontSize = double.TryParse(segmenti[2], out double fs) ? fs : 14;

                // Applica il tema visivo
                Current.UserAppTheme = tema switch
                {
                    "Dark" => AppTheme.Dark,
                    "Light" => AppTheme.Light,
                    _ => AppTheme.Unspecified
                };

                // Applica font e dimensione globale
                Current.Resources["GlobalFontFamily"] = font;
                Current.Resources["GlobalFontSize"] = fontSize;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore caricamento impostazioni: {ex.Message}");
            }
        }
    }
}
