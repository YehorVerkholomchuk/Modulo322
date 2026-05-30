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
            await CaricaStatisticheAsync();
        }

        private async Task CaricaStatisticheAsync()
        {
            try
            {
                List<MediaItem> items = await _storageService.LoadMediaItemsAsync();

                if (items == null || items.Count == 0)
                {
                    AzzeraContatori();
                    return;
                }

                int totale = items.Count;
                int libri = items.Count(i => i.Type.Equals("Book", StringComparison.OrdinalIgnoreCase));
                int film = items.Count(i => i.Type.Equals("Movie", StringComparison.OrdinalIgnoreCase));
                double mediaVoto = items.Average(i => i.Rating);
                double percCompletati = (double)items.Count(i => i.IsCompleted) / totale * 100;

                // Raggruppa per genere e ordina per quantità
                var distribuzione = items
                    .GroupBy(i => string.IsNullOrWhiteSpace(i.Genre) ? "General" : i.Genre)
                    .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                    .OrderByDescending(kvp => kvp.Value)
                    .ToList();

                // Top 5 per tempo investito
                var topTempo = items
                    .OrderByDescending(i => i.TimeSpentMinutes)
                    .Take(5)
                    .ToList();

                // Aggiorna i label
                lblTotalBooks.Text = libri.ToString();
                lblTotalMovies.Text = film.ToString();
                lblAvgRating.Text = mediaVoto.ToString("F1");
                lblCompletionRate.Text = $"{percCompletati:F0}%";

                GenreListView.ItemsSource = distribuzione;
                TopTimeCollectionView.ItemsSource = topTempo;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore statistiche: {ex.Message}");
                await DisplayAlert("Errore", "Impossibile caricare le statistiche.", "OK");
            }
        }

        private void AzzeraContatori()
        {
            lblTotalBooks.Text = "0";
            lblTotalMovies.Text = "0";
            lblAvgRating.Text = "0";
            lblCompletionRate.Text = "0%";
            GenreListView.ItemsSource = null;
            TopTimeCollectionView.ItemsSource = null;
        }

        private async void OnNavigateToDashboard(object sender, EventArgs e) =>
            await Navigation.PushAsync(new MainPage());

        private async void OnNavigateToAddMedia(object sender, EventArgs e) =>
            await Navigation.PushAsync(new AddMediaPage());

        private async void OnNavigateToProfile(object sender, EventArgs e) =>
            await Navigation.PushAsync(new ProfilePage());

        private async void OnNavigateToSettings(object sender, EventArgs e) =>
            await Navigation.PushAsync(new SettingsPage());

        private async void OnNavigateToAbout(object sender, EventArgs e) =>
            await Navigation.PushAsync(new AboutPage());
    }
}
