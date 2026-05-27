using System;

namespace MyLibrary.Models
{
    public class Profile
    {
        public string Username { get; set; } = "";
        public string Location { get; set; } = "";
        public DateTime CreationDate { get; set; } = DateTime.Now;

        public int MoviesWatched { get; set; }
        public int BooksRead { get; set; }
        public int TvWatched { get; set; }
        public int AnimeWatched { get; set; }
        public int ComicsWatched { get; set; }
        public string FavoriteMovies { get; set; } = string.Empty;
    }
}