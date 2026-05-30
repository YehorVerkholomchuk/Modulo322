using System;

namespace MyLibrary.Models
{
    public class AppSettings
    {
        private double _fontSize = 14;

        // Tema visivo: Light, Dark, System Default
        public string Theme { get; set; } = "System Default";

        public string FontFamily { get; set; } = "Default (Open Sans)";

        public double FontSize
        {
            get => _fontSize;
            set => _fontSize = Math.Clamp(value, 10, 24);
        }

        public string Language { get; set; } = "English";

        public bool HighContrast { get; set; } = false;

        public bool ScreenReaderOptimized { get; set; } = true;

        public bool EnableNotifications { get; set; } = true;
    }
}
