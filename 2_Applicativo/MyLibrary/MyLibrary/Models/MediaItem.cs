using System;

namespace MyLibrary.Models
{
    public class MediaItem
    {
        private string _title;
        private int _rating;

        public Guid Id { get; set; } = Guid.NewGuid();

        public string Title
        {
            get => _title;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Il titolo non può essere vuoto.");
                _title = value;
            }
        }

        // Tipo di media: Movie, Book, Comic, TV Series, Anime
        public string Type { get; set; } = "Book";

        public string Genre { get; set; } = "General";

        public int Rating
        {
            get => _rating;
            set
            {
                if (value < 0 || value > 10)
                    throw new ArgumentOutOfRangeException(nameof(Rating), "Il voto deve essere tra 0 e 10.");
                _rating = value;
            }
        }

        public string Review { get; set; } = string.Empty;

        // Minuti totali dedicati a questo media
        public double TimeSpentMinutes { get; set; }

        public bool IsCompleted { get; set; }

        public bool IsFavorite { get; set; }

        // Saga/universo di appartenenza (es. "Il Signore degli Anelli")
        public string ConnectedSaga { get; set; } = string.Empty;

        public DateTime DateAdded { get; set; } = DateTime.Now;
    }
}
