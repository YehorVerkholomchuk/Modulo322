using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MyLibrary.Models;

namespace MyLibrary.Services
{
    // Gestisce la lettura e scrittura dei media sul file .txt dell'utente loggato.
    public class TxtStorageService
    {
        private readonly string _filePath;
        private const char Delimitatore = '|';

        public TxtStorageService()
        {
            _filePath = Path.Combine(FileSystem.AppDataDirectory, $"library_data_{AuthService.CurrentUsername}.txt");
        }

        // Salva la lista completa dei media su disco (sovrascrive il file esistente).
        public async Task SaveMediaItemsAsync(List<MediaItem> items)
        {
            var righe = new List<string>();

            foreach (var item in items)
            {
                // Escape delle newline nei campi testuali per non rompere la lettura riga per riga
                string titolo = SanitizeField(item.Title ?? "Senza titolo");
                string genere = SanitizeField(item.Genre ?? "General");
                string saga = SanitizeField(item.ConnectedSaga ?? string.Empty);
                string recensione = (item.Review ?? string.Empty)
                    .Replace("\r\n", "[NL]")
                    .Replace("\n", "[NL]");

                string riga = string.Join(Delimitatore,
                    item.Id.ToString(),
                    titolo,
                    item.Type ?? "Book",
                    genere,
                    item.Rating.ToString(),
                    item.TimeSpentMinutes.ToString(),
                    item.IsCompleted.ToString(),
                    item.IsFavorite.ToString(),
                    saga,
                    item.DateAdded.ToString("o"), // formato ISO 8601
                    recensione
                );

                righe.Add(riga);
            }

            await File.WriteAllLinesAsync(_filePath, righe);
        }

        // Carica tutti i media salvati dell'utente corrente.
        public async Task<List<MediaItem>> LoadMediaItemsAsync()
        {
            if (!File.Exists(_filePath))
                return new List<MediaItem>();

            string[] lines = await File.ReadAllLinesAsync(_filePath);
            var items = new List<MediaItem>();

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] seg = line.Split(Delimitatore);
                if (seg.Length < 11) continue; // riga corrotta, salta

                try
                {
                    var item = new MediaItem
                    {
                        Id = Guid.TryParse(seg[0], out Guid id) ? id : Guid.NewGuid(),
                        Title = seg[1],
                        Type = seg[2],
                        Genre = seg[3],
                        Rating = int.TryParse(seg[4], out int r) ? r : 5,
                        TimeSpentMinutes = double.TryParse(seg[5], out double t) ? t : 0,
                        IsCompleted = bool.TryParse(seg[6], out bool c) && c,
                        IsFavorite = bool.TryParse(seg[7], out bool f) && f,
                        ConnectedSaga = seg[8],
                        DateAdded = DateTime.TryParse(seg[9], out DateTime dt) ? dt : DateTime.Now,
                        Review = seg[10].Replace("[NL]", Environment.NewLine)
                    };

                    items.Add(item);
                }
                catch (Exception ex)
                {
                    // Riga corrotta: viene saltata senza interrompere il caricamento
                    System.Diagnostics.Debug.WriteLine($"Riga saltata: {ex.Message}");
                }
            }

            return items;
        }

        // Rimuove il delimitatore dai campi per non rompere il formato del file
        private string SanitizeField(string value) => value.Replace(Delimitatore.ToString(), " ");
    }
}
