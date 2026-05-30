using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

            // Valori predefiniti al caricamento
            PickerType.SelectedIndex = 0;
            PickerRating.SelectedIndex = 4; // voto 5 di default
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(EntryTitle.Text))
                {
                    await DisplayAlert("Errore", "Il campo Titolo è obbligatorio.", "OK");
                    return;
                }

                double minutiSpesi = 0;
                if (!string.IsNullOrWhiteSpace(EntryTime.Text) &&
                    !double.TryParse(EntryTime.Text, out minutiSpesi))
                {
                    await DisplayAlert("Errore", "Il campo Tempo deve essere un numero valido.", "OK");
                    return;
                }

                var nuovoItem = new MediaItem
                {
                    Title = EntryTitle.Text.Trim(),
                    Type = PickerType.SelectedItem?.ToString() ?? "Book",
                    Genre = string.IsNullOrWhiteSpace(EntryGenre.Text) ? "General" : EntryGenre.Text.Trim(),
                    Rating = (int)(PickerRating.SelectedItem ?? 5),
                    TimeSpentMinutes = minutiSpesi,
                    Review = EditorReview.Text?.Trim() ?? string.Empty,
                    IsCompleted = CheckCompleted.IsChecked,
                    IsFavorite = CheckFavorite.IsChecked,
                    DateAdded = DateTime.Now
                };

                List<MediaItem> esistenti = await _storageService.LoadMediaItemsAsync();
                esistenti.Add(nuovoItem);
                await _storageService.SaveMediaItemsAsync(esistenti);

                await DisplayAlert("Salvato", $"\"{nuovoItem.Title}\" aggiunto alla libreria.", "OK");
                await Navigation.PopToRootAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Errore di sistema", $"Impossibile salvare: {ex.Message}", "OK");
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e) =>
            await Navigation.PopAsync();

        private async void OnNavigateToDashboard(object sender, EventArgs e) =>
            await Navigation.PushAsync(new MainPage());

        private async void OnNavigateToStats(object sender, EventArgs e) =>
            await Navigation.PushAsync(new StatsPage());

        private async void OnNavigateToProfile(object sender, EventArgs e) =>
            await Navigation.PushAsync(new ProfilePage());

        private async void OnNavigateToSettings(object sender, EventArgs e) =>
            await Navigation.PushAsync(new SettingsPage());

        private async void OnNavigateToAbout(object sender, EventArgs e) =>
            await Navigation.PushAsync(new AboutPage());
    }
}
