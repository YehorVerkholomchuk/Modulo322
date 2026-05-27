using System;

namespace MyLibrary.Models
{
    public class AppSettings
    {
        private double _fontSize;

        public string Theme { get; set; } = "System"; // System, Light, Dark
        public string FontFamily { get; set; } = "Default";

        public double FontSize
        {
            get => _fontSize;
            set => _fontSize = Math.Clamp(value, 10, 24); // Prevents font from becoming unreadably small or massive
        }

        // Accessibility Properties
        public bool HighContrast { get; set; } = false;
        public bool ScreenReaderOptimized { get; set; } = true;

        // General Properties
        public string Language { get; set; } = "English";
        public bool EnableNotifications { get; set; } = true;

        public AppSettings()
        {
            FontSize = 14; // Default safe size
        }
    }
}