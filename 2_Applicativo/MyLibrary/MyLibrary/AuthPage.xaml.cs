using System;
using Microsoft.Maui.Controls;
using MyLibrary.Services;

namespace MyLibrary
{
    public partial class AuthPage : ContentPage
    {
        private readonly AuthService _authService;

        public AuthPage()
        {
            InitializeComponent();
            _authService = new AuthService();
        }

        private async void OnLoginAttemptClick(object sender, EventArgs e)
        {
            try
            {
                DateTime dataCreazione = await _authService.ValidateLoginAsync(LoginUser.Text, LoginPass.Text);

                // Salva la data di creazione nelle preferenze per mostrarla nel profilo
                Preferences.Default.Set(
                    $"{AuthService.CurrentUsername}_CreationDate",
                    dataCreazione.ToString("yyyy-MM-dd")
                );

                // Applica le impostazioni salvate dell'utente
                await App.ApplyGlobalSettingsAsync(AuthService.CurrentUsername);

                // Naviga alla pagina principale
                Application.Current.MainPage = new NavigationPage(new MainPage());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Accesso negato", ex.Message, "Riprova");
            }
        }

        private async void OnRegisterAttemptClick(object sender, EventArgs e)
        {
            try
            {
                await _authService.RegisterUserAsync(RegisterUser.Text, RegisterPass.Text);

                await DisplayAlert("Registrazione completata", "Account creato! Puoi ora effettuare il login.", "OK");

                RegisterUser.Text = string.Empty;
                RegisterPass.Text = string.Empty;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Registrazione fallita", ex.Message, "Correggi");
            }
        }
    }
}
