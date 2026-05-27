using System;

namespace MyLibrary.Models
{
    public class MediaItem
    {
        private string _title;
        private string _type; // Movie, Book, Comic, TV Series
        private string _genre;
        private int _rating;
        private string _review;
        private double _timeSpentMinutes;

        public Guid Id { get; set; } = Guid.NewGuid();

        public string Title
        {
            get => _title;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Title cannot be empty.");
                _title = value;
            }
        }

        public string Type
        {
            get => _type;
            set => _type = value ?? "Book";
        }

        public string Genre
        {
            get => _genre;
            set => _genre = value ?? "General";
        }

        public int Rating
        {
            get => _rating;
            set
            {
                if (value < 0 || value > 10)
                    throw new ArgumentOutOfRangeException("Rating must be between 0 and 10.");
                _rating = value;
            }
        }

        public string Review
        {
            get => _review;
            set => _review = value ?? string.Empty;
        }

        public double TimeSpentMinutes
        {
            get => _timeSpentMinutes;
            set => _timeSpentMinutes = value >= 0 ? value : 0;
        }

        public bool IsCompleted { get; set; }

        public bool IsFavorite { get; set; } = false;
        public string ConnectedSaga { get; set; } // Keeps track of universe links (e.g. Lord of the Rings)
        public DateTime DateAdded { get; set; } = DateTime.Now;
    }
}