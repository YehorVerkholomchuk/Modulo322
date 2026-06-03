using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyLibrary.Services
{
    public class AuthService
    {
        private readonly string _authFilePath;
        private const char Delimitatore = '|';

        // Utente attualmente loggato, accessibile globalmente
        public static string CurrentUsername { get; set; } = string.Empty;

        public AuthService()
        {
            _authFilePath = Path.Combine(FileSystem.AppDataDirectory, "users.txt");
        }

        // Registra un nuovo utente. Lancia eccezione se il nome è già in uso.
        public async Task<bool> RegisterUserAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Username e password non possono essere vuoti.");

            username = username.Trim();

            List<string> lines = File.Exists(_authFilePath)
                ? (await File.ReadAllLinesAsync(_authFilePath)).ToList()
                : new List<string>();

            bool esisteGia = lines.Any(l => l.Split(Delimitatore)[0].Equals(username, StringComparison.OrdinalIgnoreCase));
            if (esisteGia)
                throw new InvalidOperationException("Questo username è già in uso.");

            // Formato riga: username|password|data_creazione
            string nuovaRiga = $"{username}{Delimitatore}{password}{Delimitatore}{DateTime.Now:yyyy-MM-dd}";
            lines.Add(nuovaRiga);
            await File.WriteAllLinesAsync(_authFilePath, lines);

            return true;
        }

        // Valida le credenziali e restituisce la data di creazione dell'account.
        public async Task<DateTime> ValidateLoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Inserisci username e password.");

            username = username.Trim();

            if (!File.Exists(_authFilePath))
                throw new FileNotFoundException("Nessun account trovato. Registrati prima.");

            string[] lines = await File.ReadAllLinesAsync(_authFilePath);

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] segmenti = line.Split(Delimitatore);
                if (segmenti.Length < 2) continue;

                if (!segmenti[0].Equals(username, StringComparison.OrdinalIgnoreCase)) continue;

                if (segmenti[1] != password)
                    throw new UnauthorizedAccessException("Password errata.");

                CurrentUsername = segmenti[0];

                return segmenti.Length >= 3 && DateTime.TryParse(segmenti[2], out DateTime data)
                    ? data
                    : DateTime.Now;
            }

            throw new KeyNotFoundException("Account non trovato.");
        }

        // Cambia l'username dell'utente loggato e rinomina i file associati.
        public async Task ChangeUsernameAsync(string vecchioUsername, string nuovoUsername)
        {
            if (string.IsNullOrWhiteSpace(nuovoUsername))
                throw new ArgumentException("Il nuovo username non può essere vuoto.");

            nuovoUsername = nuovoUsername.Trim();

            if (vecchioUsername.Equals(nuovoUsername, StringComparison.OrdinalIgnoreCase))
                return;

            if (!File.Exists(_authFilePath))
                throw new FileNotFoundException("File utenti non trovato.");

            List<string> lines = (await File.ReadAllLinesAsync(_authFilePath)).ToList();

            bool giàUsato = lines.Any(l => l.Split(Delimitatore)[0].Equals(nuovoUsername, StringComparison.OrdinalIgnoreCase));
            if (giàUsato)
                throw new InvalidOperationException("Questo username è già in uso.");

            // Aggiorna la riga dell'utente nel file
            for (int i = 0; i < lines.Count; i++)
            {
                string[] segmenti = lines[i].Split(Delimitatore);
                if (!segmenti[0].Equals(vecchioUsername, StringComparison.OrdinalIgnoreCase)) continue;

                segmenti[0] = nuovoUsername;
                lines[i] = string.Join(Delimitatore, segmenti);
                break;
            }

            await File.WriteAllLinesAsync(_authFilePath, lines);

            // Rinomina i file dati associati all'utente
            RenameFileIfExists(
                Path.Combine(FileSystem.AppDataDirectory, $"library_data_{vecchioUsername}.txt"),
                Path.Combine(FileSystem.AppDataDirectory, $"library_data_{nuovoUsername}.txt")
            );
            RenameFileIfExists(
                Path.Combine(FileSystem.AppDataDirectory, $"user_profile_data_{vecchioUsername}.txt"),
                Path.Combine(FileSystem.AppDataDirectory, $"user_profile_data_{nuovoUsername}.txt")
            );
            RenameFileIfExists(
                Path.Combine(FileSystem.AppDataDirectory, $"settings_data_{vecchioUsername}.txt"),
                Path.Combine(FileSystem.AppDataDirectory, $"settings_data_{nuovoUsername}.txt")
            );

            // Aggiorna la preferenza della data di creazione
            string chiaveVecchia = $"{vecchioUsername}_CreationDate";
            string chiaveNuova = $"{nuovoUsername}_CreationDate";
            string dataSalvata = Preferences.Default.Get(chiaveVecchia, DateTime.Now.ToString("yyyy-MM-dd"));
            Preferences.Default.Remove(chiaveVecchia);
            Preferences.Default.Set(chiaveNuova, dataSalvata);

            CurrentUsername = nuovoUsername;
        }

        private static void RenameFileIfExists(string oldPath, string newPath)
        {
            if (File.Exists(oldPath))
                File.Move(oldPath, newPath);
        }
    }
}
