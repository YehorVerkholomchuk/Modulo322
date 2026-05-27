using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MyLibrary.Services
{
    public class AuthService
    {
        private readonly string _authFilePath;
        private const char Delimiter = '|';
        public static string CurrentUsername { get; set; } = string.Empty;

        public AuthService()
        {
            _authFilePath = Path.Combine(FileSystem.AppDataDirectory, "users.txt");
        }

        public async Task<bool> RegisterUserAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Username and password fields cannot be left empty.");

            username = username.Trim();

            try
            {
                List<string> lines = File.Exists(_authFilePath)
                    ? (await File.ReadAllLinesAsync(_authFilePath)).ToList()
                    : new List<string>();

                // Check if user already exists
                foreach (string line in lines)
                {
                    string[] segments = line.Split(Delimiter);
                    if (segments.Length > 0 && segments[0].Equals(username, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException("This username is already taken.");
                    }
                }

                // Append new account parameters. Store creation date here to lock it down automatically.
                string newAccountLine = $"{username}{Delimiter}{password}{Delimiter}{DateTime.Now:yyyy-MM-dd}";
                lines.Add(newAccountLine);

                await File.WriteAllLinesAsync(_authFilePath, lines);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DateTime> ValidateLoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Please fill out all credential inputs.");

            username = username.Trim();

            if (!File.Exists(_authFilePath))
                throw new FileNotFoundException("No accounts found on this machine. Please register first.");

            string[] lines = await File.ReadAllLinesAsync(_authFilePath);

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] segments = line.Split(Delimiter);

                if (segments.Length >= 2 && segments[0].Equals(username, StringComparison.OrdinalIgnoreCase))
                {
                    if (segments[1] == password)
                    {
                        CurrentUsername = segments[0];

                        return segments.Length >= 3 && DateTime.TryParse(segments[2], out DateTime parsedDate)
                            ? parsedDate
                            : DateTime.Now;
                    }
                    throw new UnauthorizedAccessException("Incorrect password entered.");
                }
            }

            throw new KeyNotFoundException("Account credentials not recognized.");
        }

        public async Task<bool> ChangeUsernameAsync(string oldUsername, string newUsername)
        {
            if (string.IsNullOrWhiteSpace(newUsername))
                throw new ArgumentException("Username cannot be empty.");

            newUsername = newUsername.Trim();

            if (oldUsername.Equals(newUsername, StringComparison.OrdinalIgnoreCase))
                return true;

            if (!File.Exists(_authFilePath))
                throw new FileNotFoundException("Auth database missing.");

            List<string> lines = (await File.ReadAllLinesAsync(_authFilePath)).ToList();

            bool isTaken = lines.Any(l => l.Split(Delimiter)[0].Equals(newUsername, StringComparison.OrdinalIgnoreCase));
            if (isTaken)
                throw new InvalidOperationException("This username is already taken.");

            for (int i = 0; i < lines.Count; i++)
            {
                string[] segments = lines[i].Split(Delimiter);
                if (segments[0].Equals(oldUsername, StringComparison.OrdinalIgnoreCase))
                {
                    segments[0] = newUsername;
                    lines[i] = string.Join(Delimiter, segments);
                    break;
                }
            }
            await File.WriteAllLinesAsync(_authFilePath, lines);

            string oldLibraryPath = Path.Combine(FileSystem.AppDataDirectory, $"library_data_{oldUsername}.txt");
            string newLibraryPath = Path.Combine(FileSystem.AppDataDirectory, $"library_data_{newUsername}.txt");
            if (File.Exists(oldLibraryPath)) File.Move(oldLibraryPath, newLibraryPath);

            string oldProfilePath = Path.Combine(FileSystem.AppDataDirectory, $"user_profile_data_{oldUsername}.txt");
            string newProfilePath = Path.Combine(FileSystem.AppDataDirectory, $"user_profile_data_{newUsername}.txt");
            if (File.Exists(oldProfilePath)) File.Move(oldProfilePath, newProfilePath);

            string cachedDate = Preferences.Default.Get($"{oldUsername}_CreationDate", DateTime.Now.ToString("yyyy-MM-dd"));
            Preferences.Default.Remove($"{oldUsername}_CreationDate");
            Preferences.Default.Set($"{newUsername}_CreationDate", cachedDate);

            CurrentUsername = newUsername;

            return true;
        }
    }
}