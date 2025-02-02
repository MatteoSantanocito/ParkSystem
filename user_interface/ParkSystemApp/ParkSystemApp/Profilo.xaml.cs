using Microsoft.Maui.Storage; // per SecureStorage, Preferences
using ParkSystemApp.Services;
namespace ParkSystemApp
{
    public partial class Profilo : ContentPage
    {
        public Profilo()
        {
            InitializeComponent();
            CaricaDatiUtente();
        }

        private void CaricaDatiUtente()
        {
            // Recupera i dati da Preferences (o come li hai salvati)
            var nome = Preferences.Get("UserName", "Nessun Nome");
            var cognome = Preferences.Get("UserCognome", "Nessun Cognome");
            var email = Preferences.Get("UserEmail", "Nessuna Email");
            var tipoAvv = Preferences.Get("UserTipoAvventura", "Nessun tipo Avventura");

            
            WelcomeLabel.Text = $"Benvenuto, {nome}";
            NomeValLabel.Text = nome;
            CognomeValLabel.Text = cognome;
            EmailLabel.Text = email;
            TipoAvvLabel.Text = tipoAvv;
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            // Svuota token e preferenze
            await SecureStorage.SetAsync("AuthToken", "");
            Preferences.Clear();

            // Naviga a Login (percorso assoluto Shell)
            await Shell.Current.GoToAsync("//Login");
        }

        private async void OnBackToMainPageClicked(Object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"//{nameof(MainPage)}", false);
        }

        private async void OnChangeEmailClicked(object sender, EventArgs e)
        {
            // Naviga a pagina cambio email
            await Shell.Current.GoToAsync("ChangeEmailPage");
        }

        private async void OnChangePasswordClicked(object sender, EventArgs e)
        {
            // Naviga a pagina cambio password
            await Shell.Current.GoToAsync("ChangePasswordPage");
        }

        private async void OnEliminaAccountClicked(object sender, EventArgs e)
        {
            bool conferma1 = await DisplayAlert("Conferma", "Sei sicuro di voler eliminare l'account?", "Sì", "No");
            if (!conferma1) return;

            bool conferma2 = await DisplayAlert("Conferma definitiva", "L'operazione è irreversibile. Eliminare?", "Sì", "No");
            if (!conferma2) return;

            var api = new ApiService();
            var result = await api.DeleteAccountAsync();
            if (result.StartsWith("Errore"))
            {
                await DisplayAlert("Errore", result, "OK");
            }
            else
            {
                // Logout forzato
                await SecureStorage.SetAsync("AuthToken", "");
                Preferences.Clear();
                
                await DisplayAlert("Ok", "Account eliminato correttamente", "OK");
                // Torno a Login
                await Shell.Current.GoToAsync("//Login");
            }
        }
    }
}
