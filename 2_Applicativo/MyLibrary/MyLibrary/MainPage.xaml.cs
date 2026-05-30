using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using MyLibrary.Models;
using MyLibrary.Services;

namespace MyLibrary
{
    public partial class MainPage : ContentPage
    {
        private readonly TxtStorageService _storageService;
        private List<MediaItem> _tuttiGliItem;

        public ObservableCollection<MediaItem> ItemVisualizzati { get; set; }

        public MainPage()
        {
            InitializeComponent();
            _storageService = new TxtStorageService();
            ItemVisualizzati = new ObservableCollection<MediaItem>();
            MediaCollectionView.ItemsSource = ItemVisualizzati;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CaricaDatiAsync();
        }

        private async Task CaricaDatiAsync()
        {
            try
            {
                _tuttiGliItem = await _storageService.LoadMediaItemsAsync();
                AggiornataGriglia(MediaSearchBar.Text);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Errore", $"Impossibile caricare i dati: {ex.Message}", "OK");
            }
        }

        // Filtra la lista e aggiorna i contatori in base alla query di ricerca
        private void AggiornataGriglia(string query)
        {
            if (_tuttiGliItem == null) return;

            var filtrati = string.IsNullOrWhiteSpace(query)
                ? _tuttiGliItem
                : _tuttiGliItem.Where(i =>
                    i.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    i.Genre.Contains(query, StringComparison.OrdinalIgnoreCase)
                ).ToList();

            ItemVisualizzati.Clear();
            foreach (var item in filtrati)
                ItemVisualizzati.Add(item);

            // Aggiorna i contatori in base a TUTTI i media, non solo quelli filtrati
            lblTotalCount.Text = _tuttiGliItem.Count.ToString();
            lblCompletedCount.Text = _tuttiGliItem.Count(i => i.IsCompleted).ToString();
            lblTotalHours.Text = $"{_tuttiGliItem.Sum(i => i.TimeSpentMinutes) / 60.0:F1} hrs";
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            AggiornataGriglia(e.NewTextValue);
        }

        // Mostra la recensione del media selezionato in un dialog
        private async void OnMediaSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is not MediaItem selected) return;

            string corpo = string.IsNullOrWhiteSpace(selected.Review)
                ? "Nessuna recensione inserita."
                : selected.Review;

            await DisplayAlert(selected.Title, corpo, "Chiudi");
            MediaCollectionView.SelectedItem = null;
        }

        private async void OnNavigateToAddMedia(object sender, EventArgs e) =>
            await Navigation.PushAsync(new AddMediaPage());

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
