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
        private const char Delimitatore = '|';

        public SettingsPage()
        {
            InitializeComponent();
            _settingsFilePath = Path.Combine(
                FileSystem.AppDataDirectory,
                $"settings_data_{AuthService.CurrentUsername}.txt"
            );
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CaricaImpostazioniAsync();
        }

        // Legge il file impostazioni e popola i controlli. Usa i valori di default se il file non esiste.
        private async Task CaricaImpostazioniAsync()
        {
            // Valori di default
            PickerTheme.SelectedIndex = 2;    // System Default
            PickerFont.SelectedIndex = 0;      // Default (Open Sans)
            PickerLanguage.SelectedIndex = 0;  // English
            SliderFontSize.Value = 14;
            SwitchHighContrast.IsToggled = false;
            SwitchScreenReader.IsToggled = true;
            SwitchNotifications.IsToggled = true;

            if (!File.Exists(_settingsFilePath)) return;

            try
            {
                string contenuto = await File.ReadAllTextAsync(_settingsFilePath);
                string[] seg = contenuto.Split(Delimitatore);

                // Formato file: tema|font|fontSize|highContrast|screenReader|lingua|notifiche
                if (seg.Length < 7) return;

                // Usa SelectedItem per i Picker così da abbinare per valore, non per indice
                PickerTheme.SelectedItem = seg[0];
                PickerFont.SelectedItem = seg[1];
                SliderFontSize.Value = double.TryParse(seg[2], out double fs) ? fs : 14;
                SwitchHighContrast.IsToggled = bool.TryParse(seg[3], out bool hc) && hc;
                SwitchScreenReader.IsToggled = !bool.TryParse(seg[4], out bool sr) || sr;
                PickerLanguage.SelectedItem = seg[5];
                SwitchNotifications.IsToggled = !bool.TryParse(seg[6], out bool n) || n;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore lettura impostazioni: {ex.Message}");
            }
        }

        // Aggiorna il label del font size mentre si sposta lo slider
        private void OnFontSizeChanged(object sender, ValueChangedEventArgs e)
        {
            LblFontSizeDisplay.Text = $"Dimensione testo: {(int)e.NewValue}";
        }

        // Salva le impostazioni su file e le applica globalmente all'app
        private async void OnSaveSettingsClicked(object sender, EventArgs e)
        {
            try
            {
                string tema = PickerTheme.SelectedItem?.ToString() ?? "System Default";
                string font = PickerFont.SelectedItem?.ToString() ?? "Default (Open Sans)";
                double fontSize = Math.Round(SliderFontSize.Value);
                bool altoContrasto = SwitchHighContrast.IsToggled;
                bool screenReader = SwitchScreenReader.IsToggled;
                string lingua = PickerLanguage.SelectedItem?.ToString() ?? "English";
                bool notifiche = SwitchNotifications.IsToggled;

                // Applica tema visivo all'app
                Application.Current.UserAppTheme = tema switch
                {
                    "Dark" => AppTheme.Dark,
                    "Light" => AppTheme.Light,
                    _ => AppTheme.Unspecified
                };

                // Applica font e dimensione globalmente tramite le risorse dell'app
                Application.Current.Resources["GlobalFontFamily"] = FontFamilyFromLabel(font);
                Application.Current.Resources["GlobalFontSize"] = fontSize;

                // Salva tutte le impostazioni su file
                string datiDaSalvare = string.Join(Delimitatore,
                    tema,
                    font,
                    fontSize.ToString(),
                    altoContrasto.ToString(),
                    screenReader.ToString(),
                    lingua,
                    notifiche.ToString()
                );

                await File.WriteAllTextAsync(_settingsFilePath, datiDaSalvare);

                await DisplayAlert("Salvato", "Impostazioni applicate con successo.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Errore", $"Impossibile salvare le impostazioni: {ex.Message}", "OK");
            }
        }

        // Converte il nome leggibile del font nel nome tecnico per MAUI
        private static string FontFamilyFromLabel(string label) => label switch
        {
            "Serif (Times New Roman)" => "Georgia",
            "Monospace (Consolas)" => "Courier New",
            "Dyslexic Friendly" => "OpenDyslexic",
            _ => "OpenSansRegular"
        };

        private async void OnNavigateToDashboard(object sender, EventArgs e) =>
            await Navigation.PushAsync(new MainPage());

        private async void OnNavigateToAddMedia(object sender, EventArgs e) =>
            await Navigation.PushAsync(new AddMediaPage());

        private async void OnNavigateToStats(object sender, EventArgs e) =>
            await Navigation.PushAsync(new StatsPage());

        private async void OnNavigateToProfile(object sender, EventArgs e) =>
            await Navigation.PushAsync(new ProfilePage());

        private async void OnNavigateToAbout(object sender, EventArgs e) =>
            await Navigation.PushAsync(new AboutPage());
    }
}
