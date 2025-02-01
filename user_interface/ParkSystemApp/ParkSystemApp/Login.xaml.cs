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
                // Debug
                System.Diagnostics.Debug.WriteLine($"emailEntry={emailEntry.Text}, passwordText={passwordText.Text}");
                
                string email = emailEntry.Text;
                string password = passwordText.Text;

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    await DisplayAlert("Errore", "Compila tutti i campi", "OK");
                    return;
                }

                // Esegui la chiamata all'endpoint di login
                var result = await _apiService.LoginAsync(email, password);

                // Se la stringa ritornata inizia con "Errore", mostriamo l'errore
                if (result.StartsWith("Errore"))
                {
                    await DisplayAlert("Errore", result, "OK");
                    return;
                }

                // Altrimenti significa che il login è riuscito e abbiamo un token
                await DisplayAlert("Successo", "Login effettuato con successo", "OK");

                // Navigazione usando rotta “assoluta” di Shell: //MainPage
                await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
            }


        private async void Button_Clicked(object sender, EventArgs e)
        {
            // Naviga alla pagina di registrazione
            await Shell.Current.GoToAsync(nameof(Registrazione));
        }
    }
}
