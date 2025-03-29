using Microsoft.Maui.Storage;

namespace ParkSystemApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Registrazione delle rotte
            Routing.RegisterRoute(nameof(Registrazione), typeof(Registrazione));
            Routing.RegisterRoute(nameof(Login), typeof(Login));
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(AttrazionePage), typeof(AttrazionePage));

            Routing.RegisterRoute("Profilo", typeof(Profilo));
            Routing.RegisterRoute("FriendshipPage", typeof(FriendshipPage));
            Routing.RegisterRoute("ChangeEmailPage", typeof(ChangeEmailPage));
            Routing.RegisterRoute("ChangePasswordPage", typeof(ChangePasswordPage));
            
            // Eseguiamo la verifica del token
            CheckTokenAndNavigate();
        }

        private async void CheckTokenAndNavigate()
        {
            // Se hai già un token salvato (quindi utente “loggato”)
            var token = await SecureStorage.GetAsync("AuthToken");
            //SecureStorage è consigliato per il token JWT, perché è più protetto. 
            //I dati “pubblici” (nome, cognome, email) stanno in Preferences, che sono più 
            //facili da leggere/modificare ma anche più semplici per l’uso quotidiano.
            if (!string.IsNullOrEmpty(token))
            {
                // Naviga direttamente a MainPage
                await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
            }
            else
            {
                // Altrimenti, obblighiamo a passare dal Login
                await Shell.Current.GoToAsync($"//{nameof(Login)}");
            }
        }
    }
}
