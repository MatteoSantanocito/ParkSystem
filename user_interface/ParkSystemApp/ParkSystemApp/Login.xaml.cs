using System;
using Microsoft.Maui.Controls;
using ParkSystemApp.Services;

namespace ParkSystemApp
{
    public partial class Login : ContentPage
    {
        private readonly ApiService _apiService;

        public Login()
        {
            InitializeComponent();
            _apiService = new ApiService();
        }

        private async void Button_Login_Clicked(object sender, EventArgs e)
        {
            
            string username = email.Text;
            string password = passwordText.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                await DisplayAlert("Errore", "Compila tutti i campi", "OK");
                return;
            }

            // Esegui la chiamata all'endpoint di login
            var result = await _apiService.LoginAsync(username, password);

            if (result.StartsWith("Errore"))
            {
                // Se la stringa inizia con "Errore", mostriamo l'errore
                await DisplayAlert("Errore", result, "OK");
            }
            else
            {
                if (!string.IsNullOrEmpty(result))
                {
                    // Token ricevuto correttamente
                    await DisplayAlert("Successo", "Login effettuato con successo", "OK");

                    // Se stai usando Shell, naviga alla MainPage
                    await Shell.Current.GoToAsync("//Mainpage");
                }
                else
                {
                    await DisplayAlert("Errore", "Token non ricevuto, login non riuscito", "OK");
                }
            }
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            // Naviga alla pagina di registrazione
            await Shell.Current.GoToAsync(nameof(Registrazione));
        }
    }
}
