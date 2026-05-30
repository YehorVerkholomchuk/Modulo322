using System;

namespace MyLibrary.Models
{
    public class Profile
    {
        public string Username { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public DateTime CreationDate { get; set; } = DateTime.Now;

        // Contatori aggiornati automaticamente dai media salvati
        public int MoviesWatched { get; set; }
        public int BooksRead { get; set; }
        public int TvWatched { get; set; }
        public int AnimeWatched { get; set; }
        public int ComicsRead { get; set; }

        public string FavoritesList { get; set; } = string.Empty;
    }
}
