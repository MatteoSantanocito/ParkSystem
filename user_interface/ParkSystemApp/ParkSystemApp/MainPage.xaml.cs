using System.Collections.ObjectModel;
using ParkSystemApp.Models;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using ParkSystemApp.Services;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;

namespace ParkSystemApp
{
    public partial class MainPage : ContentPage
    {
        private WebSocketClient _webSocketClient = new WebSocketClient();
        private ApiService _apiService; // Aggiungi una dipendenza al tuo ApiService
        public ObservableCollection<Attrazione> Attrazioni { get; set; }
        public Command<Attrazione> OpenDetailCommand { get; private set; }

        public MainPage()
        {
            InitializeComponent();
            Attrazioni = new ObservableCollection<Attrazione>();
            BindingContext = this;
            BindingContext = this;
            _apiService = new ApiService(); // Inizializza ApiService
            CaricaDati();
            OpenDetailCommand = new Command<Attrazione>(ExecuteOpenDetailCommand);
            ConnectWebSocket();

        }

        // Metodo per richiedere la possibilita di mandare notifiche push
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.PostNotifications>();
            }
        }

        private async void ConnectWebSocket()
        {
            // Assicurati di usare l'URL corretto per l'emulatore
            var uri = new Uri("ws://10.0.2.2:8080/ws");

            await _webSocketClient.ConnectAsync(uri);
        }

        private async void ExecuteOpenDetailCommand(Attrazione attrazione)
        {
            var navigationParameters = new Dictionary<string, object>
        {
            { "attrazione", attrazione }
        };
            await Shell.Current.GoToAsync(nameof(AttrazionePage), navigationParameters);
        }


        private async void CaricaDati()
        {
            try
            {
                string tipoUtente = Preferences.Get("UserType", "defaultType");
                var attrazioni = await _apiService.GetAttrazioniAsync(tipoUtente); // Usa il tuo ApiService per ottenere i dati
                foreach (var attrazione in attrazioni)
                {
                    Attrazioni.Add(attrazione);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Errore", $"Non è stato possibile recuperare i dati: {ex.Message}", "OK");
            }

        }

        private async void OnProfileClicked(object sender, EventArgs e)
        {
            // Naviga alla pagina di registrazione
            await Shell.Current.GoToAsync(nameof(Profilo), false);
        }

        private async void OnFriendshipClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("///FriendshipPage");
        }




    }



}

