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
                DateTime accountCreatedDate = await _authService.ValidateLoginAsync(LoginUser.Text, LoginPass.Text);
                Preferences.Default.Set($"{AuthService.CurrentUsername}_CreationDate", accountCreatedDate.ToString("yyyy-MM-dd"));
                await App.ApplyGlobalSettingsAsync(AuthService.CurrentUsername);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Application.Current.MainPage = new NavigationPage(new MainPage());
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Login Blocked", ex.Message, "Retry");
            }
        }

        private async void OnRegisterAttemptClick(object sender, EventArgs e)
        {
            try
            {
                bool processed = await _authService.RegisterUserAsync(RegisterUser.Text, RegisterPass.Text);
                if (processed)
                {
                    await DisplayAlert("Registration Success", "Account created! You can now login using the panel on the left.", "OK");

                    // Clear fields out cleanly
                    RegisterUser.Text = string.Empty;
                    RegisterPass.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Registration Denied", ex.Message, "Fix Inputs");
            }
        }
    }
}