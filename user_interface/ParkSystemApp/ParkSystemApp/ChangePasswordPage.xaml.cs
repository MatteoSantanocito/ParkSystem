using Microsoft.Maui.Controls;
using ParkSystemApp.Services;
using System;

namespace ParkSystemApp
{
    public partial class ChangePasswordPage : ContentPage
    {
        private readonly ApiService _apiService;

        public ChangePasswordPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
        }

        private async void OnSalvaClicked(object sender, EventArgs e)
        {
            var oldPass = OldPasswordEntry.Text?.Trim();
            var newPass = NewPasswordEntry.Text?.Trim();
            var confNewPass = ConfirmNewPasswordEntry.Text?.Trim();

            if (string.IsNullOrWhiteSpace(oldPass) ||
                string.IsNullOrWhiteSpace(newPass) ||
                string.IsNullOrWhiteSpace(confNewPass))
            {
                await DisplayAlert("Errore", "Compila tutti i campi.", "OK");
                return;
            }

            if (newPass != confNewPass)
            {
                await DisplayAlert("Errore", "La nuova password e la conferma non coincidono.", "OK");
                return;
            }

            var result = await _apiService.ChangePasswordAsync(oldPass, newPass);
            if (result.StartsWith("Errore"))
            {
                await DisplayAlert("Errore", result, "OK");
            }
            else
            {
                await DisplayAlert("Successo", "Password aggiornata correttamente", "OK");
                await Shell.Current.GoToAsync("Profilo");
            }
        }

        private async void OnAnnullaClicked(object sender, EventArgs e)
        {
            // Torno al profilo senza salvare
            await Shell.Current.GoToAsync("Profilo");
        }
    }
}
