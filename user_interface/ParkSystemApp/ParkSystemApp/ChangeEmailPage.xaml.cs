using Microsoft.Maui.Controls;
using ParkSystemApp.Services; 
using System;

namespace ParkSystemApp
{
    public partial class ChangeEmailPage : ContentPage
    {
        private readonly ApiService _apiService;

        public ChangeEmailPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
        }

        private async void OnSalvaClicked(object sender, EventArgs e)
        {
            var newEmail = NewEmailEntry.Text?.Trim();
            if (string.IsNullOrWhiteSpace(newEmail))
            {
                await DisplayAlert("Errore", "L'email non può essere vuota.", "OK");
                return;
            }

            bool conferma = await DisplayAlert("Conferma", "Sei sicuro di voler modificare la tua email?", "Si", "No");
            if (!conferma)
            {
                return; // L'utente ha annullato l'operazione
            }

            // Chiamata all’API
            var result = await _apiService.ChangeEmailAsync(newEmail);

            if (result.StartsWith("Errore"))
            {
                await DisplayAlert("Errore", result, "OK");
            }
            else
            {
                // Aggiorna localmente
                Preferences.Set("UserEmail", newEmail);

                await DisplayAlert("Successo", "Email aggiornata correttamente", "OK");

                // Torniamo al profilo (o dove preferisci)
                await Shell.Current.GoToAsync("Profilo");
            }
        }

        private async void OnAnnullaClicked(object sender, EventArgs e)
        {
            // Torna semplicemente al profilo senza cambiare nulla
            await Shell.Current.GoToAsync("Profilo");
        }
    }
}
