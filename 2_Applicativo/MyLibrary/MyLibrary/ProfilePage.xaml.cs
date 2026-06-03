using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using MyLibrary.Models;
using MyLibrary.Services;

namespace MyLibrary
{
    public partial class ProfilePage : ContentPage
    {
        private readonly TxtStorageService _storageService;
        private string _profileFilePath;

        public ProfilePage()
        {
            InitializeComponent();
            _storageService = new TxtStorageService();
            _profileFilePath = GetProfilePath(AuthService.CurrentUsername);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CaricaProfiloAsync();
        }

        private async Task CaricaProfiloAsync()
        {
            try
            {
                // Recupera la data di creazione salvata nelle preferenze al momento del login
                string dataSalvata = Preferences.Default.Get(
                    $"{AuthService.CurrentUsername}_CreationDate",
                    DateTime.Now.ToString("yyyy-MM-dd")
                );
                DateTime dataCreazione = DateTime.TryParse(dataSalvata, out DateTime d) ? d : DateTime.Now;

                // Legge la posizione dal file profilo se esiste
                string posizione = string.Empty;
                if (File.Exists(_profileFilePath))
                {
                    string contenuto = await File.ReadAllTextAsync(_profileFilePath);
                    posizione = contenuto.Split('|')[0];
                }

                // Conta i media per tipo dalla libreria
                List<MediaItem> items = await _storageService.LoadMediaItemsAsync();

                int film = items.Count(i => i.Type.Equals("Movie", StringComparison.OrdinalIgnoreCase));
                int libri = items.Count(i => i.Type.Equals("Book", StringComparison.OrdinalIgnoreCase));
                int serie = items.Count(i => i.Type.Equals("TV Series", StringComparison.OrdinalIgnoreCase));
                int anime = items.Count(i => i.Type.Equals("Anime", StringComparison.OrdinalIgnoreCase));
                int fumetti = items.Count(i => i.Type.Equals("Comic", StringComparison.OrdinalIgnoreCase));

                // Costruisce la lista dei preferiti
                var preferiti = items
                    .Where(i => i.IsFavorite)
                    .Select(i => $"• {i.Title} ({i.Type})");
                string testoPreferiti = preferiti.Any()
                    ? string.Join(Environment.NewLine, preferiti)
                    : "Nessun preferito aggiunto.";

                // Aggiorna la UI
                EntryUsername.Text = AuthService.CurrentUsername;
                EntryLocation.Text = posizione;
                PickerCreationDate.Date = dataCreazione;
                EntryMovies.Text = film.ToString();
                EntryBooks.Text = libri.ToString();
                EntryTV.Text = serie.ToString();
                EntryAnime.Text = anime.ToString();
                EntryComics.Text = fumetti.ToString();
                EditorFavorites.Text = testoPreferiti;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore caricamento profilo: {ex.Message}");
            }
        }

        private async void OnSaveProfileClicked(object sender, EventArgs e)
        {
            try
            {
                string nuovoUsername = EntryUsername.Text?.Trim();

                if (string.IsNullOrWhiteSpace(nuovoUsername))
                {
                    await DisplayAlert("Errore", "L'username non può essere vuoto.", "OK");
                    return;
                }

                // Cambia username se è stato modificato
                if (!nuovoUsername.Equals(AuthService.CurrentUsername, StringComparison.OrdinalIgnoreCase))
                {
                    var authService = new AuthService();
                    await authService.ChangeUsernameAsync(AuthService.CurrentUsername, nuovoUsername);
                    _profileFilePath = GetProfilePath(AuthService.CurrentUsername);
                }

                // Salva la posizione (rimuove il delimitatore per sicurezza)
                string posizioneSicura = (EntryLocation.Text ?? string.Empty).Replace("|", " ");
                await File.WriteAllTextAsync(_profileFilePath, posizioneSicura);

                await DisplayAlert("Profilo salvato", "Le modifiche sono state applicate.", "OK");
            }
            catch (InvalidOperationException ex)
            {
                await DisplayAlert("Username non disponibile", ex.Message, "OK");
                EntryUsername.Text = AuthService.CurrentUsername;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Errore di sistema", $"Impossibile salvare: {ex.Message}", "OK");
            }
        }

        private void OnLogoutClicked(object sender, EventArgs e)
        {
            AuthService.CurrentUsername = string.Empty;
            Application.Current.MainPage = new NavigationPage(new AuthPage());
        }

        private async void OnDeleteProfileClicked(object sender, EventArgs e)
        {
            string username = AuthService.CurrentUsername;
            if (string.IsNullOrWhiteSpace(username)) return;

            bool conferma = await DisplayAlert(
                "Conferma eliminazione",
                $"Sei sicuro di voler eliminare il profilo '{username}' e tutti i suoi dati?",
                "Elimina",
                "Annulla"
            );

            if (!conferma) return;

            try
            {
                // Rimuove l'utente dal file di autenticazione
                string authPath = Path.Combine(FileSystem.AppDataDirectory, "users.txt");
                if (File.Exists(authPath))
                {
                    var righe = (await File.ReadAllLinesAsync(authPath)).ToList();
                    righe.RemoveAll(r => r.Split('|')[0].Equals(username, StringComparison.OrdinalIgnoreCase));
                    await File.WriteAllLinesAsync(authPath, righe);
                }

                // Elimina i file dati dell'utente
                EliminaFileSeEsiste(Path.Combine(FileSystem.AppDataDirectory, $"library_data_{username}.txt"));
                EliminaFileSeEsiste(Path.Combine(FileSystem.AppDataDirectory, $"user_profile_data_{username}.txt"));
                EliminaFileSeEsiste(Path.Combine(FileSystem.AppDataDirectory, $"settings_data_{username}.txt"));

                // Rimuove le preferenze salvate
                string chiave = $"{username}_CreationDate";
                if (Preferences.Default.ContainsKey(chiave))
                    Preferences.Default.Remove(chiave);

                AuthService.CurrentUsername = string.Empty;
                await DisplayAlert("Profilo eliminato", "Account e dati rimossi con successo.", "OK");
                Application.Current.MainPage = new NavigationPage(new AuthPage());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Errore", $"Eliminazione non riuscita: {ex.Message}", "OK");
            }
        }

        private static string GetProfilePath(string username) =>
            Path.Combine(FileSystem.AppDataDirectory, $"user_profile_data_{username}.txt");

        private static void EliminaFileSeEsiste(string path)
        {
            if (File.Exists(path)) File.Delete(path);
        }

        private async void OnNavigateToDashboard(object sender, EventArgs e) =>
            await Navigation.PushAsync(new MainPage());

        private async void OnNavigateToAddMedia(object sender, EventArgs e) =>
            await Navigation.PushAsync(new AddMediaPage());

        private async void OnNavigateToStats(object sender, EventArgs e) =>
            await Navigation.PushAsync(new StatsPage());

        private async void OnNavigateToSettings(object sender, EventArgs e) =>
            await Navigation.PushAsync(new SettingsPage());

        private async void OnNavigateToAbout(object sender, EventArgs e) =>
            await Navigation.PushAsync(new AboutPage());
    }
}
