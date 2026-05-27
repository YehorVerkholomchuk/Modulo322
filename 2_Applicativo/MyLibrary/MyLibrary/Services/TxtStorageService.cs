using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MyLibrary.Models;

namespace MyLibrary.Services
{
    public class TxtStorageService
    {
        private readonly string _filePath;
        private const char Delimiter = '|';

        public TxtStorageService()
        {
            // Generates files like library_data_Yehor99.txt uniquely matching the logged-in user account
            _filePath = Path.Combine(FileSystem.AppDataDirectory, $"library_data_{AuthService.CurrentUsername}.txt");
        }
        public async Task SaveMediaItemsAsync(List<MediaItem> items)
        {
            try
            {
                List<string> lines = new List<string>();

                foreach (var item in items)
                {
                    // Sanitize potential multi-line text fields so they don't break ReadAllLines execution
                    string sanitizedReview = (item.Review ?? string.Empty).Replace("\r\n", "[NL]").Replace("\n", "[NL]");
                    string sanitizedTitle = (item.Title ?? "Untitled").Replace(Delimiter.ToString(), " ");
                    string sanitizedGenre = (item.Genre ?? "General").Replace(Delimiter.ToString(), " ");
                    string sanitizedSaga = (item.ConnectedSaga ?? string.Empty).Replace(Delimiter.ToString(), " ");

                    // Construct a single delimited string line
                    string line = string.Join(Delimiter,
                        item.Id.ToString(),
                        sanitizedTitle,
                        item.Type,
                        sanitizedGenre,
                        item.Rating.ToString(),
                        item.TimeSpentMinutes.ToString(),
                        item.IsCompleted.ToString(),
                        item.IsFavorite.ToString(),
                        sanitizedSaga,
                        item.DateAdded.ToString("o"), // ISO 8601 Round-trip format
                        sanitizedReview
                    );

                    lines.Add(line);
                }

                // Core requirement: Direct line array disk write output execution
                await File.WriteAllLinesAsync(_filePath, lines);
            }
            catch (UnauthorizedAccessException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Permissions Error: {ex.Message}");
                throw new Exception("Local security policies blocked write operations to the storage ledger.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"File IO Error: {ex.Message}");
                throw new Exception("A system error occurred while generating the text database layout.");
            }
        }
        public async Task<List<MediaItem>> LoadMediaItemsAsync()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    return new List<MediaItem>();
                }

                // Core requirement: Read raw rows array straight from local path
                string[] lines = await File.ReadAllLinesAsync(_filePath);
                List<MediaItem> items = new List<MediaItem>();

                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] segments = line.Split(Delimiter);

                    // Fail-safe protection matching potential database corruptions or schema updates
                    if (segments.Length < 10) continue;

                    try
                    {
                        var item = new MediaItem
                        {
                            Id = Guid.TryParse(segments[0], out Guid parsedId) ? parsedId : Guid.NewGuid(),
                            Title = segments[1],
                            Type = segments[2],
                            Genre = segments[3],
                            Rating = int.TryParse(segments[4], out int r) ? r : 5,
                            TimeSpentMinutes = double.TryParse(segments[5], out double t) ? t : 0,
                            IsCompleted = bool.TryParse(segments[6], out bool c) && c,
                            IsFavorite = bool.TryParse(segments[7], out bool favorite) && favorite,
                            ConnectedSaga = segments[8],
                            DateAdded = DateTime.TryParse(segments[9], out DateTime dt) ? dt : DateTime.Now,
                            // Unescape newlines to display long-form notes cleanly in UI view controls
                            Review = segments[10].Replace("[NL]", Environment.NewLine)
                        };

                        items.Add(item);
                    }
                    catch (Exception lineEx)
                    {
                        // Log corrupted single rows independently without interrupting total load workflow
                        System.Diagnostics.Debug.WriteLine($"Row parsing parsing skipped: {lineEx.Message}");
                    }
                }

                return items;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"File Read Core Failure: {ex.Message}");
                return new List<MediaItem>();
            }
        }
    }
}