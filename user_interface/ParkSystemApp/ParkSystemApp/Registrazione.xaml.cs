using ParkSystemApp.Services; // Per usare ApiService
using System;

namespace ParkSystemApp
{
    public partial class Registrazione : ContentPage
    {
        private readonly ApiService _apiService;

        public Registrazione()
        {
            InitializeComponent();
            _apiService = new ApiService();
        }

        private async void ButtonRegistrati_Clicked(object sender, EventArgs e)
        {
            // Recupera i valori dagli Entry
            string nome = NomeEntry.Text?.Trim();
            string cognome = CognomeEntry.Text?.Trim();
            string tipoAvventura = TipoAvventuraPicker.SelectedItem?.ToString();
            string email = EmailEntry.Text?.Trim();
            string password = PasswordEntry.Text;
            string confermaPassword = ConfermaPasswordEntry.Text;

            // Controlli di base
            if (string.IsNullOrWhiteSpace(nome) ||
                string.IsNullOrWhiteSpace(cognome) ||
                string.IsNullOrWhiteSpace(tipoAvventura) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confermaPassword))
            {
                await DisplayAlert("Errore", "Compila tutti i campi.", "OK");
                return;
            }

            if (password != confermaPassword)
            {
                await DisplayAlert("Errore", "Le password non coincidono.", "OK");
                return;
            }

            // Chiamata all'API di registrazione
            string risultato = await _apiService.RegisterAsync(nome, cognome, tipoAvventura, email, password);

            if (risultato.StartsWith("Errore"))
            {
                // Qualcosa è andato storto
                await DisplayAlert("Errore Registrazione", risultato, "OK");
            }
            else
            {
                // Successo
                await DisplayAlert("Registrato", "Registrazione effettuata con successo!", "OK");
                // Esempio: navigare alla pagina di Login
                await Shell.Current.GoToAsync("//Login");
            }
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            // Login
            await Shell.Current.GoToAsync("//Login");
        }
    }
}
